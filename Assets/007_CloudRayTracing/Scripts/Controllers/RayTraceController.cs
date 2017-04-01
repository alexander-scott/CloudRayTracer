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
        public Transform pointCloudParent;
        public GameObject pointCloudPrefab;

        private List<GameObject> pointCloudMeshses = new List<GameObject>();
        private int meshTotal;

        private bool rayTracing = false;
        private int transmissionID = 0;
        private int frameNumber = 0;

        public void StartRayTracing()
        {
            rayTracing = true;

            StartCoroutine(RayTracerCoroutine());
        }

        public void StopRayTracing()
        {
            rayTracing = false;

            for (int i = 0; i < pointCloudMeshses.Count; i++)
            {
                Destroy(pointCloudMeshses[i]);
            }

            pointCloudMeshses.Clear();
        }

        public void RecieveMesh(int number, int total, Mesh mesh)
        {
            if (!rayTracing && DataController.Instance.applicationState != DataController.ApplicationState.Client)
                return;

            if ((number + 1) > pointCloudMeshses.Count) // If we have been given more meshes than we have in the list create a new one
            {
                CreateNewMeshFilter();
            }

            if ((number + 1) == total) // If we've done all the meshes
            {
                if (pointCloudMeshses.Count > total) // If we have more meshes in the list than we were sent destroy the unused ones
                {
                    for (int i = pointCloudMeshses.Count - 1; i >= total; i--)
                    {
                        Destroy(pointCloudMeshses[i]);
                    }
                }
            }

            if (pointCloudMeshses[number] == null)
            {
                pointCloudMeshses[number] = Instantiate(pointCloudPrefab);
                pointCloudMeshses[number].transform.parent = pointCloudParent;
            }
            else
            {
                Destroy(pointCloudMeshses[number].GetComponent<MeshFilter>().sharedMesh);
            }
            
            pointCloudMeshses[number].GetComponent<MeshFilter>().mesh = mesh;
            pointCloudMeshses[number].transform.localPosition = new Vector3(0, 0, 0);
            pointCloudMeshses[number].transform.localRotation = new Quaternion(0, 0, 0, 0);
        }

        private IEnumerator RayTracerCoroutine()
        {
            while (rayTracing)
            {
                sensorManager.StartRayTracer();

                // Wait until all the sensors have finished ray tracing and built the meshes
                yield return new WaitUntil(() => sensorManager.finishedRayTracing);

                meshTotal = sensorManager.listOfMeshes.Count;

                SendData(sensorManager.listOfMeshes);

                sensorManager.listOfMeshes.Clear();
                sensorManager.finishedRayTracing = false;

                // How long should we wait before doing it all again? Bear in mind the data might not have fully reached the client yet.
                yield return new WaitForSeconds(DataController.Instance.meshSendRate);

                frameNumber++;
            }
        }

        private void SendData(List<Mesh> meshList)
        {
            if (DataController.Instance.applicationState == DataController.ApplicationState.Server)
            {
                // Loop through each mesh and send it back
                for (int i = 0; i < meshTotal; i++)
                {
                    if (rayTracing)
                    {
                        ServerController.Instance.SendSeralisedMeshToClient(transmissionID, i, meshTotal, frameNumber, MeshSerializer.WriteMesh(sensorManager.listOfMeshes[i], true, true));
                        Destroy(sensorManager.listOfMeshes[i]);
                    }
                        
                    transmissionID++; // Count is used so the tranmission ID is never identical to a previous transmission
                }
            }
            else
            {
                for (int i = 0; i < meshTotal; i++)
                {
                    if (rayTracing)
                    {
                        RecieveMesh(i, meshTotal, sensorManager.listOfMeshes[i]);
                    }  
                }
            }
        }

        public void CreateNewMeshFilter()
        {
            GameObject newMesh = Instantiate(pointCloudPrefab) as GameObject;
            newMesh.transform.parent = pointCloudParent;

            newMesh.GetComponent<MeshFilter>().mesh = new Mesh();
            pointCloudMeshses.Add(newMesh);
        }
    }
}
