using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Verification.CloudRayTracing
{
    public class SensorManager : MonoBehaviour
    {
        #region Singleton

        private static SensorManager _instance;

        public static SensorManager Instance { get { return _instance; } }

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

        public LayerMask toDetect;
        public GameObject linePrefab;
        public bool enableSensorGizmos = false;

        [HideInInspector]
        public List<Vector3> hitPositions = new List<Vector3>();
        [HideInInspector]
        public bool finishedRayTracing = false;
        [HideInInspector]
        public List<Mesh> listOfMeshes = new List<Mesh>();

        private Sensor[] sensors;

        void Start()
        {
            sensors = GetComponentsInChildren<Sensor>();

            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].gameObject.SetActive(DataController.Instance.activeSensors[sensors[i].sensorType]);
            }
        }

        public void StartRayTracer()
        {
            StartCoroutine(FireRaysAndBuildMesh());
        }

        public void ToggleSensor(DataController.SensorType sensorType, bool active)
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensors[i].sensorType == sensorType)
                {
                    sensors[i].gameObject.SetActive(active);

                    break;
                }
            }
        }

        private IEnumerator FireRaysAndBuildMesh()
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensors[i].gameObject.activeInHierarchy)
                {
                    sensors[i].FireRays();
                }
            }

            yield return new WaitUntil(() => sensors.All(b => b.finishedRayCasting));

            CreatePointMesh();
        }

        private void CreatePointMesh()
        {
            int totalPoints = 0;

            Vector3[] points = new Vector3[hitPositions.Count];
            int[] indecies = new int[hitPositions.Count];

            //Debug.Log("TOTAL POINTS: " + hitPositions.Count);

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
