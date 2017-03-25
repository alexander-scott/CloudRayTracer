
using System;
using NetworkScopes;
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

        [Signal]
        public void Authenticate(string userName, string passwordHash)
        {
            // process the user's credentials - for now we're just checking for null/empty string to keep it simple for this example
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passwordHash))
            {
                // user authenticated successfully - assign player name within this peer's ExamplePeer instance
                SenderPeer.SetAuthenticated(userName);

                // ..then send peer to the Lobby scope
                //HandoverPeer(SenderPeer, ExampleServer.Lobby);
            }
            // if they couldn't be authenticated, we might as well disconnect them
            else
            {
                // TODO: we might want to send a disconnection reason rather than just closing the connection
                SenderPeer.Disconnect();
            }
        }

        #region Send to client

        public void SendPacket(int packetNum, string contents)
        {
            SendToPeer(SenderPeer).RecievePacket(packetNum, contents);
        }

        public void SendPerformanceDictionary(Peer peer, int performanceType, float performanceVal)
        {
            SendToPeer(peer).RecieveServerPerformanceDictionary(performanceType, performanceVal);
        }

        #endregion

        #region Recieve from clients

        [Signal]
        public void RecieveObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            ServerController.Instance.UpdateObjectPosition(oldKey, position, rotation, localScale);
        }

        [Signal]
        public void RecievePacket(int packetNum, string contents)
        {
            ServerController.Instance.PacketRecieved((DataController.PacketType)packetNum, contents);
        }

        #endregion

        #region Network Transmitter

        public IEnumerator SendBytesToClientsRoutine(int transmissionId, byte[] data)
        {
            Debug.Assert(!serverTransmissionIds.Contains(transmissionId));
            Debug.Log("SendBytesToClients processId=" + transmissionId + " | datasize=" + data.Length);

            // Tell client that he is going to receive some data and tell him how much it will be.
            SendToPeer(SenderPeer).ClientPrepareToRecieveTransmission(transmissionId, data.Length);
            yield return null;

            // Begin transmission of data. send chunks of 'bufferSize' until completely transmitted.
            serverTransmissionIds.Add(transmissionId);
            DataController.TransmissionData dataToTransmit = new DataController.TransmissionData(data);
            int bufferSize = DataController.Instance.defaultBufferSize;

            while (dataToTransmit.curDataIndex < dataToTransmit.data.Length - 1)
            {
                // Determine the remaining amount of bytes, still need to be sent.
                int remaining = dataToTransmit.data.Length - dataToTransmit.curDataIndex;
                if (remaining < bufferSize)
                    bufferSize = remaining;

                // Prepare the chunk of data which will be sent in this iteration
                byte[] buffer = new byte[bufferSize];
                System.Array.Copy(dataToTransmit.data, dataToTransmit.curDataIndex, buffer, 0, bufferSize);

                // Send the chunk
                SendToPeer(SenderPeer).ClientRecieveTransmission(transmissionId, buffer);
                dataToTransmit.curDataIndex += bufferSize;

                yield return null;

                if (null != OnDataFragmentSent)
                    OnDataFragmentSent.Invoke(transmissionId, buffer);
            }

            // Transmission complete.
            serverTransmissionIds.Remove(transmissionId);

            if (null != OnDataComepletelySent)
                OnDataComepletelySent.Invoke(transmissionId, dataToTransmit.data);
        }

        #endregion
    }
}