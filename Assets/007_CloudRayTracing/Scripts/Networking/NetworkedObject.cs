using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class NetworkedObject : MonoBehaviour
    {
        private Vector3 oldKey;

        private float sendTimer = 0f;

        void Start()
        {
            if (DataController.Instance.applicationType == DataController.ApplicationType.Server)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }

                Destroy(this);
            }

            oldKey = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (DataController.Instance.applicationType == DataController.ApplicationType.Client && ClientController.Instance.client.IsConnected)
            {
                if (transform.hasChanged
                    && sendTimer > DataController.Instance.networkedObjectSendRate)
                {
                    transform.hasChanged = false;

                    ClientController.Instance.UpdateObjectPositionOnServer(oldKey, transform.position, transform.eulerAngles, transform.localScale);

                    oldKey = transform.position;

                    sendTimer = 0f;
                }
                else
                {
                    sendTimer += Time.deltaTime;
                }
            }
        }
    }
}
