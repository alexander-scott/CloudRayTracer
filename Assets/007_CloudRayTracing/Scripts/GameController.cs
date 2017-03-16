using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BMW.Verification.CloudRayTracing
{
    public class GameController : MonoBehaviour
    {
        private GameObject pointCloud;
        private List<GameObject> pcMeshes = new List<GameObject>();

        private int count = 0;

        void Start()
        {
            pointCloud = FindObjectOfType<GlobalAssets>().pointCloud;
        }

        public void StartRaytracing()
        {
            NetworkController nController = FindObjectOfType<NetworkController>();

            if (nController != null)
            {
                nController.CommenceRayTracing();
            }
            else
            {
                Debug.Log("NOT CONNECTED");
            }
        }

        public void DrawToPointCloud(Mesh mesh, int meshCount)
        {
            if (meshCount > pcMeshes.Count)
            {
                for (int i = 0; i < meshCount - pcMeshes.Count; i++)
                {
                    GameObject newMesh = Instantiate(FindObjectOfType<GlobalAssets>().pointCloudMesh) as GameObject;
                    newMesh.transform.parent = pointCloud.transform;
                    pcMeshes.Add(newMesh);
                }
            }
            else if (meshCount < pcMeshes.Count)
            {
                // If we have less meshes than mesh filters that means that there could be objects on screen from previous frames
                // so we will manually clear them.
                for (int i = pcMeshes.Count; i > meshCount; i--)
                {
                    pcMeshes[i].GetComponent<MeshFilter>().mesh.Clear();
                }
            }

            if (count == meshCount - 1)
            {
                count = 0;
            }
            else
            {
                count++;
            }

            pcMeshes[count].GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
