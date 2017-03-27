using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class DataController : MonoBehaviour
    {
        #region Singleton

        private static DataController _instance;

        public static DataController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                ipAddress = PlayerPrefs.GetString("IPAddress", "127.0.0.1");
                _instance = this;
            }
        }

        #endregion

        public string ipAddress;
        public int defaultBufferSize = 1300; // Max ethernet MTU is ~1400
        public ApplicationType applicationType = ApplicationType.Undefined;
        public float pointsPerMesh = 500f;
        [Range(0.001f, 5)]
        public float networkSendRate = 1f;
        [Range(0.001f, 1)]
        public float rayTracerGap = 0.02f; // The gap between each ray fired in the sensor bounds

        public enum PacketType { StartRayTracer, StopRayTracer, UpdateNetworkSendRate, UpdateRayTracerGap, }
        public enum ApplicationType { Undefined, Client, Server, Host, }
        public enum StatisticType { FPS, AVGFPS, MINFPS, MAXFPS, MEMTOTAL, MEMALLOC, }
        public enum ClientCanvasButtonType { Information, Controls, Viewports, Performance, Sensors, Disconnect, }

        public Dictionary<DataController.StatisticType, float> performanceDictionary = new Dictionary<DataController.StatisticType, float>();

        public class TransmissionData
        {
            public int curDataIndex; // Current position in the array of data already received.
            public byte[] data;

            public TransmissionData(byte[] _data)
            {
                curDataIndex = 0;
                data = _data;
            }
        }

        public string LocalIPAddress()
        {
#if UNITY_EDITOR_OSX
            return "NULL";
#else
            System.Net.IPHostEntry host;
            string localIP = "";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
#endif
        }
    }
}
