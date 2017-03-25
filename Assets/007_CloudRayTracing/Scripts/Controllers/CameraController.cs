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


        [Header("Car")]
        public GameObject car;

        [Space(10)]
        [Header("Cameras")]
        public GameObject cameraDefault;
        public GameObject cameraEverything;
        public GameObject cameraPCOnly;
        public GameObject cameraWireFrame;

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

        void LateUpdate()
        {
            if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined)
            {
                if (Input.GetMouseButton(0))
                {
                    velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * 0.02f;
                    velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
                }

                rotationYAxis += velocityX;
                rotationXAxis -= velocityY;
                rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

                Quaternion fromRotation = Quaternion.Euler(cameraDefault.transform.rotation.eulerAngles.x, cameraDefault.transform.rotation.eulerAngles.y, 0);
                Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
                Quaternion rotation = toRotation;

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

                // Don't need this as there won't be anything blocking our view of the car
                //RaycastHit hit;
                //if (Physics.Linecast(car.transform.position, cameraDefault.transform.position, out hit))
                //{
                //    distance -= hit.distance;
                //}

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + car.transform.position;

                UpdateCameras(rotation, position);

                velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
                velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
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

        private void UpdateCameras(Quaternion rotation, Vector3 position)
        {
            cameraDefault.transform.rotation = rotation;
            cameraDefault.transform.position = position;

            cameraWireFrame.transform.rotation = rotation;
            cameraWireFrame.transform.position = position;

            cameraPCOnly.transform.rotation = rotation;
            cameraPCOnly.transform.position = position;

            cameraEverything.transform.rotation = rotation;
            cameraEverything.transform.position = position;
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
