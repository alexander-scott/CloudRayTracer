
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

namespace BMW.Verification.CloudRayTracing
{
    [Scope(typeof(ClientConnection))]
    public partial class ServerConnection : ServerScope<Peer>
    {
        // List of transmissions currently going on. A transmission id is used to uniquely identify to which transmission a received byte[] belongs to.
        List<int> serverTransmissionIds = new List<int>();
        public event UnityAction<int, byte[]> OnDataComepletelySent;
        public event UnityAction<int, byte[]> OnDataFragmentSent;

        #region Send to client

        public void SendPacket(int packetNum, string contents)
        {
            SendToPeer(SenderPeer).RecievePacket(packetNum, contents);
        }

        public void SendPerformanceDictionary(int performanceType, float performanceVal)
        {
            if (SenderPeer != null)
            {
                SendToPeer(SenderPeer).RecieveServerPerformanceDictionary(performanceType, performanceVal);
            }
            else
            {
                Debug.Log("SENDER PEER IS NULL");
            }
        }

        #endregion

        #region Recieve from clients

        [Signal]
        public void RecieveObjectPosition(int objectID, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            ServerController.Instance.UpdateObjectPosition(objectID, position, rotation, localScale);
        }

        [Signal]
        public void RecieveObjectState(int objectID, bool active)
        {
            ServerController.Instance.UpdateObjectState(objectID, active);
        }

        [Signal]
        public void RecieveObjectStateAndPosition(int objectID, bool active, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            ServerController.Instance.UpdateObjectStateAndPosition(objectID, active, position, rotation, localScale);
        }

        [Signal]
        public void RecievePacket(int packetNum, string contents)
        {
            ServerController.Instance.PacketRecieved((DataController.PacketType)packetNum, contents);
        }

        [Signal]
        public void RecieveNewCarSpawn(int objectID, bool active)
        {
            TrafficController.Instance.SpawnCarServer(objectID, active);
        }

        #endregion

        #region Network Transmitter

        public IEnumerator<float> SendBytesToClientsRoutine(int transmissionId, byte[] data, Vector3 centralCarPos)
        {
            SendToPeer(SenderPeer).ClientPrepareToRecieveTransmission(transmissionId, data.Length, centralCarPos);
            yield return 0f;

            serverTransmissionIds.Add(transmissionId);
            DataController.TransmissionData dataToTransmit = new DataController.TransmissionData(data);
            int bufferSize = DataController.Instance.defaultBufferSize;

            while (dataToTransmit.curDataIndex < dataToTransmit.data.Length - 1)
            {
                int remaining = dataToTransmit.data.Length - dataToTransmit.curDataIndex;

                if (remaining < bufferSize)
                {
                    bufferSize = remaining;
                }
                    
                byte[] buffer = new byte[bufferSize];
                System.Array.Copy(dataToTransmit.data, dataToTransmit.curDataIndex, buffer, 0, bufferSize);

                SendToPeer(SenderPeer).ClientRecieveTransmission(transmissionId, buffer);
                dataToTransmit.curDataIndex += bufferSize;

                yield return 0f;

                if (null != OnDataFragmentSent)
                {
                    OnDataFragmentSent.Invoke(transmissionId, buffer);
                }    
            }

            // Transmission complete.
            serverTransmissionIds.Remove(transmissionId);

            if (null != OnDataComepletelySent)
            {
                OnDataComepletelySent.Invoke(transmissionId, dataToTransmit.data);
            }  
        }

        #endregion
    }
}