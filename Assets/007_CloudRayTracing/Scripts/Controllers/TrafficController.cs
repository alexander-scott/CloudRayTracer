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
        public List<GameObject> trafficCars = new List<GameObject>();

        public void SpawnCarsHost()
        {
            for (int i = 0; i < totalCarsToSpawn; i++)
            {
                trafficCars.Add(Instantiate(carPrefab, transform));

                trafficCars[i].transform.position = new Vector3(Random.Range(-100f, 200f), 0, Random.Range(-200f, 200f));
            }

            DataController.Instance.centralCar = trafficCars[0].GetComponent<CarController>();
            DataController.Instance.centralCar.isFocusCar = true;

            SensorManager.Instance.transform.parent = DataController.Instance.centralCar.transform;
            SensorManager.Instance.transform.localPosition = Vector3.zero;
        }

        public List<NetworkedObject> SpawnCarsClient()
        {
            List<NetworkedObject> networkObjects = new List<NetworkedObject>();

            for (int i = 0; i < totalCarsToSpawn; i++)
            {
                trafficCars.Add(Instantiate(carPrefab, transform));

                trafficCars[i].transform.position = new Vector3(Random.Range(-100f, 200f), 2f, Random.Range(-200f, 200f));

                int objectID = Random.Range(1, 10000000);

                trafficCars[i].GetComponent<NetworkedObject>().objectID = objectID;

                DataController.Instance.networkedObjectDictionary[objectID] = trafficCars[i].GetComponent<NetworkedObject>();

                networkObjects.Add(trafficCars[i].GetComponent<NetworkedObject>());
            }

            return networkObjects;
        }

        public void SpawnCarServer(int objectID, bool active)
        {
            GameObject newCar = Instantiate(carPrefab, transform);

            newCar.transform.position = new Vector3(Random.Range(-100f, 200f), 2f, Random.Range(-200f, 200f));

            newCar.GetComponent<NetworkedObject>().objectID = objectID;
            newCar.GetComponent<NetworkedObject>().active = active;
            newCar.GetComponent<NetworkedObject>().ServerStart();

            newCar.SetActive(active);

            DataController.Instance.networkedObjectDictionary[objectID] = newCar.GetComponent<NetworkedObject>();

            trafficCars.Add(newCar);
        }
    }
}
