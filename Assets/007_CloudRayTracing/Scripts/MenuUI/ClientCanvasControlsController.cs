using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasControlsController : MonoBehaviour
    {
        public Button startRaytracerButton;
        public Button stopRaytracerButton;
        public InputField networkSendRateInput;
        public InputField rayTracerGapSizeInput;

        // Use this for initialization
        void Start()
        {
            startRaytracerButton.onClick.AddListener(StartRayTracer);
            stopRaytracerButton.onClick.AddListener(StopRayTracer);

            networkSendRateInput.text = DataController.Instance.networkSendRate.ToString();
            rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
        }

        public void NetworkSendRateChanged(string newVal)
        {
            float parsedVal;
            if(float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.networkSendRate = parsedVal;
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateNetworkSendRate, newVal);
                }
            }
            else
            {
                networkSendRateInput.text = DataController.Instance.networkSendRate.ToString();
            }
        }

        public void RayTracerGapSizeChanged(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.rayTracerGap = parsedVal;
                if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateNetworkSendRate, newVal);
                }
            }
            else
            {
                rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
            }
        }

        private void StartRayTracer()
        {
            if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
            {
                ClientController.Instance.StartRayTracer();
            }
            else
            {
                HostController.Instance.StartRayTracer();
            }
        }

        private void StopRayTracer()
        {
            if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
            {
                ClientController.Instance.StopRayTracer();
            }
            else
            {
                HostController.Instance.StopRayTracer();
            }
        }
    }
}
