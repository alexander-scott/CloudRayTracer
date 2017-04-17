using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientController : MonoBehaviour
    {
        #region Singleton

        private static ClientController _instance;

        public static ClientController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        #endregion

        private Client client;

        private Vector3 transmissionCentralCarPos;

        // Use this for initialization
        void Start()
        {
            client = new Client();
            client.PersistConnection = true;

            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnConnectFailed += Client_OnConnectFailed;

            client.Connection.OnDataCompletelyReceived += Connection_OnDataCompletelyReceived;
            client.Connection.OnTransmissionPreparation += Connection_OnTransmissionPreparation;
        }

        private void Update()
        {
            // Only update after objects have finished syncing and our app state is in client mode
            if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
            {
                foreach (NetworkedObject netObj in DataController.Instance.networkedObjectDictionary.Values)
                {
                    netObj.ClientUpdate();
                }
            }
        }

        #region Connection

        public void ConnectToServer() // Called from MenuController when the user clicks the connect to server button
        {
            client.Connect(DataController.Instance.ipAddress, 7777);
        }

        private void Client_OnConnectFailed() // Called when connection attempt timed out
        {
            MenuController.Instance.UpdateSubTitleText("Failed to connect to the server");
        }

        private void Client_OnDisconnected(byte disconnectMsg) // Called when client disconnected from server
        {
            MenuController.Instance.UpdateSubTitleText("Disconnected from the server. Restarting...");

            Timing.RunCoroutine(RestartDelay(3f));
        }

        private IEnumerator<float> RestartDelay(float delay)
        {
            yield return Timing.WaitForSeconds(delay);

            UnityEngine.SceneManagement.SceneManager.LoadScene("CloudRayTracer");
        }

        private void Client_OnConnected() // Called when we have a successfull connection to the server
        {
            MenuController.Instance.UpdateSubTitleText("Synchronizing objects");
            DataController.Instance.applicationState = DataController.ApplicationState.ClientSynchronising;

            Timing.RunCoroutine(SyncObjects(), "SynchronizingObjects"); // Start syncing networked objects
        }

        private void OnFinishedSync() // Called when the SyncObjects() coroutine finishes
        {
            Debug.Log(DataController.Instance.networkedObjectDictionary.Count + " objects synced"); // Total number of objects synced
            SendPacket(DataController.PacketType.FinishedSyncing, ""); // Tell the server we have finished syncing (debug purposes)

            DataController.Instance.applicationState = DataController.ApplicationState.Client;

            MenuController.Instance.OnClientConnected();

            SendPacket(DataController.PacketType.UpdateCentralCar, DataController.Instance.centralCar.GetComponent<NetworkedObject>().objectID.ToString());
            SendPacket(DataController.PacketType.UpdateHitPositionsSendRate, DataController.Instance.hitPositionsSendRate.ToString());
            SendPacket(DataController.PacketType.UpdateRayTracerGap, DataController.Instance.rayTracerGap.ToString());
            SendPacket(DataController.PacketType.UpdateNetworkedObjectSendRate, DataController.Instance.networkedObjectSendRate.ToString());
            SendPacket(DataController.PacketType.UpdatePointCloudPointSize, DataController.Instance.pointCloudPointSize.ToString());

            for (int i = 0; i < Enum.GetNames(typeof(DataController.SensorType)).Length; i++)
            {
                if (DataController.Instance.activeSensors[(DataController.SensorType)i])
                {
                    SendPacket(DataController.PacketType.SetSensorEnabled, (i).ToString());
                }
                else
                {
                    SendPacket(DataController.PacketType.SetSensorDisabled, (i).ToString());
                }
            }
        }

        #endregion

        #region Raytracing

        private void Connection_OnTransmissionPreparation(Vector3 centralCarPos) // Called when we are told to prepare to recieve a new array
        {
            transmissionCentralCarPos = centralCarPos;
        }

        private void Connection_OnDataCompletelyReceived(int transmissionID, byte[] data) // Called when we have recieved the entire array
        {
            Vector3[] hitPositions = BytesToVectors(data);
            PointCloudController.Instance.UpdatePositions(hitPositions, transmissionCentralCarPos);
        }

        public void StartRayTracer() // Called from MenuController when the start raytracing toggle is clicked
        {
            SendPacket(DataController.PacketType.StartRayTracer, true.ToString());

            PointCloudController.Instance.StartRendering();
        }

        public void StopRayTracer() // Called from MenuController when the stop raytracing toggle is clicked
        {
            SendPacket(DataController.PacketType.StopRayTracer, true.ToString());
            PointCloudController.Instance.StopRendering();
        }

        #endregion

        #region Sync objects

        public void UpdateObjectPositionOnServer(int objectID, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (client.IsConnected)
            {
                client.Connection.UpdateObjectPosition(objectID, position, rotation, localScale);
            }
        }

        public void UpdateObjectState(int objectID, bool active)
        {
            if (client.IsConnected)
            {
                client.Connection.UpdateObjectState(objectID, active);
            }
        }

        public void UpdateObjectStateAndPosition(int objectID, bool active, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (client.IsConnected)
            {
                client.Connection.UpdateObjectState(objectID, active, position, rotation, localScale);
            }
        }

        private IEnumerator<float> SyncObjects() // Syncs all objects to the server
        {
            yield return Timing.WaitForSeconds(0.1f); // Add a slight delay

            List<NetworkedObject> objectIDs = TrafficController.Instance.SpawnCarsClient(); // Spawn them on the client first

            DataController.Instance.centralCar = TrafficController.Instance.trafficCars[0].GetComponent<CarController>();
            DataController.Instance.centralCar.isFocusCar = true;

            SensorManager.Instance.transform.parent = DataController.Instance.centralCar.transform;
            SensorManager.Instance.transform.localPosition = Vector3.zero;

            for (int i = 0; i < objectIDs.Count; i++)
            {
                if (Vector3.Distance(objectIDs[i].transform.position, DataController.Instance.centralCar.transform.position) < DataController.Instance.updateDistance)
                {
                    objectIDs[i].active = true;
                }

                client.Connection.SpawnCarOnServer(objectIDs[i].objectID, objectIDs[i].active);
            }

            foreach (NetworkedObject netObj in DataController.Instance.networkedObjectDictionary.Values)
            {
                if (Vector3.Distance(netObj.transform.position, DataController.Instance.centralCar.transform.position) < DataController.Instance.updateDistance)
                {
                    netObj.active = true;

                    // SET ACTIVE ON SERVER
                    client.Connection.UpdateObjectState(netObj.objectID, true);
                }
            }

            OnFinishedSync();
        }

        #endregion  

        public void SendPacket(DataController.PacketType packetType, string contents)
        {
            client.Connection.SendPacket((int)packetType, contents);
        }

        public void PacketRecieved(DataController.PacketType packetType, string contents)
        {

        }

        public void OnApplicationQuit()
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
        }

        private Vector3[] BytesToVectors(byte[] bytes)
        {
            Vector3[] hitPositions = new Vector3[bytes.Length / (sizeof(float) * 3)];

            int buffIndex = 0;

            for (int i = 0; i < bytes.Length / (sizeof(float) * 3); i++)
            {
                float x = BitConverter.ToSingle(bytes, buffIndex);
                buffIndex += sizeof(float);
                float y = BitConverter.ToSingle(bytes, buffIndex);
                buffIndex += sizeof(float);
                float z = BitConverter.ToSingle(bytes, buffIndex);
                buffIndex += sizeof(float);

                hitPositions[i] = new Vector3(x, y, z);
            }

            return hitPositions;
        }
    }
}
