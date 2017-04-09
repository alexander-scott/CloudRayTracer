﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasControlsController : MonoBehaviour
    {
        public Toggle rayTracerToggle;
        public Toggle aiMovementToggle;
        public Toggle firstPersonToggle;
        public InputField hitPositionsSendRateInput;
        public InputField rayTracerGapSizeInput;
        public InputField networkedObjectSendRateInput;

        // Use this for initialization
        void Start()
        {
            rayTracerToggle.onValueChanged.AddListener(RayTracerChanged);
            aiMovementToggle.onValueChanged.AddListener(AIMovementChanged);
            firstPersonToggle.onValueChanged.AddListener(FirstPersonChanged);

            networkedObjectSendRateInput.onEndEdit.AddListener(NetworkedObjectSendRateInput);
            rayTracerGapSizeInput.onEndEdit.AddListener(RayTracerGapSizeChanged);
            hitPositionsSendRateInput.onEndEdit.AddListener(HitPositionsSendRateChanged);

            hitPositionsSendRateInput.text = DataController.Instance.hitPositionsSendRate.ToString();
            rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
            networkedObjectSendRateInput.text = DataController.Instance.networkedObjectSendRate.ToString();
        }

        private void RayTracerChanged(bool arg0)
        {
            if (arg0)
            {
                if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
                {
                    ClientController.Instance.StartRayTracer();
                }
                else
                {
                    HostController.Instance.StartRayTracer();
                }
            }
            else
            {
                if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
                {
                    ClientController.Instance.StopRayTracer();
                }
                else
                {
                    HostController.Instance.StopRayTracer();
                }
            }
        }

        private void AIMovementChanged(bool arg0)
        {
            DataController.Instance.aiMovement = arg0;
        }

        private void FirstPersonChanged(bool arg0)
        {
            DataController.Instance.firstPerson = arg0;
        }

        public void HitPositionsSendRateChanged(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.hitPositionsSendRate = parsedVal;
                PlayerPrefs.SetFloat("NetworkSendRate", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateHitPositionsSendRate, newVal);
                }
            }
            else
            {
                hitPositionsSendRateInput.text = DataController.Instance.hitPositionsSendRate.ToString();
            }
        }

        private void NetworkedObjectSendRateInput(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.networkedObjectSendRate = parsedVal;
                PlayerPrefs.SetFloat("NetworkedObjectSendRate", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateNetworkedObjectSendRate, newVal);
                }
            }
            else
            {
                hitPositionsSendRateInput.text = DataController.Instance.hitPositionsSendRate.ToString();
            }
        }

        public void RayTracerGapSizeChanged(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.rayTracerGap = parsedVal;
                PlayerPrefs.SetFloat("RayTracerGap", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdateRayTracerGap, newVal);
                }
            }
            else
            {
                rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
            }
        }
    }
}
