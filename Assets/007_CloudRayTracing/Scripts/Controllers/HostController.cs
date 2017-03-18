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

        public GameObject pointCloud;
        public GameObject clientCanvas;
        public Button startRaytracer;

        // Use this for initialization
        void Start()
        {
            startRaytracer.onClick.AddListener(StartRayTracer);
        }

        public void HostSelected()
        {
            clientCanvas.SetActive(true);
            MenuController.Instance.UpdateSubTitleText("You are the HOST");
        }

        public void RenderMesh(List<Mesh> meshes)
        {
            pointCloud.GetComponent<MeshFilter>().mesh = meshes[0];
        }

        private void StartRayTracer()
        {
            Debug.Log("Raytrace start");
            CarController.Instance.StartRayTracing();
        }
    }
}
