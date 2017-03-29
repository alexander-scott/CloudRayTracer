using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class NetworkedObject : MonoBehaviour
    {
        private Vector3 oldKey;

        void Start()
        {
            oldKey = transform.position;

            if (DataController.Instance.applicationType == DataController.ApplicationType.Server)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (DataController.Instance.applicationType == DataController.ApplicationType.Client && ClientController.Instance.client.IsConnected)
            {
                if (transform.hasChanged)
                {
                    transform.hasChanged = false;

                    ClientController.Instance.UpdateObjectPositionOnServer(oldKey, transform.position, transform.eulerAngles, transform.localScale);

                    oldKey = transform.position;
                }
            }
        }
    }
}
