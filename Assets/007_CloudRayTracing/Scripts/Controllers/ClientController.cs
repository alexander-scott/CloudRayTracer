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

        public Client client;

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
            MenuController.Instance.UpdateSubTitleText("Disconnected from the server");
            DataController.Instance.aiMovement = false;
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
        }

        #endregion

        #region Raytracing

        private void Connection_OnTransmissionPreparation() // Called when we are told to prepare to recieve a new mesh
        {
            
        }

        private void Connection_OnDataCompletelyReceived(int transmissionID, byte[] data) // Called when we have recieved the entire mesh
        {
            Vector3[] hitPositions = DeserializeObject<Vector3[]>(data);
            PointCloudController.Instance.UpdatePositions(hitPositions);
        }

        public void StartRayTracer() // Called from MenuController when the start raytracing toggle is clicked
        {
            SendPacket(DataController.PacketType.UpdateNetworkSendRate, DataController.Instance.meshSendRate.ToString());
            SendPacket(DataController.PacketType.UpdateRayTracerGap, DataController.Instance.rayTracerGap.ToString());
            SendPacket(DataController.PacketType.UpdateNetworkSendRate, DataController.Instance.networkedObjectSendRate.ToString());
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
            foreach (NetworkedObject netObj in DataController.Instance.networkedObjectDictionary.Values)
            {
                if (Vector3.Distance(netObj.transform.position, DataController.Instance.centralCar.transform.position) < DataController.Instance.updateDistance)
                {
                    netObj.active = true;

                    // SET ACTIVE ON SERVER
                    client.Connection.UpdateObjectState(netObj.objectID, true);
                }

                yield return Timing.WaitForSeconds(0.05f);
            }

            Timing.RunCoroutine(SpawnCarsOnServer(), "SpawnCarsOnServer");
        }

        public IEnumerator<float> SpawnCarsOnServer() // Spawn our cars on the server
        {
            List<NetworkedObject> objectIDs = TrafficController.Instance.SpawnCarsClient(); // Spawn them on the client first

            for (int i = 0; i < objectIDs.Count; i++)
            {
                if (Vector3.Distance(objectIDs[i].transform.position, DataController.Instance.centralCar.transform.position) < DataController.Instance.updateDistance)
                {
                    objectIDs[i].active = true;
                }

                client.Connection.SpawnCarOnServer(objectIDs[i].objectID, objectIDs[i].active);

                yield return Timing.WaitForSeconds(0.05f);
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

        _T DeserializeObject<_T>(byte[] dataStream)
        {
            MemoryStream memStr = new MemoryStream(dataStream);
            memStr.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            bf.Binder = new VersionFixer();
            return (_T)bf.Deserialize(memStr);
        }
    }

    sealed class VersionFixer : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            // For each assemblyName/typeName that you want to deserialize to
            // a different type, set typeToDeserialize to the desired type.
            String assemVer1 = Assembly.GetExecutingAssembly().FullName;
            if (assemblyName != assemVer1)
            {
                // To use a type from a different assembly version, 
                // change the version number.
                // To do this, uncomment the following line of code.
                assemblyName = assemVer1;
                // To use a different type from the same assembly, 
                // change the type name.
            }
            // The following line of code returns the type.
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
            return typeToDeserialize;
        }
    }
}
