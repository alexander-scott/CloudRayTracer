using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class CameraController : MonoBehaviour
    {
        #region Singleton

        private static CameraController _instance;

        public static CameraController Instance { get { return _instance; } }

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

        [Space(10)]
        [Header("Cameras")]
        public GameObject cameraParent;
        public GameObject cameraDefault;
        public GameObject cameraEverything;
        public GameObject cameraPCOnly;
        public GameObject cameraWireFrame;

        private Camera _cameraDefault;
        private Camera _cameraEverything;
        private Camera _cameraPCOnly;
        private Camera _cameraWireframe;

        public Camera CameraDefault
        {
            get
            {
                if (_cameraDefault == null)
                {
                    _cameraDefault = cameraDefault.GetComponent<Camera>();
                }

                return _cameraDefault;
            }
        }

        public Camera CameraEverything
        {
            get
            {
                if (_cameraEverything == null)
                {
                    _cameraEverything = cameraEverything.GetComponent<Camera>();
                }

                return _cameraEverything;
            }
        }

        public Camera CameraPCOnly
        {
            get
            {
                if (_cameraPCOnly == null)
                {
                    _cameraPCOnly = cameraPCOnly.GetComponent<Camera>();
                }

                return _cameraPCOnly;
            }
        }

        public Camera CameraWireframe
        {
            get
            {
                if (_cameraWireframe == null)
                {
                    _cameraWireframe = cameraWireFrame.GetComponent<Camera>();
                }

                return _cameraWireframe;
            }
        }

        [Space(10)]
        [Header("Camera Movement")]

        public float distance = 5.0f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;
        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;
        public float distanceMin = .5f;
        public float distanceMax = 15f;
        public float smoothTime = 2f;
        float rotationYAxis = 0.0f;
        float rotationXAxis = 0.0f;
        float velocityX = 0.0f;
        float velocityY = 0.0f;

        // Use this for initialization
        void Start()
        {
            Vector3 angles = cameraDefault.transform.eulerAngles;
            rotationYAxis = angles.y;
            rotationXAxis = angles.x;
        }

        void Update()
        {
            if ((DataController.Instance.applicationState == DataController.ApplicationState.Client ||
                DataController.Instance.applicationState == DataController.ApplicationState.Host ||
                DataController.Instance.applicationState == DataController.ApplicationState.Server) &&
                DataController.Instance.centralCar != null)
            {
                if (!DataController.Instance.firstPerson)
                {
                    if (Input.GetMouseButton(0))
                    {
                        velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * 0.02f;
                        velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
                    }

                    rotationYAxis += velocityX;
                    rotationXAxis -= velocityY;
                    rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

                    Quaternion rotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);

                    distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

                    Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                    Vector3 position = rotation * negDistance + DataController.Instance.centralCar.transform.position;

                    cameraParent.transform.position = Vector3.Lerp(cameraParent.transform.position, position, 0.1f);
                    cameraParent.transform.rotation = Quaternion.Lerp(cameraParent.transform.rotation, rotation, 0.1f);

                    velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
                    velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
                }
                else
                {
                    cameraParent.transform.position = Vector3.Lerp(cameraParent.transform.position, DataController.Instance.centralCar.firstPersonCam.position, 0.1f);
                    cameraParent.transform.rotation = Quaternion.Lerp(cameraParent.transform.rotation, DataController.Instance.centralCar.transform.rotation, 0.1f); 
                }
            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;

            if (angle > 360F)
                angle -= 360F;

            return Mathf.Clamp(angle, min, max);
        }

        public IEnumerator<float> ResizeCamera(Camera camera, float xPos, float yPos, float width, float height, float duration, bool instaMove)
        {
            float smoothness = 0.02f;
            float progress = 0; // This float will serve as the 3rd parameter of the lerp function.
            float increment = smoothness / duration; // The amount of change to apply.

            float originalWidth = camera.rect.width;
            float originalHeight = camera.rect.height;
            float originalXPos = camera.rect.x;
            float orignalYPos = camera.rect.y;

            float newXPos;
            float newYPos;

            while (progress < 1)
            {
                if (instaMove)
                {
                    newXPos = xPos;
                    newYPos = yPos;
                }
                else
                {
                    newXPos = Mathf.Lerp(originalXPos, xPos, progress);
                    newYPos = Mathf.Lerp(orignalYPos, yPos, progress);
                }

                camera.rect = new Rect(newXPos, newYPos, Mathf.Lerp(originalWidth, width, progress), Mathf.Lerp(originalHeight, height, progress));

                progress += increment;
                yield return Timing.WaitForSeconds(smoothness);
            }
        }
    }
}
