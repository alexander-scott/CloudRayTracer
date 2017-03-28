using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class SensorType : MonoBehaviour
    {
        public DataController.SensorType sensorType;

        private Toggle _toggle;

        public Toggle Toggle
        {
            get
            {
                if (_toggle == null)
                {
                    _toggle = GetComponent<Toggle>();
                }

                return _toggle;
            }
        }
    }
}
