using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class HostController : MonoBehaviour
    {
        #region Singleton

        private static HostController _instance;

        public static HostController Instance { get { return _instance; } }

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

        public void StartRayTracer()
        {
            Debug.Log("Raytrace start");
            PointCloudController.Instance.StartRendering();
            RayTraceController.Instance.StartRayTracing();
        }

        public void StopRayTracer()
        {
            Debug.Log("Raytrace stop");
            RayTraceController.Instance.StopRayTracing();
            PointCloudController.Instance.StopRendering();
        }
    }
}
