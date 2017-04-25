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

        public void StartRayTracing()
        {
            rayTracing = true;
            sensorManager.hitPositions = new Octree(0.1f, DataController.Instance.centralCar.transform.position, 0.1f);

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

                Debug.Log(sensorManager.hitPositions.ObjectCount + " HIT POSITIONS");

                if (sensorManager.hitPositions.ObjectCount > 0)
                    SendData(sensorManager.hitPositions.GetAllPositions());

                sensorManager.hitPositions = new Octree(0.05f, DataController.Instance.centralCar.transform.position, 0.05f);

                GC.Collect();

                sensorManager.finishedRayTracing = false;

                // How long should we wait before doing it all again? Bear in mind the data might not have fully reached the client yet.
                if (DataController.Instance.hitPositionsSendRate == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    yield return new WaitForSeconds(DataController.Instance.hitPositionsSendRate);
                }
            }
        }

        private void SendData(List<Vector3> hitPositions)
        {
            if (!rayTracing)
                return;

            if (DataController.Instance.applicationState == DataController.ApplicationState.Server)
            {
                ServerController.Instance.SendHitPositionsToClient(hitPositions);
            }
            else
            {
                PointCloudController.Instance.UpdatePositions(hitPositions);
            }
        }
    }
}
