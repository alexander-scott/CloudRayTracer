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
            MenuController.Instance.stopRaytracerButton.onClick.AddListener(StopRayTracer);
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

        }

        public void OnApplicationQuit()
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
        }

        private void Connection_OnDataCompletelyReceived(int arg0, byte[] mesh)
        {
            // Deserialize data back to a mesh
            Mesh newMesh = MeshSerializer.ReadMesh(mesh, true);

            //pointCloud.GetComponent<MeshFilter>().mesh = newMesh;
        }

        private void StartRayTracer()
        {
            SendPacket(DataController.PacketType.StartRayTracer, true.ToString());
        }

        private void StopRayTracer()
        {
            SendPacket(DataController.PacketType.StopRayTracer, true.ToString());
            //pointCloud.GetComponent<MeshFilter>().mesh = null;
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
            MenuController.Instance.OnClientConnected();
        }
    }
}
