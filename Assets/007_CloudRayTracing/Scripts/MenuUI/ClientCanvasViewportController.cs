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
        public Toggle defaultCameraToggle; 
        public Toggle pointCloudOnlyCameraToggle; 
        public Toggle wireframeCameraToggle; 
        public Toggle everythingCameraToggle;

        [Space(10)]
        [Header("Camera Labels")]

        public Text topLeftText;
        public Text topCentreText;
        public Text topRightText;
        public Text botLeftText;
        public Text botCentreText;
        public Text botRightText;

        private List<Camera> activeCameras = new List<Camera>();
        private List<string> activeCameraNames = new List<string>();

        private bool autoToggle = false;

        void Start()
        {
            defaultCameraToggle.onValueChanged.AddListener(DefaultCameraChanged);
            pointCloudOnlyCameraToggle.onValueChanged.AddListener(PCOnlyCameraChanged);
            wireframeCameraToggle.onValueChanged.AddListener(WireframeCameraChanged);
            everythingCameraToggle.onValueChanged.AddListener(EverythingCameraChanged);

            activeCameras.Add(CameraController.Instance.cameraDefault.GetComponent<Camera>());
            activeCameraNames.Add("Default Camera");
        }

        private void EverythingCameraChanged(bool arg0)
        {
            if (arg0 && !autoToggle)
            {
                AddCamera(CameraController.Instance.cameraEverything.GetComponent<Camera>(), "Everything Camera");
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
                    everythingCameraToggle.isOn = true;
                }
                else
                {
                    RemoveCameraAndReorder(CameraController.Instance.cameraEverything.GetComponent<Camera>());
                }
            }
        }

        private void WireframeCameraChanged(bool arg0)
        {
            if (arg0 && !autoToggle)
            {
                AddCamera(CameraController.Instance.cameraWireFrame.GetComponent<Camera>(), "Wireframe Camera");
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
                AddCamera(CameraController.Instance.cameraPCOnly.GetComponent<Camera>(), "Point Cloud Camera");
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
                AddCamera(CameraController.Instance.cameraDefault.GetComponent<Camera>(), "Default Camera");
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

        private void AddCamera(Camera camera, string cameraName)
        {
            switch (activeCameras.Count)
            {
                case 1:
                    // Resize camera 0 to bottom half
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 1, 0.5f, 0.3f, false), "ResizeCamera");
                    // Add bot centre text
                    Timing.RunCoroutine(ChangeTextDuringFade(botCentreText, topCentreText.text, 0.1f), "ChangeTextDuringFade");

                    activeCameras.Add(camera);

                    // Add new camera at top half
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 1, 0.5f, 0.3f, true), "ResizeCamera");
                    // Change top centre text
                    Timing.RunCoroutine(ChangeTextDuringFade(topCentreText, cameraName, 0.1f), "ChangeTextDuringFade");
                    break;

                case 2:
                    // Resize camera 0 to bottom left corner
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 0.5f, 0.5f, 0.3f, false), "ResizeCamera");
                    // Remove bot centre text
                    Timing.RunCoroutine(ChangeTextDuringFade(botCentreText, "", 0.1f), "ChangeTextDuringFade");
                    // Add bot left text
                    Timing.RunCoroutine(ChangeTextDuringFade(botLeftText, botCentreText.text, 0.1f), "ChangeTextDuringFade");

                    activeCameras.Add(camera);

                    // Add new camera to bottom right corner
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[2], 0.5f, 0, 0.5f, 0.5f, 0.3f, true), "ResizeCamera");
                    // Add bot right text
                    Timing.RunCoroutine(ChangeTextDuringFade(botRightText, cameraName, 0.1f), "ChangeTextDuringFade");
                    break;

                case 3:
                    // Resize camera 1 to top left corner
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 0.5f, 0.5f, 0.3f, false), "ResizeCamera");
                    // Remove top centre text
                    Timing.RunCoroutine(ChangeTextDuringFade(topCentreText, "", 0.1f), "ChangeTextDuringFade");
                    // Add top left text
                    Timing.RunCoroutine(ChangeTextDuringFade(topLeftText, topCentreText.text, 0.1f), "ChangeTextDuringFade");

                    activeCameras.Add(camera);

                    // Add new camera to top right corner
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[3], 0.5f, 0.5f, 0.5f, 0.5f, 0.3f, true), "ResizeCamera");
                    // Add top right text
                    Timing.RunCoroutine(ChangeTextDuringFade(topRightText, cameraName, 0.1f), "ChangeTextDuringFade");

                    break;
            }

            activeCameraNames.Add(cameraName);
        }

        private void RemoveCameraAndReorder(Camera camera)
        {
            int indexOfCamera = activeCameras.FindIndex(g => g.gameObject == camera.gameObject);

            // Make the select camera disspear
            Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[indexOfCamera].GetComponent<Camera>(), 
                activeCameras[indexOfCamera].GetComponent<Camera>().rect.x, activeCameras[indexOfCamera].GetComponent<Camera>().rect.y, 
                0, 0, 0.3f, false), "ResizeCamera");

            activeCameras.RemoveAt(indexOfCamera);
            activeCameraNames.RemoveAt(indexOfCamera);

            switch (activeCameras.Count)
            {
                case 3:
                    // Remove top right and left text
                    Timing.RunCoroutine(ChangeTextDuringFade(topRightText, "", 0.1f), "ChangeTextDuringFade");
                    Timing.RunCoroutine(ChangeTextDuringFade(topLeftText, "", 0.1f), "ChangeTextDuringFade");

                    // Move camera to bot left corner
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 0.5f, 0.5f, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(ChangeTextDuringFade(botLeftText, activeCameraNames[0], 0.1f), "ChangeTextDuringFade");

                    // Move camera to top half
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 1f, 0.5f, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(ChangeTextDuringFade(topCentreText, activeCameraNames[1], 0.1f), "ChangeTextDuringFade");

                    // Move camera to bot right corner
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[2], 0.5f, 0, 0.5f, 0.5f, 0.3f, true), "ResizeCamera");
                    Timing.RunCoroutine(ChangeTextDuringFade(botRightText, activeCameraNames[2], 0.1f), "ChangeTextDuringFade");
                    break;

                case 2:
                    // Remove bot right and left text
                    Timing.RunCoroutine(ChangeTextDuringFade(botRightText, "", 0.1f), "ChangeTextDuringFade");
                    Timing.RunCoroutine(ChangeTextDuringFade(botLeftText, "", 0.1f), "ChangeTextDuringFade");

                    // Move camera to bot half
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 1, 0.5f, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(ChangeTextDuringFade(botCentreText, activeCameraNames[0], 0.1f), "ChangeTextDuringFade");

                    // Move camera to top half
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[1], 0, 0.5f, 1, 0.5f, 0.3f, true), "ResizeCamera");
                    Timing.RunCoroutine(ChangeTextDuringFade(topCentreText, activeCameraNames[1], 0.1f), "ChangeTextDuringFade");
                    break;

                case 1:
                    // Remove bot half text
                    Timing.RunCoroutine(ChangeTextDuringFade(botCentreText, "", 0.1f), "ChangeTextDuringFade");

                    // Move camera to full screen
                    Timing.RunCoroutine(CameraController.Instance.ResizeCamera(activeCameras[0], 0, 0, 1, 1, 0.3f, false), "ResizeCamera");
                    Timing.RunCoroutine(ChangeTextDuringFade(topCentreText, activeCameraNames[0], 0.1f), "ChangeTextDuringFade");
                    break;
            }
        }

        private IEnumerator<float> ChangeTextDuringFade(Text text, string newText, float duration)
        {
            float smoothness = 0.02f;
            float progress = 0; // This float will serve as the 3rd parameter of the lerp function.
            float increment = smoothness / duration; // The amount of change to apply.

            Color32 startColour = text.color;

            while (progress < 1)
            {
                text.color = Color32.Lerp(startColour, Color.clear, progress);

                progress += increment;
                yield return Timing.WaitForSeconds(smoothness);
            }

            text.text = newText;
            progress = 0;

            while (progress < 1)
            {
                text.color = Color32.Lerp(Color.clear, Color.white, progress);

                progress += increment;
                yield return Timing.WaitForSeconds(smoothness);
            }
        }
    }
}
