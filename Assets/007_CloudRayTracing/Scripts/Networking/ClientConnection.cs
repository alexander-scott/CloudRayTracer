
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
        Dictionary<int, GlobalVariables.TransmissionData> clientTransmissionData = new Dictionary<int, GlobalVariables.TransmissionData>();

        public event UnityAction<int, byte[]> OnDataFragmentReceived;
        public event UnityAction<int, byte[]> OnDataCompletelyReceived;

        protected override void OnEnterScope()
        {
            //SendToServer.Authenticate("sour", "testpw");
        }

        #region Send to server

        public void SendPacket(int packetNum, string contents)
        {
            SendToServer.RecievePacket(packetNum, contents);
        }

        public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            SendToServer.RecieveObjectPosition(oldKey, position, rotation, localScale);
        }

        #endregion

        #region Recieve from server

        [Signal]
        public void RecievePacket(int packetNum, string contents)
        {
            ClientController.Instance.PacketRecieved((GlobalVariables.PacketType)packetNum, contents);
        }

        #endregion

        #region Network transmitter

        [Signal]
        public void ClientPrepareToRecieveTransmission(int transmissionId, int expectedSize)
        {
            if (clientTransmissionData.ContainsKey(transmissionId))
                return;

            // Prepare data array which will be filled chunk by chunk by the received data
            GlobalVariables.TransmissionData receivingData = new GlobalVariables.TransmissionData(new byte[expectedSize]);
            clientTransmissionData.Add(transmissionId, receivingData);
        }

        [Signal]
        public void ClientRecieveTransmission(int transmissionId, byte[] recBuffer)
        {
            // Already completely received or not prepared?
            if (!clientTransmissionData.ContainsKey(transmissionId))
                return;

            // Copy received data into prepared array and remember current dataposition
            GlobalVariables.TransmissionData dataToReceive = clientTransmissionData[transmissionId];
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
