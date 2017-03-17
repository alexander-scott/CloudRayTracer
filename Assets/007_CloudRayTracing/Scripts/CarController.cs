using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class CarController : MonoBehaviour
    {
        #region Singleton

        private static CarController _instance;

        public static CarController Instance { get { return _instance; } }

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

        private SensorManager sensorManager;
        private int meshCount;

        // Use this for initialization
        void Start()
        {
            sensorManager = GetComponentInChildren<SensorManager>();
        }

        public void StartRayTracing()
        {
            sensorManager.StartRayTracer();
            StartCoroutine(SendSingleMesh());
        }

        private IEnumerator SendDataBack()
        {
            //int count = 0;
            //while (true)
            //{
            //    // Wait until all the sensors have finished ray tracing and built the meshes
            //    yield return new WaitUntil(() => sensorManager.finishedRayTracing);

            //    meshCount = sensorManager.listOfMeshes.Count;

            //    // Loop through each mesh and send it back
            //    for (int i = 0; i < meshCount; i++)
            //    {
            //        StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(count, MeshSerializer.WriteMesh(sensorManager.listOfMeshes[i], true, true)));
            //        count++; // Count is used so the tranmission ID is never identical to a previous transmission
            //    }

            //    sensorManager.listOfMeshes.Clear();
            //    sensorManager.finishedRayTracing = false;

            //    // How long should we wait before doing it all again? Bear in mind the data might not have fully reached the client yet.
            //    yield return new WaitForSeconds(0.3f);
            //}

            yield return null;
        }

        private IEnumerator SendSingleMesh()
        {
            //while (true)
            //{
                // Wait until all the sensors have finished ray tracing and built the meshes
                yield return new WaitUntil(() => sensorManager.finishedRayTracing);

                meshCount = sensorManager.listOfMeshes.Count;

                ServerController.Instance.SendSeralisedMeshToClient(MeshSerializer.WriteMesh(sensorManager.listOfMeshes[0], true, true));

                sensorManager.listOfMeshes.Clear();
                sensorManager.finishedRayTracing = false;

                // How long should we wait before doing it all again? Bear in mind the data might not have fully reached the client yet.
                //yield return new WaitForSeconds(0.5f);
                yield return null;
            //}
        }
    }
}
