using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasSensorController : MonoBehaviour
    {
        public SensorType[] sensorToggles;

        void Start()
        {
            for (int i = 0; i < sensorToggles.Length; i++)
            {
                sensorToggles[i].Toggle.onValueChanged.AddListener(ToggleChanged);

                if (DataController.Instance.activeSensors[sensorToggles[i].sensorType])
                {
                    sensorToggles[i].Toggle.isOn = true;
                }
                else
                {
                    sensorToggles[i].Toggle.isOn = false;
                }
            }
        }

        // This might be the most inefficient way to do anything ever
        private void ToggleChanged(bool arg0)
        {
            if (Time.timeSinceLevelLoad > 1f) // What a hack
            {
                for (int i = 0; i < sensorToggles.Length; i++)
                {
                    if (sensorToggles[i].Toggle.isOn != DataController.Instance.activeSensors[sensorToggles[i].sensorType])
                    {
                        DataController.Instance.SaveSensorState(sensorToggles[i].sensorType, arg0);
                        break;
                    }
                }
            }
        }
    }
}
