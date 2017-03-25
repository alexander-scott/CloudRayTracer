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
        private int meshCount;

        private bool rayTracing = false;

        public void StartRayTracing()
        {
            sensorManager.StartRayTracer();

            rayTracing = true;

            if (DataController.Instance.applicationType == DataController.ApplicationType.Host)
            {
                StartCoroutine(SendMeshToHost());
            }
            else
            {
                StartCoroutine(SendMeshToClient());
            }
        }

        public void StopRayTracing()
        {
            sensorManager.StopRayTracer();

            rayTracing = false;
        }

        private IEnumerator SendMeshToClient()
        {
            int count = 0;
            while (rayTracing)
            {
                // Wait until all the sensors have finished ray tracing and built the meshes
                yield return new WaitUntil(() => sensorManager.finishedRayTracing);

                meshCount = sensorManager.listOfMeshes.Count;

                // Loop through each mesh and send it back
                for (int i = 0; i < meshCount; i++)
                {
                    if (rayTracing)
                        ServerController.Instance.SendSeralisedMeshToClient(count, MeshSerializer.WriteMesh(sensorManager.listOfMeshes[i], true, true));
                    count++; // Count is used so the tranmission ID is never identical to a previous transmission
                }

                sensorManager.listOfMeshes.Clear();
                sensorManager.finishedRayTracing = false;

                // How long should we wait before doing it all again? Bear in mind the data might not have fully reached the client yet.
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator SendMeshToHost()
        {
            while (rayTracing)
            {
                // Wait until all the sensors have finished ray tracing and built the meshes
                yield return new WaitUntil(() => sensorManager.finishedRayTracing);

                meshCount = sensorManager.listOfMeshes.Count;

                if (rayTracing)
                    HostController.Instance.RenderMesh(sensorManager.listOfMeshes);

                sensorManager.listOfMeshes.Clear();
                sensorManager.finishedRayTracing = false;

                yield return new WaitForFixedUpdate();
            }
        }
    }
}
