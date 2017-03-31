
using NetworkScopes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BMW.Verification.CloudRayTracing
{
    [Scope(typeof(ServerConnection))]
    public partial class ClientConnection : ClientScope
    {
        // Maps the transmission id to the data being received.
        private Dictionary<int, DataController.TransmissionData> clientTransmissionData = new Dictionary<int, DataController.TransmissionData>();

        public event UnityAction<int> OnTransmissionPreparation;
        public event UnityAction<int, byte[]> OnDataFragmentReceived;
        public event UnityAction<int, int, byte[]> OnDataCompletelyReceived;
        public event UnityAction OnFrameChanged;

        private int currentFrameCount; 

        //protected override void OnEnterScope()
        //{
        //    //SendToServer.Authenticate("sour", "testpw");
        //    Debug.Log("Scope entered");
        //    base.OnEnterScope();
        //}

        #region Send to server

        public void SendPacket(int packetNum, string contents)
        {
            SendToServer.RecievePacket(packetNum, contents);
        }

        public void UpdateObjectPosition(int oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            SendToServer.RecieveObjectPosition(oldKey, position, rotation, localScale);
        }

        public void SpawnCarOnServer(int objectID)
        {
            SendToServer.RecieveNewObjectSpawnID(objectID);
        }

        #endregion

        #region Recieve from server

        [Signal]
        public void RecievePacket(int packetNum, string contents)
        {
            ClientController.Instance.PacketRecieved((DataController.PacketType)packetNum, contents);
        }

        [Signal]
        public void RecieveServerPerformanceDictionary(int performanceType, float performanceVal)
        {
            DataController.Instance.performanceDictionary[(DataController.StatisticType)performanceType] = performanceVal;
        }

        #endregion

        #region Network transmitter

        [Signal]
        public void ClientPrepareToRecieveTransmission(int transmissionId, int expectedSize, int frameCount,  int meshTotal)
        {
            if (clientTransmissionData.ContainsKey(transmissionId))
                return;

            if (currentFrameCount != frameCount)
            {
                currentFrameCount = frameCount;

                if (null != OnFrameChanged)
                    OnFrameChanged.Invoke();
            }

            if (null != OnTransmissionPreparation)
                OnTransmissionPreparation.Invoke(meshTotal);

            // Prepare data array which will be filled chunk by chunk by the received data
            DataController.TransmissionData receivingData = new DataController.TransmissionData(new byte[expectedSize]);
            clientTransmissionData.Add(transmissionId, receivingData);
        }

        [Signal]
        public void ClientRecieveTransmission(int transmissionId, int meshCount, byte[] recBuffer)
        {
            // Already completely received or not prepared?
            if (!clientTransmissionData.ContainsKey(transmissionId))
                return;

            // Copy received data into prepared array and remember current dataposition
            DataController.TransmissionData dataToReceive = clientTransmissionData[transmissionId];
            System.Array.Copy(recBuffer, 0, dataToReceive.data, dataToReceive.curDataIndex, recBuffer.Length);
            dataToReceive.curDataIndex += recBuffer.Length;

            if (null != OnDataFragmentReceived)
                OnDataFragmentReceived(transmissionId, recBuffer);

            if (dataToReceive.curDataIndex < dataToReceive.data.Length - 1)
                // Current data not completely received
                return;

            // Current data completely received
            Debug.Log("Completely Received Data at transmissionId=" + transmissionId);
            clientTransmissionData.Remove(transmissionId);

            if (null != OnDataCompletelyReceived)
                OnDataCompletelyReceived.Invoke(transmissionId, meshCount, dataToReceive.data);
        }

        #endregion
    }
}
