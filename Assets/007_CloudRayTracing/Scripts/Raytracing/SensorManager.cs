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
        public GameObject linePrefab;

        [HideInInspector]
        public List<Vector3> hitPositions = new List<Vector3>();
        [HideInInspector]
        public bool finishedRayTracing = false;
        [HideInInspector]
        public List<Mesh> listOfMeshes = new List<Mesh>();

        private Sensor[] sensors;
        private List<GameObject> pcMeshes = new List<GameObject>();

        void Start()
        {
            sensors = GetComponentsInChildren<Sensor>();
        }

        public void StartRayTracer()
        {
            StartCoroutine(FireRaysAndBuildMesh());
        }

        private IEnumerator FireRaysAndBuildMesh()
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].FireRays();
            }

            yield return new WaitUntil(() => sensors.All(b => b.finishedRayCasting));

            CreatePointMesh();
        }

        private void CreatePointMesh()
        {
            int totalPoints = 0;

            Vector3[] points = new Vector3[hitPositions.Count];
            int[] indecies = new int[hitPositions.Count];

            for (int i = 0; i < hitPositions.Count; ++i)
            {
                points[i] = hitPositions[i];
                indecies[i] = i;

                if (totalPoints > DataController.Instance.pointsPerMesh || i == (hitPositions.Count - 1))
                {
                    Mesh mesh = new Mesh();
                    mesh.vertices = points;
                    mesh.SetIndices(indecies, MeshTopology.Points, 0);

                    listOfMeshes.Add(mesh);

                    points = new Vector3[hitPositions.Count];
                    indecies = new int[hitPositions.Count];

                    totalPoints = 0;
                }

                totalPoints++;
            }

            // Clear list of positions
            hitPositions.Clear();

            finishedRayTracing = true;
        }
    }
}
