using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Verification.CloudRayTracing
{
    public class SensorManager : MonoBehaviour
    {
        [Header("Sensor Config")]
        [Range(0, 500)]
        public float sensorWidth = 142f; 
        [Range(0, 500)]
        public float sensorHeight = 36f; 
        [Range(0, 500)]
        public float sensorDepth = 14f;
        [Range(0.001f, 1)]
        public float gapBetweenPoints = 0.02f; // The gap between each ray fired in the sensor bounds

        [Space(10)]
        [Header("References")]
        public LayerMask toDetect;
        public GameObject pointCloudPrefab;
        public GameObject linePrefab;

        [HideInInspector]
        public List<Vector3> hitPositions = new List<Vector3>();
        [HideInInspector]
        public bool finishedRayTracing = false;
        [HideInInspector]
        public List<Mesh> listOfMeshes = new List<Mesh>();

        private Sensor[] sensors;
        private List<GameObject> pcMeshes = new List<GameObject>();
        private bool rayTracing = false;

        void Start()
        {
            sensors = GetComponentsInChildren<Sensor>();

            CreateNewMeshFilter(); // Add one to start with
        }

        public void StartRayTracer()
        {
            rayTracing = true;
            StartCoroutine(StartRaytracing());
        }

        public void StopRayTracer()
        {
            rayTracing = false;
        }

        private IEnumerator StartRaytracing()
        {
            while (rayTracing)
            {
                for (int i = 0; i < sensors.Length; i++)
                {
                    sensors[i].FireRays();
                }

                yield return new WaitUntil(() => sensors.All(b => b.finishedRayCasting));

                CreatePointMesh();

                yield return new WaitUntil(() => !finishedRayTracing);
                //yield return new WaitForSeconds(0.1f);
            }
        }

        private void CreatePointMesh()
        {
            Vector3[] points = new Vector3[hitPositions.Count];
            int[] indecies = new int[hitPositions.Count];
            Color[] colors = new Color[hitPositions.Count];

            for (int i = 0; i < hitPositions.Count; ++i) // MAX 65000
            {
                points[i] = hitPositions[i];
                indecies[i] = i;
                colors[i] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            }

            // Clear list of positions
            hitPositions.Clear();

            Mesh mesh = new Mesh();
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

            listOfMeshes.Add(mesh);

            finishedRayTracing = true;
        }

        public void CreateNewMeshFilter()
        {
            GameObject newMesh = Instantiate(pointCloudPrefab) as GameObject;

            if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                newMesh.transform.parent = ClientController.Instance.pointCloud.transform;
            else
                newMesh.transform.parent = HostController.Instance.pointCloud.transform;

            newMesh.GetComponent<MeshFilter>().mesh = new Mesh();
            pcMeshes.Add(newMesh);
        }
    }
}
