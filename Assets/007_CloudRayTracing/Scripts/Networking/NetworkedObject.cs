using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BMW.Verification.CloudRayTracing
{
    [ExecuteInEditMode]
    public class NetworkedObject : MonoBehaviour
    {
        public int objectID;

        private float sendTimer = 0f;

        void Start()
        {
            if (DataController.Instance == null)
            {
                if (objectID == 0)
                {
                    objectID = Random.Range(1, 10000000);
                }

                return;
            }

            DataController.Instance.networkedObjectDictionary[objectID] = gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (DataController.Instance == null)
                return;

            if (DataController.Instance.applicationType == DataController.ApplicationType.Client
                && ClientController.Instance.client.IsConnected
                && Vector3.Distance(transform.position, DataController.Instance.centralCar.transform.position) < DataController.Instance.updateDistance)
            {
                if (transform.hasChanged
                    && sendTimer > DataController.Instance.networkedObjectSendRate)
                {
                    transform.hasChanged = false;

                    ClientController.Instance.UpdateObjectPositionOnServer(objectID, transform.position, transform.eulerAngles, transform.localScale);

                    sendTimer = 0f;
                }
                else
                {
                    sendTimer += Time.deltaTime;
                }
            }
            else if (DataController.Instance.applicationType == DataController.ApplicationType.Server)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }

                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    Destroy(col);
                }

                Destroy(this);
            }
        }
    }
}
