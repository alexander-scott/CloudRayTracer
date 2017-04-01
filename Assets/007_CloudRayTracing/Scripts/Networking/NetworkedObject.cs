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
        public bool active = false;

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

            if (!DataController.Instance.networkedObjectDictionary.ContainsKey(objectID))
            {
                DataController.Instance.networkedObjectDictionary[objectID] = this;
            }

            transform.hasChanged = true;
        }

        public void ClientUpdate()
        {
            if (sendTimer > DataController.Instance.networkedObjectSendRate)
            {
                // If this game object is active (within the distance)
                if (active)
                {
                    // If this game object has moved out of distance since last frame
                    if (Vector3.Distance(transform.position, DataController.Instance.centralCar.transform.position) > DataController.Instance.updateDistance)
                    {
                        // Set inactive
                        active = false;
                        // SET TO INACTIVE ON THE SERVER
                        ClientController.Instance.UpdateObjectState(objectID, false);
                    }
                    else // If this game object has remained in distance since last frame
                    {
                        if (transform.hasChanged) // If the transform has changed
                        {
                            transform.hasChanged = false;

                            // Update position
                            ClientController.Instance.UpdateObjectPositionOnServer(objectID, transform.position, transform.eulerAngles, transform.localScale);
                        }
                    }
                }
                else // If this game object is in active (outside the distance)
                {
                    // If this game object has moved in distance since last frame
                    if (Vector3.Distance(transform.position, DataController.Instance.centralCar.transform.position) < DataController.Instance.updateDistance)
                    {
                        // Set inactive
                        active = true;
                        // SET TO ACTIVE ON THE SERVER AND UPDATE POSITION
                        ClientController.Instance.UpdateObjectStateAndPosition(objectID, true, transform.position, transform.eulerAngles, transform.localScale);
                    }
                }

                sendTimer = 0f;       
            }
            else
            {
                sendTimer += Time.deltaTime;
            }
        }

        public void ServerStart()
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

            //Destroy(this);

            active = false;
            gameObject.SetActive(false);
        }
    }
}
