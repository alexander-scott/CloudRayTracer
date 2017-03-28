using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class DataController : MonoBehaviour
    {
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

        private static DataController _instance;
        public static DataController Instance { get { return _instance; } }

        public string ipAddress;
        public int defaultBufferSize = 1300; // Max ethernet MTU is ~1400
        public ApplicationType applicationType = ApplicationType.Undefined;
        public float pointsPerMesh = 500f;
        public float networkSendRate = 1f;
        public float rayTracerGap = 0.02f; // The gap between each ray fired in the sensor bounds
        public Dictionary<SensorType, bool> activeSensors = new Dictionary<SensorType, bool>();

        public enum PacketType { StartRayTracer, StopRayTracer, UpdateNetworkSendRate, UpdateRayTracerGap, }
        public enum ApplicationType { Undefined, Client, Server, Host, }
        public enum StatisticType { FPS, AVGFPS, MINFPS, MAXFPS, MEMTOTAL, MEMALLOC, }
        public enum ClientCanvasButtonType { Information, Controls, Viewports, Performance, Sensors, Disconnect, }
        public enum SensorType
        {
            FrontLong,
            FrontShort,
            FrontLeft,
            FrontRight,
            BackLong,
            BackShort,
            BackLeft,
            BackRight,
            RightLong,
            RightShort,
            LeftLong,
            LeftShort,
        }

        public Dictionary<StatisticType, float> performanceDictionary = new Dictionary<DataController.StatisticType, float>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                GetPlayerPrefs();
                _instance = this;
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

        public void GetPlayerPrefs()
        {
            ipAddress = PlayerPrefs.GetString("IPAddress", "127.0.0.1");

            int numOfSensorTypes = Enum.GetNames(typeof(SensorType)).Length;

            for (int i = 0; i < numOfSensorTypes; i++)
            {
                string key = "SensorState" + ((SensorType)i).ToString();
                bool active = bool.Parse(PlayerPrefs.GetString(key, "true"));
                activeSensors[(SensorType)i] = active;
            }
        }

        public void SaveSensorState(SensorType sensorType, bool state)
        {
            activeSensors[sensorType] = state;
            SensorManager.Instance.ToggleSensor(sensorType, state);
            PlayerPrefs.SetString("SensorState" + sensorType.ToString(), state.ToString());
            PlayerPrefs.Save();
        }
    }
}
