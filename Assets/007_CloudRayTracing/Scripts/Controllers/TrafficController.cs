using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class TrafficController : MonoBehaviour
    {
        #region Singleton

        private static TrafficController _instance;

        public static TrafficController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                wayPoints = wayPointParent.GetComponentsInChildren<Transform>();
            }
        }

        #endregion

        public GameObject wayPointParent;
        public GameObject carPrefab;

        public int totalCarsToSpawn = 20;

        [HideInInspector]
        public Transform[] wayPoints;
        private List<GameObject> trafficCars = new List<GameObject>();

        void Start()
        {
            for (int i = 0; i < totalCarsToSpawn; i++)
            {
                trafficCars.Add(Instantiate(carPrefab, transform));

                trafficCars[i].transform.position = new Vector3(Random.Range(-100f, 200f), 2f, Random.Range(-200f, 200f));
            }
        }
    }
}
