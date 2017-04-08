using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        private int transmissionID = 0;

        // Use this for initialization
        void Start()
        {
            server = new Server();

            server.OnPeerConnected += Server_OnPeerConnected;
        }

        public void StartServer()
        {
            server.StartServer(7777);

            foreach (NetworkedObject netObj in DataController.Instance.networkedObjectDictionary.Values)
            {
                netObj.ServerStart();
            }

            MenuController.Instance.UpdateSubTitleText("You are the SERVER");
        }

        public void UpdateObjectPosition(int objectID, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                GameObject go = DataController.Instance.networkedObjectDictionary[objectID].gameObject;
                go.transform.position = position;
                go.transform.eulerAngles = rotation;
                go.transform.localScale = localScale;
            }
            else
            {
                Debug.Log("Object with ID " + objectID + " not found in UpdateObjectPosition");
            }
        }

        public void UpdateObjectState(int objectID, bool active)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                DataController.Instance.networkedObjectDictionary[objectID].gameObject.SetActive(active);
            }
            else
            {
                Debug.Log("Object with ID " + objectID + " not found in UpdateObjectState");
            }
        }

        public void UpdateObjectStateAndPosition(int objectID, bool active, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            if (DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                GameObject go = DataController.Instance.networkedObjectDictionary[objectID].gameObject;
                go.SetActive(active);
                go.transform.position = position;
                go.transform.eulerAngles = rotation;
                go.transform.localScale = localScale;
            }
            else
            {
                Debug.Log("Object with ID " + objectID + " not found in UpdateObjectStateAndPosition");
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

                case DataController.PacketType.FinishedSyncing:
                    Debug.Log(DataController.Instance.networkedObjectDictionary.Count + " objects synced");
                    Timing.RunCoroutine(SendPerformanceData(), "SendPerformanceData");
                    break;
            }
        }

        public void SendHitPositionsToClient(Vector3[] hitPostions)
        {
            if (server.NumberOfPeers > 0)
            {
                byte[] result = VectorsToBytes(hitPostions);
                StartCoroutine(server.Connection.SendBytesToClientsRoutine(transmissionID, result));

                transmissionID++;
            }
            else
            {
                RayTraceController.Instance.StopRayTracing();
            }
        }

        //byte[] SerializeObject<_T>(_T objectToSerialize)
        ////same as above, but should technically work anyway
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    MemoryStream memStr = new MemoryStream();
        //    bf.Serialize(memStr, objectToSerialize);
        //    memStr.Position = 0;
        //    return memStr.ToArray();
        //}

        private byte[] VectorsToBytes(Vector3[] hitPositions)
        {
            byte[] buff = new byte[(sizeof(float) * 3) * hitPositions.Length];
            int buffIndex = 0;

            for (int i = 0; i < hitPositions.Length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(hitPositions[i].x), 0, buff, buffIndex * sizeof(float), sizeof(float));
                buffIndex++;
                Buffer.BlockCopy(BitConverter.GetBytes(hitPositions[i].y), 0, buff, buffIndex * sizeof(float), sizeof(float));
                buffIndex++;
                Buffer.BlockCopy(BitConverter.GetBytes(hitPositions[i].z), 0, buff, buffIndex * sizeof(float), sizeof(float));
                buffIndex++;
            }

            return buff;
        }

        public void Server_OnPeerConnected(Peer obj)
        {
            Debug.Log("Peer connected!");

            DataController.Instance.applicationState = DataController.ApplicationState.ServerSynchronising;
        }

        private void Server_OnPeersDisconnected()
        {
            Debug.Log("Peer disconnected");

            for (int i = 0; i < TrafficController.Instance.trafficCars.Count; i++)
            {
                Destroy(TrafficController.Instance.trafficCars[i]);
            }

            TrafficController.Instance.trafficCars.Clear();
        }

        public IEnumerator<float> SendPerformanceData()
        {
            while (server.NumberOfPeers > 0)
            {
                server.Connection.SendPerformanceDictionary((int)DataController.StatisticType.FPS, Mathf.Floor(1.0f / Time.deltaTime));
                server.Connection.SendPerformanceDictionary((int)DataController.StatisticType.MEM, GC.GetTotalMemory(false));

                yield return Timing.WaitForSeconds(0.5f);
            }

            Server_OnPeersDisconnected();
        }
    }
}
