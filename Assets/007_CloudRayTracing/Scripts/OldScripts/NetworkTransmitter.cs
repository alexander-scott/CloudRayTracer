using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BMW.Verification.CloudRayTracing
{
    /// <summary>
    /// Used to send a large number of bytes from server to client by splitting it up into smaller chunks.
    /// </summary>
    public class NetworkTransmitter : NetworkBehaviour
    {
        private static readonly string LOG_PREFIX = "[" + typeof(NetworkTransmitter).Name + "]: ";
        public const int RELIABLE_SEQUENCED_CHANNEL = (int)QosType.UnreliableFragmented; // Quality of service. Ensure it is declared in network manager
        private static int defaultBufferSize = 1300; // Max ethernet MTU is ~1400

        private class TransmissionData
        {
            public int curDataIndex; // Current position in the array of data already received.
            public byte[] data;

            public TransmissionData(byte[] _data)
            {
                curDataIndex = 0;
                data = _data;
            }
        }

        // List of transmissions currently going on. A transmission id is used to uniquely identify to which transmission a received byte[] belongs to.
        List<int> serverTransmissionIds = new List<int>();

        // Maps the transmission id to the data being received.
        Dictionary<int, TransmissionData> clientTransmissionData = new Dictionary<int, TransmissionData>();

        // Callbacks which are invoked on the respective events. int = transmissionId. byte[] = data sent or received.
        public event UnityAction<int, byte[]> OnDataComepletelySent;
        public event UnityAction<int, byte[]> OnDataFragmentSent;
        public event UnityAction<int, byte[]> OnDataFragmentReceived;
        public event UnityAction<int, byte[]> OnDataCompletelyReceived;

        /// <summary>
        /// Only accessible from the SERVER. Sends an array of bytes back to the client.
        /// </summary>
        /// <param name="transmissionId">The ID of this tramission. A number which currently isn't in use by another transmission.</param>
        /// <param name="data">The array of bytes to send to the client</param>
        [Server]
        public void SendBytesToClients(int transmissionId, byte[] data)
        {
            Debug.Assert(!serverTransmissionIds.Contains(transmissionId));
            StartCoroutine(SendBytesToClientsRoutine(transmissionId, data));
        }

        /// <summary>
        /// Only accessible from the SERVER. The coroutine that actually does the sending.
        /// </summary>
        /// <param name="transmissionId">The ID of this tramission. A number which currently isn't in use by another transmission.</param>
        /// <param name="data">The array of bytes to send to the client</param>
        /// <returns></returns>
        [Server]
        public IEnumerator SendBytesToClientsRoutine(int transmissionId, byte[] data)
        {
            Debug.Assert(!serverTransmissionIds.Contains(transmissionId));
            Debug.Log(LOG_PREFIX + "SendBytesToClients processId=" + transmissionId + " | datasize=" + data.Length);

            // Tell client that he is going to receive some data and tell him how much it will be.
            RpcPrepareToReceiveBytes(transmissionId, data.Length);
            yield return null;

            // Begin transmission of data. send chunks of 'bufferSize' until completely transmitted.
            serverTransmissionIds.Add(transmissionId);
            TransmissionData dataToTransmit = new TransmissionData(data);
            int bufferSize = defaultBufferSize;

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
                RpcReceiveBytes(transmissionId, buffer);
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

        /// <summary>
        /// Only accessible by the SERVER but is executed on the CLIENT. Lets the client know to expect some data.
        /// </summary>
        /// <param name="transmissionId">The ID of the transmission</param>
        /// <param name="expectedSize">The size of the data</param>
        [ClientRpc]
        private void RpcPrepareToReceiveBytes(int transmissionId, int expectedSize)
        {
            if (clientTransmissionData.ContainsKey(transmissionId))
                return;

            // Prepare data array which will be filled chunk by chunk by the received data
            TransmissionData receivingData = new TransmissionData(new byte[expectedSize]);
            clientTransmissionData.Add(transmissionId, receivingData);
        }

        /// <summary>
        /// Only accessible by the SERVER but is executed on the CLIENT. Recieves the chunk of bytes from the server.
        /// Use reliable sequenced channel to ensure bytes are sent in correct order.
        /// </summary>
        /// <param name="transmissionId">The ID of the transmission</param>
        /// <param name="recBuffer">The buffer containing a chunk of bytes.</param>
        [ClientRpc(channel = RELIABLE_SEQUENCED_CHANNEL)]
        private void RpcReceiveBytes(int transmissionId, byte[] recBuffer)
        {
            // Already completely received or not prepared?
            if (!clientTransmissionData.ContainsKey(transmissionId))
                return;

            // Copy received data into prepared array and remember current dataposition
            TransmissionData dataToReceive = clientTransmissionData[transmissionId];
            System.Array.Copy(recBuffer, 0, dataToReceive.data, dataToReceive.curDataIndex, recBuffer.Length);
            dataToReceive.curDataIndex += recBuffer.Length;

            if (null != OnDataFragmentReceived)
                OnDataFragmentReceived(transmissionId, recBuffer);

            if (dataToReceive.curDataIndex < dataToReceive.data.Length - 1)
                // Current data not completely received
                return;

            // Current data completely received
            Debug.Log(LOG_PREFIX + "Completely Received Data at transmissionId=" + transmissionId);
            clientTransmissionData.Remove(transmissionId);

            if (null != OnDataCompletelyReceived)
                OnDataCompletelyReceived.Invoke(transmissionId, dataToReceive.data);
        }
    }
}