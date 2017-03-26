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

        // Use this for initialization
        void Start()
        {
            MenuController.Instance.startRaytracerButton.onClick.AddListener(StartRayTracer);
            MenuController.Instance.stopRaytracerButton.onClick.AddListener(StopRayTracer);
        }

        public void HostSelected()
        {
            MenuController.Instance.clientCanvas.gameObject.SetActive(true);
            MenuController.Instance.UpdateSubTitleText("You are the HOST");
        }

        public void RenderMesh(List<Mesh> meshes)
        {
            
        }

        private void StartRayTracer()
        {
            Debug.Log("Raytrace start");
            RayTraceController.Instance.StartRayTracing();
        }

        private void StopRayTracer()
        {
            Debug.Log("Raytrace stop");
            RayTraceController.Instance.StopRayTracing();
        }
    }
}
