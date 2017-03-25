using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ServerController : MonoBehaviour
    {
        #region Singleton

        private static ServerController _instance;

        public static ServerController Instance { get { return _instance; } }

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

        private Server server;

        private float count = 0f;
        private float totalFps = 0f;

        private float minFPS = 60f;
        private float maxFPS = 60f;

        // Use this for initialization
        void Start()
        {
            server = new Server();

            server.OnPeerConnected += Server_OnPeerConnected;
        }

        public void StartServer()
        {
            server.StartServer(7777);

            MenuController.Instance.UpdateSubTitleText("You are the SERVER");
        }

        public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            GameObject go = ObjectManager.Instance.GetGameObject(oldKey);
            go.transform.position = position;
            go.transform.eulerAngles = rotation;
            go.transform.localScale = localScale;

            ObjectManager.Instance.UpdateKey(oldKey);
        }

        public void SendPacket(DataController.PacketType packetType, string contents)
        {
            server.Connection.SendPacket((int)packetType, contents);
        }

        public void PacketRecieved(DataController.PacketType packetType, string contents)
        {
            switch (packetType)
            {
                case DataController.PacketType.StartRayTracer:
                    Debug.Log("Raytrace start");
                    RayTraceController.Instance.StartRayTracing();
                    break;

                case DataController.PacketType.StopRayTracer:
                    Debug.Log("Raytrace stop");
                    RayTraceController.Instance.StopRayTracing();
                    break;
            }
        }

        public void SendSeralisedMeshToClient(int transmissionID, byte[] mesh)
        {
            // SPLIT UP ARRAY
            StartCoroutine(server.Connection.SendBytesToClientsRoutine(transmissionID, mesh));
        }

        private void Server_OnPeerConnected(Peer obj)
        {
            Debug.Log("Peer connected!");

            Timing.RunCoroutine(SendPerformanceData(), "SendPerformanceData");
        }

        private IEnumerator<float> SendPerformanceData()
        {
            while (server.NumberOfPeers != 0)
            {
                float fpsVal = 1.0f / Time.deltaTime;
                DataController.Instance.performanceDictionary[DataController.StatisticType.FPS] = Mathf.Floor(fpsVal);

                count++;
                totalFps += fpsVal;
                DataController.Instance.performanceDictionary[DataController.StatisticType.AVGFPS] = Mathf.Floor(totalFps / count);

                if (fpsVal < minFPS)
                {
                    minFPS = fpsVal;
                }

                if (fpsVal > maxFPS)
                {
                    maxFPS = fpsVal;
                }

                DataController.Instance.performanceDictionary[DataController.StatisticType.MINFPS] = Mathf.Floor(minFPS);
                DataController.Instance.performanceDictionary[DataController.StatisticType.MAXFPS] = Mathf.Floor(maxFPS);

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
