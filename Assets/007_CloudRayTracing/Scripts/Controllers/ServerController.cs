using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
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

        public void UpdateObjectPosition(int key, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(key))
            {
                GameObject go = DataController.Instance.networkedObjectDictionary[key];
                go.transform.position = position;
                go.transform.eulerAngles = rotation;
                go.transform.localScale = localScale;
            }
            else
            {
                Debug.Log("KEY NOT FOUND");
            }
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

                case DataController.PacketType.UpdateNetworkSendRate:
                    float parseVal;
                    if (float.TryParse(contents, out parseVal))
                    {
                        DataController.Instance.meshSendRate = parseVal;
                    }
                    break;

                case DataController.PacketType.UpdateRayTracerGap:
                    float parseVals;
                    if (float.TryParse(contents, out parseVals))
                    {
                        DataController.Instance.rayTracerGap = parseVals;
                    }
                    break;

                case DataController.PacketType.UpdateNetworkedObjectSendRate:
                    float parseVals1;
                    if (float.TryParse(contents, out parseVals1))
                    {
                        DataController.Instance.networkedObjectSendRate = parseVals1;
                    }
                    break;
            }
        }

        public void SendSeralisedMeshToClient(int transmissionID, int meshIndex, int meshTotal, int frameNumber, byte[] mesh)
        {
            if (server.NumberOfPeers > 0)
            {
                // SPLIT UP ARRAY
                StartCoroutine(server.Connection.SendBytesToClientsRoutine(transmissionID, frameNumber, meshIndex, meshTotal, mesh));
            }
            else
            {
                RayTraceController.Instance.StopRayTracing();
            }
        }

        public void Server_OnPeerConnected(Peer obj)
        {
            Debug.Log("Peer connected!");

            if (obj.isConnected)
            {
                Timing.RunCoroutine(SendPerformanceData(), "SendPerformanceData");
            }
        }

        public IEnumerator<float> SendPerformanceData()
        {
            yield return Timing.WaitForSeconds(1f); 

            while (server.NumberOfPeers > 0)
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

                long totalMem = (Profiler.GetTotalReservedMemoryLong() / 1048576);
                long memoryAlloc = totalMem - (Profiler.GetTotalAllocatedMemoryLong() / 1048576);

                DataController.Instance.performanceDictionary[DataController.StatisticType.MEMTOTAL] = float.Parse(totalMem.ToString());
                DataController.Instance.performanceDictionary[DataController.StatisticType.MEMALLOC] = float.Parse(memoryAlloc.ToString());

                foreach (KeyValuePair<DataController.StatisticType, float> kvp in DataController.Instance.performanceDictionary)
                {
                    server.Connection.SendPerformanceDictionary((int)kvp.Key, kvp.Value);
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
