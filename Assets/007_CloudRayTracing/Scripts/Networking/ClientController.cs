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
        public GameObject clientCanvas;
        public Button startRaytracer;

        private Client client;

        // Use this for initialization
        void Start()
        {
            client = new Client();
            //client.PersistConnection = true;

            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnConnectFailed += Client_OnConnectFailed;

            startRaytracer.onClick.AddListener(StartRayTracer);
        }

        public void ConnectToServer()
        {
            GlobalVariables.isClient = true;
            GlobalVariables.activated = true;
            client.Connect(GlobalVariables.ipAddress, 7777);
        }

        public void UpdateObjectPositionOnServer(Vector3 oldkey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (client.IsConnected)
                client.Connection.UpdateObjectPosition(oldkey, position, rotation, localScale);
        }

        public void RenderMesh(byte[] mesh)
        {
            // Deserialize data back to a mesh
            Mesh newMesh = MeshSerializer.ReadMesh(mesh, true);

            pointCloud.GetComponent<MeshFilter>().mesh = newMesh;
        }

        public void SendPacket(GlobalVariables.PacketType packetType, string contents)
        {
            client.Connection.SendPacket((int)packetType, contents); 
        }

        public void PacketRecieved(GlobalVariables.PacketType packetType, string contents)
        {
            switch (packetType)
            {
                case GlobalVariables.PacketType.ToggleRaytracer:
                    // DO SOMETHING
                    break;
            }
        }

        private void StartRayTracer()
        {
            SendPacket(GlobalVariables.PacketType.ToggleRaytracer, true.ToString());
        }

        private void Client_OnConnectFailed()
        {
            UIManager.Instance.UpdateSubTitleText("Failed to connect to the server");
        }

        private void Client_OnDisconnected(byte disconnectMsg)
        {
            UIManager.Instance.UpdateSubTitleText("Disconnected from the server");
        }

        private void Client_OnConnected()
        {
            Debug.Log("Connected");
            UIManager.Instance.UpdateSubTitleText("You are the CLIENT");
            clientCanvas.SetActive(true);
        }
    }
}
