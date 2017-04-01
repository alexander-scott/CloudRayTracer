using System;
using System.Collections;
using System.Collections.Generic;
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

        private int meshTotal;

        // Use this for initialization
        void Start()
        {
            client = new Client();
            client.PersistConnection = true;

            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnConnectFailed += Client_OnConnectFailed;

            client.Connection.OnDataCompletelyReceived += Connection_OnDataCompletelyReceived;
            client.Connection.OnFrameChanged += Connection_OnFrameChanged;
            client.Connection.OnTransmissionPreparation += Connection_OnTransmissionPreparation;
        }

        private void Update()
        {
            if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
            {
                foreach (NetworkedObject netObj in DataController.Instance.networkedObjectDictionary.Values)
                {
                    netObj.ClientUpdate();
                }
            }
        }

        #region Connection

        public void ConnectToServer()
        {
            client.Connect(DataController.Instance.ipAddress, 7777);
        }

        private void Client_OnConnectFailed()
        {
            MenuController.Instance.UpdateSubTitleText("Failed to connect to the server");
        }

        private void Client_OnDisconnected(byte disconnectMsg)
        {
            MenuController.Instance.UpdateSubTitleText("Disconnected from the server");
            DataController.Instance.aiMovement = false;
        }

        private void Client_OnConnected()
        {
            MenuController.Instance.UpdateSubTitleText("Synchronizing objects");
            DataController.Instance.applicationState = DataController.ApplicationState.ClientSynchronising;
            Timing.RunCoroutine(SyncObjects(), "SynchronizingObjects");
        }

        private void OnFinishedSync()
        {
            Debug.Log(DataController.Instance.networkedObjectDictionary.Count + " objects synced");
            SendPacket(DataController.PacketType.FinishedSyncing, "");
            MenuController.Instance.OnClientConnected();
        }

        #endregion

        #region Raytracing

        private void Connection_OnTransmissionPreparation(int meshCount)
        {
            meshTotal = meshCount;
        }

        private void Connection_OnFrameChanged()
        {
            //throw new NotImplementedException();
        }

        private void Connection_OnDataCompletelyReceived(int transmissionID, int meshCount, byte[] mesh)
        {
            // Deserialize data back to a mesh
            Mesh newMesh = MeshSerializer.ReadMesh(mesh, true);

            RayTraceController.Instance.RecieveMesh(meshCount, meshTotal, newMesh);
        }

        public void StartRayTracer()
        {
            SendPacket(DataController.PacketType.UpdateNetworkSendRate, DataController.Instance.meshSendRate.ToString());
            SendPacket(DataController.PacketType.UpdateRayTracerGap, DataController.Instance.rayTracerGap.ToString());
            SendPacket(DataController.PacketType.UpdateNetworkSendRate, DataController.Instance.networkedObjectSendRate.ToString());
            SendPacket(DataController.PacketType.StartRayTracer, true.ToString());
        }

        public void StopRayTracer()
        {
            SendPacket(DataController.PacketType.StopRayTracer, true.ToString());
        }

        #endregion

        #region Sync

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

        private IEnumerator<float> SyncObjects()
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

        public IEnumerator<float> SpawnCarsOnServer()
        {
            List<NetworkedObject> objectIDs = TrafficController.Instance.SpawnCarsClient();

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
    }
}
