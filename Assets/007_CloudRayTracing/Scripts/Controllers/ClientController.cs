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

        public GameObject pointCloud;

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

            MenuController.Instance.startRaytracerButton.onClick.AddListener(StartRayTracer);
        }

        public void ConnectToServer()
        {
            client.Connect(DataController.Instance.ipAddress, 7777);
        }

        public void UpdateObjectPositionOnServer(Vector3 oldkey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (client.IsConnected)
            {
                client.Connection.UpdateObjectPosition(oldkey, position, rotation, localScale);
            }   
        }

        public void SendPacket(DataController.PacketType packetType, string contents)
        {
            client.Connection.SendPacket((int)packetType, contents); 
        }

        public void PacketRecieved(DataController.PacketType packetType, string contents)
        {  
            switch (packetType)
            {
                case DataController.PacketType.ToggleRaytracer:
                    // DO SOMETHING
                    break;
            }
        }

        private void Connection_OnDataCompletelyReceived(int arg0, byte[] mesh)
        {
            // Deserialize data back to a mesh
            Mesh newMesh = MeshSerializer.ReadMesh(mesh, true);

            pointCloud.GetComponent<MeshFilter>().mesh = newMesh;
        }

        private void StartRayTracer()
        {
            SendPacket(DataController.PacketType.ToggleRaytracer, true.ToString());
        }

        private void Client_OnConnectFailed()
        {
            MenuController.Instance.UpdateSubTitleText("Failed to connect to the server");
        }

        private void Client_OnDisconnected(byte disconnectMsg)
        {
            MenuController.Instance.UpdateSubTitleText("Disconnected from the server");
        }

        private void Client_OnConnected()
        {
            Debug.Log("Connected");
            Destroy(ServerController.Instance); Destroy(HostController.Instance);

            MenuController.Instance.UpdateSubTitleText("You are the CLIENT");
            MenuController.Instance.menuCanvas.SetActive(false);
            MenuController.Instance.clientCanvas.SetActive(true);
        }
    }
}
