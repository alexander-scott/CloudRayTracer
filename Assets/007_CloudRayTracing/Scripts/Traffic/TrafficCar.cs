using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class TrafficCar : MonoBehaviour
    {
        public float acceleration = 0.1f;
        public float speedDecay = 0.96f;
        public float rotationStep = 1;
        public float maxSpeed = 1;

        public int currentWayPointIndex = 0;

        private float speed = 0;
        private float speedx = 0;
        private float speedy = 0;

        private float sameDirectionTimer = 0f;
        private float reverseTimer = 0f;
        private bool turningLeft = false;

        void Update()
        {
            if (speed > 0.001f || speed < -0.001f)
            {
                speed *= speedDecay;
            }
            else
            {
                speed = 0;
            }

            Vector3 targetDir = (TrafficController.Instance.wayPoints[currentWayPointIndex].position - transform.position).normalized;
            float angleToWaypoint = AngleSigned(transform.forward, targetDir, transform.up);

            if (sameDirectionTimer > 3f)
            {
                if (reverseTimer < 1f)
                {
                    if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined && speed > -maxSpeed)
                    {
                        speed -= acceleration;
                    }

                    reverseTimer += Time.deltaTime;
                }
                else
                {
                    reverseTimer = 0f;
                    sameDirectionTimer = 0f;
                }
            }
            else if (transform.eulerAngles.z > 90f || transform.eulerAngles.z < -90f)
            {
                if (speed < 0.1f && speed > -0.1f)
                {
                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (300f * Time.deltaTime));
                }
            }
            else
            {
                if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined && speed < maxSpeed)
                {
                    speed += acceleration;
                }

                if (Mathf.Abs(angleToWaypoint - 1f) > 1f)
                {
                    if (angleToWaypoint < 0f)
                    {
                        if (turningLeft)
                        {
                            turningLeft = false;
                            sameDirectionTimer = 0f;
                        }
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                    }
                    else
                    {
                        if (!turningLeft)
                        {
                            turningLeft = true;
                            sameDirectionTimer = 0f;
                        }
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                    }

                    sameDirectionTimer += Time.deltaTime;
                }
            }

            speedx = Mathf.Sin(transform.eulerAngles.y * (Mathf.PI / 180)) * speed;
            speedy = Mathf.Cos(transform.eulerAngles.y * (Mathf.PI / 180)) * speed;
            transform.position += new Vector3(speedx, 0, speedy);

            if (Vector3.Distance(transform.position, TrafficController.Instance.wayPoints[currentWayPointIndex].position) < 1f)
            {
                if (currentWayPointIndex == TrafficController.Instance.wayPoints.Length - 1)
                {
                    currentWayPointIndex = 0;
                }
                else
                {
                    currentWayPointIndex++;
                }
            }
        }

        public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }
    }
}
