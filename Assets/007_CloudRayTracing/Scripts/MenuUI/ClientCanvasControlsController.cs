using System;
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
        public Toggle groundUndetectableToggle;
        public InputField hitPositionsSendRateInput;
        public InputField rayTracerGapSizeInput;
        public InputField networkedObjectSendRateInput;
        public InputField pointCloudPointSizeInput;

        // Use this for initialization
        void Start()
        {
            rayTracerToggle.onValueChanged.AddListener(RayTracerChanged);
            aiMovementToggle.onValueChanged.AddListener(AIMovementChanged);
            firstPersonToggle.onValueChanged.AddListener(FirstPersonChanged);
            groundUndetectableToggle.onValueChanged.AddListener(GroundUndetectableChanged);

            networkedObjectSendRateInput.onEndEdit.AddListener(NetworkedObjectSendRateInput);
            rayTracerGapSizeInput.onEndEdit.AddListener(RayTracerGapSizeChanged);
            hitPositionsSendRateInput.onEndEdit.AddListener(HitPositionsSendRateChanged);
            pointCloudPointSizeInput.onEndEdit.AddListener(PointCloudPointSizeChanged);

            hitPositionsSendRateInput.text = DataController.Instance.hitPositionsSendRate.ToString();
            rayTracerGapSizeInput.text = DataController.Instance.rayTracerGap.ToString();
            networkedObjectSendRateInput.text = DataController.Instance.networkedObjectSendRate.ToString();
            pointCloudPointSizeInput.text = DataController.Instance.pointCloudPointSize.ToString();
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

        private void GroundUndetectableChanged(bool arg0)
        {
            if (arg0)
            {
                foreach(Transform go in DataController.Instance.groundTrack.GetComponentInChildren<Transform>())
                {
                    go.gameObject.layer = 0;
                }
            }
            else
            {
                foreach (Transform go in DataController.Instance.groundTrack.GetComponentInChildren<Transform>())
                {
                    go.gameObject.layer = 8;
                }
            }

            if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
            {
                ClientController.Instance.SendPacket(DataController.PacketType.UpdateGroundUndetectable, arg0.ToString());
            }
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

        private void PointCloudPointSizeChanged(string newVal)
        {
            float parsedVal;
            if (float.TryParse(newVal, out parsedVal))
            {
                DataController.Instance.pointCloudPointSize = parsedVal;
                PlayerPrefs.SetFloat("PointCloudPointSize", parsedVal);
                PlayerPrefs.Save();
                if (DataController.Instance.applicationState == DataController.ApplicationState.Client)
                {
                    ClientController.Instance.SendPacket(DataController.PacketType.UpdatePointCloudPointSize, newVal);
                }
            }
            else
            {
                pointCloudPointSizeInput.text = DataController.Instance.pointCloudPointSize.ToString();
            }
        }

    }
}
