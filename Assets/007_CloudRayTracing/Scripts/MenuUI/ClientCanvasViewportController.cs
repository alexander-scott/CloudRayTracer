using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasViewportController : MonoBehaviour
    {
        [Header("Camera toggles")]
        public Toggle defaultCameraToggle; // Bot left

        public Toggle pointCloudOnlyCameraToggle; // Top left

        public Toggle wireframeCameraToggle; // Top right

        public Toggle pcAndObjectsCameraToggle; // Bot right

        private List<Camera> activeCameras = new List<Camera>();

        private bool autoToggle = false;

        void Start()
        {
            defaultCameraToggle.onValueChanged.AddListener(DefaultCameraChanged);
            pointCloudOnlyCameraToggle.onValueChanged.AddListener(PCOnlyCameraChanged);
            wireframeCameraToggle.onValueChanged.AddListener(WireframeCameraChanged);
            pcAndObjectsCameraToggle.onValueChanged.AddListener(PCandObjectsCameraChanged);

            activeCameras.Add(CameraController.Instance.cameraDefault.GetComponent<Camera>());
        }

        private void PCandObjectsCameraChanged(bool arg0)
        {
            if (arg0 && !autoToggle)
            {
                AddCamera(CameraController.Instance.cameraPCandObjects.GetComponent<Camera>());
            }
            else
            {
                if (autoToggle)
                {
                    autoToggle = false;
                    return;
                }

                if (activeCameras.Count == 1)
                {
                    Debug.Log("Can't have 0 cameras!");
                    autoToggle = true;
                    pcAndObjectsCameraToggle.isOn = true;
                }
                else
                {
                    RemoveCameraAndReorder(CameraController.Instance.cameraPCandObjects.GetComponent<Camera>());
                }
            }
        }

        private void WireframeCameraChanged(bool arg0)
        {
            if (arg0 && !autoToggle)
            {
                AddCamera(CameraController.Instance.cameraWireFrame.GetComponent<Camera>());
            }
            else
            {
                if (autoToggle)
                {
                    autoToggle = false;
                    return;
                }

                if (activeCameras.Count == 1)
                {
                    Debug.Log("Can't have 0 cameras!");
                    autoToggle = true;
                    wireframeCameraToggle.isOn = true;
                }
                else
                {
                    RemoveCameraAndReorder(CameraController.Instance.cameraWireFrame.GetComponent<Camera>());
                }
            }
        }

        private void PCOnlyCameraChanged(bool arg0)
        {
            if (arg0 && !autoToggle)
            {
                AddCamera(CameraController.Instance.cameraPCOnly.GetComponent<Camera>());
            }
            else
            {
                if (autoToggle)
                {
                    autoToggle = false;
                    return;
                }

                if (activeCameras.Count == 1)
                {
                    Debug.Log("Can't have 0 cameras!");
                    autoToggle = true;
                    pointCloudOnlyCameraToggle.isOn = true;
                }
                else
                {
                    RemoveCameraAndReorder(CameraController.Instance.cameraPCOnly.GetComponent<Camera>());
                }
            }
        }

        private void DefaultCameraChanged(bool arg0)
        {
            if (arg0 && !autoToggle)
            {
                AddCamera(CameraController.Instance.cameraDefault.GetComponent<Camera>());
            }
            else
            {
                if (autoToggle)
                {
                    autoToggle = false;
                    return;
                }

                if (activeCameras.Count == 1)
                {
                    Debug.Log("Can't have 0 cameras!");
                    autoToggle = true;
                    defaultCameraToggle.isOn = true;
                }
                else
                {
                    RemoveCameraAndReorder(CameraController.Instance.cameraDefault.GetComponent<Camera>());
                }
            }
        }

        private void AddCamera(Camera camera)
        {
            switch (activeCameras.Count)
            {
                case 1:
                    // Resize camera 0 to half height
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 1, 0.5f, 0.3f, false), "ResizeCamera");

                    activeCameras.Add(camera);

                    // Add new camera at half height above camera 0
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 1, 0.5f, 0.3f, true), "ResizeCamera");
                    break;

                case 2:
                    // Resize camera 0 to half width and half height
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 0.5f, 0.5f, 0.3f, false), "ResizeCamera");

                    activeCameras.Add(camera);

                    // Add new camera to the right of camera 0
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[2], 0.5f, 0, 0.5f, 0.5f, 0.3f, true), "ResizeCamera");
                    break;

                case 3:
                    // Resize camera 1 to half width and half height
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 0.5f, 0.5f, 0.3f, false), "ResizeCamera");

                    activeCameras.Add(camera);

                    // Add new camera to the right of camera 1
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[3], 0.5f, 0.5f, 0.5f, 0.5f, 0.3f, true), "ResizeCamera");

                    break;
            }
        }

        private void RemoveCameraAndReorder(Camera camera)
        {
            int indexOfCamera = activeCameras.FindIndex(g => g.gameObject == camera.gameObject);

            // Make the select camera disspear
            Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[indexOfCamera].GetComponent<Camera>(), 
                activeCameras[indexOfCamera].GetComponent<Camera>().rect.x, activeCameras[indexOfCamera].GetComponent<Camera>().rect.y, 
                0, 0, 0.3f, false), "ResizeCamera");

            activeCameras.RemoveAt(indexOfCamera);

            switch (activeCameras.Count)
            {
                case 3:
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 0.5f, 0.5f, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 1f, 0.5f, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[2], 0.5f, 0, 0.5f, 0.5f, 0.3f, true), "ResizeCamera");
                    break;

                case 2:
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 1, 0.5f, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 1, 0.5f, 0.3f, true), "ResizeCamera");
                    break;

                case 1:
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 1, 1, 0.3f, false), "ResizeCamera");
                    break;
            }
        }
    }
}
