using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public static class GlobalVariables
    {
        public static bool isClient = false;
        public static bool activated = false;

        public static string ipAddress = PlayerPrefs.GetString("IPAddress", "127.0.0.1");

        public static int defaultBufferSize = 1300; // Max ethernet MTU is ~1400

        public enum PacketType { ToggleRaytracer, }

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
    }
}
