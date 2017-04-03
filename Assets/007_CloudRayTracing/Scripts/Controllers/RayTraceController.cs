using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class RayTraceController : MonoBehaviour
    {
        #region Singleton

        private static RayTraceController _instance;

        public static RayTraceController Instance { get { return _instance; } }

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

        public SensorManager sensorManager;

        private bool rayTracing = false;
        private int transmissionID = 0;
        private int frameNumber = 0;

        public void StartRayTracing()
        {
            rayTracing = true;

            PointCloudController.Instance.StartRendering();

            StartCoroutine(RayTracerCoroutine());
        }

        public void StopRayTracing()
        {
            rayTracing = false;
        }

        private IEnumerator RayTracerCoroutine()
        {
            while (rayTracing)
            {
                sensorManager.StartRayTracer();

                // Wait until all the sensors have finished ray tracing and built the meshes
                yield return new WaitUntil(() => sensorManager.finishedRayTracing);

                Debug.Log(sensorManager.hitPositions.Count + " HIT POSITIONS");

                if (sensorManager.hitPositions.Count > 0)
                    SendData(sensorManager.hitPositions);

                sensorManager.hitPositions.Clear();

                sensorManager.finishedRayTracing = false;

                // How long should we wait before doing it all again? Bear in mind the data might not have fully reached the client yet.
                yield return new WaitForSeconds(DataController.Instance.meshSendRate);

                frameNumber++;
            }

            PointCloudController.Instance.StopRendering();
        }

        private void SendData(List<Vector3> hitPositions)
        {
            Vector3[] arrayData = hitPositions.ToArray();

            if (DataController.Instance.applicationState == DataController.ApplicationState.Server)
            {

            }
            else
            {
                PointCloudController.Instance.UpdatePositions(arrayData);
            }
        }
    }
}
