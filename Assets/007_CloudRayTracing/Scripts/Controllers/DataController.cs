using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
        public float timeBetweenTransmissions = 0.1f;
        public float pointsPerMesh = 500f;

        public enum PacketType { StartRayTracer, StopRayTracer, }
        public enum ApplicationType { Undefined, Client, Server, Host, }
        public enum StatisticType { FPS, AVGFPS, MINFPS, MAXFPS, MEMTOTAL, MEMALLOC, }
        public enum ClientCanvasButtonType { Information, Controls, Viewports, Performance, Disconnect, }

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
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            return localIP;
        }
    }
}
