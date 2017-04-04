
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

        public event UnityAction OnTransmissionPreparation;
        public event UnityAction<int, byte[]> OnDataFragmentReceived;
        public event UnityAction<int, byte[]> OnDataCompletelyReceived;

        private int currentFrameCount; 

        #region Send to server

        public void SendPacket(int packetNum, string contents)
        {
            SendToServer.RecievePacket(packetNum, contents);
        }

        public void UpdateObjectPosition(int objectID, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            SendToServer.RecieveObjectPosition(objectID, position, rotation, localScale);
        }

        public void UpdateObjectState(int objectID, bool active)
        {
            SendToServer.RecieveObjectState(objectID, active);
        }

        public void UpdateObjectState(int objectID, bool active, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            SendToServer.RecieveObjectStateAndPosition(objectID, active, position, rotation, localScale);
        }

        public void SpawnCarOnServer(int objectID, bool active)
        {
            SendToServer.RecieveNewCarSpawn(objectID, active);
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
        public void ClientPrepareToRecieveTransmission(int transmissionId, int expectedSize)
        {
            if (clientTransmissionData.ContainsKey(transmissionId))
                return;

            if (null != OnTransmissionPreparation)
                OnTransmissionPreparation.Invoke();

            // Prepare data array which will be filled chunk by chunk by the received data
            DataController.TransmissionData receivingData = new DataController.TransmissionData(new byte[expectedSize]);
            clientTransmissionData.Add(transmissionId, receivingData);
        }

        [Signal]
        public void ClientRecieveTransmission(int transmissionId, byte[] recBuffer)
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
                OnDataCompletelyReceived.Invoke(transmissionId, dataToReceive.data);
        }

        #endregion
    }
}
