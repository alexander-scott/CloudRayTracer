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

        public Octree hitPositions;
        public bool finishedRayTracing = false;

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
            StartCoroutine(FireRays());
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

        public bool CheckIfDuplicate(Vector3 pos)
        {
            if (hitPositions.ObjectCount == 0)
                return false;

            //for (int i = 0; i < hitPositions.Count; i++)
            //{
            //    if ((hitPositions[i] - pos).sqrMagnitude < (DataController.Instance.pointMeshSize / 2f))
            //    {
            //        return true;
            //    }
            //}

            return false;
        }

        private IEnumerator FireRays()
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensors[i].gameObject.activeInHierarchy)
                {
                    sensors[i].FireRays();
                }
            }

            yield return new WaitUntil(() => sensors.All(b => b.finishedRayCasting));

            finishedRayTracing = true; // Lets raytracecontroller know we have finished ray tracing
        }
    }
}
