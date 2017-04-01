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

        public void SpawnCarsHost()
        {
            for (int i = 0; i < totalCarsToSpawn; i++)
            {
                trafficCars.Add(Instantiate(carPrefab, transform));

                trafficCars[i].transform.position = new Vector3(Random.Range(-100f, 200f), 0, Random.Range(-200f, 200f));

                trafficCars[i].GetComponent<NetworkedObject>().objectID = Random.Range(1, 10000000);
            }
        }

        public void SpawnCarsClient()
        {
            List<int> objectIDs = new List<int>();

            for (int i = 0; i < totalCarsToSpawn; i++)
            {
                trafficCars.Add(Instantiate(carPrefab, transform));

                trafficCars[i].transform.position = new Vector3(Random.Range(-100f, 200f), 2f, Random.Range(-200f, 200f));

                int objectID = Random.Range(1, 10000000);

                trafficCars[i].GetComponent<NetworkedObject>().objectID = objectID;

                objectIDs.Add(objectID);
            }

            Timing.RunCoroutine(ClientController.Instance.SpawnCarsOnServer(objectIDs));
        }

        public void SpawnCarServer(int objectID)
        {
            GameObject newCar = Instantiate(carPrefab, transform);

            newCar.transform.position = new Vector3(Random.Range(-100f, 200f), 2f, Random.Range(-200f, 200f));

            newCar.GetComponent<NetworkedObject>().objectID = objectID;

            trafficCars.Add(newCar);
        }
    }
}
