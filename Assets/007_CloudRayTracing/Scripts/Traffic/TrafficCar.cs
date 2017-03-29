using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class TrafficCar : MonoBehaviour
    {
        public Transform backLeftWheel;
        public Transform backRightWheel;
        public Transform frontRightWheel;
        public Transform frontLeftWheel;

        public float acceleration = 0.1f;
        public float speedDecay = 0.96f;
        public float rotationStep = 1;
        public float maxSpeed;

        public int currentWayPointIndex = 0;

        private float speed = 0;
        private float speedx = 0;
        private float speedy = 0;

        private float sameDirectionTimer = 0f;
        private float reverseTimer = 0f;
        private bool turningLeft = false;

        private CarState carState = CarState.DrivingToWayPoint;

        enum CarState
        {
            DrivingToWayPoint,
            Reversing,
            Flipping,
        }

        void Start()
        {
            float closestDist = 1000f;
            int closestIndex = 0;

            for (int i = 0; i < TrafficController.Instance.wayPoints.Length; i++)
            {
                float currentDist = Vector3.Distance(TrafficController.Instance.wayPoints[i].transform.position, transform.position);
                if (currentDist < closestDist)
                {
                    closestDist = currentDist;
                    closestIndex = i;
                }
            }

            currentWayPointIndex = closestIndex;

            maxSpeed = Random.Range(0.2f, 0.6f);
            rotationStep = Random.Range(1f, 2f);
        }

        void Update()
        {
            if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined)
            {
                Vector3 targetDir = (TrafficController.Instance.wayPoints[currentWayPointIndex].position - transform.position).normalized;
                float angleToWaypoint = AngleSigned(transform.forward, targetDir, transform.up);

                switch (carState)
                {
                    case CarState.DrivingToWayPoint:
                        if (WheelsOnFloor() > 3)
                        {
                            if (speed < maxSpeed)
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

                            if (sameDirectionTimer > 3f)
                            {
                                carState = CarState.Reversing;
                                reverseTimer = 0f;
                            }
                        }

                        if ((transform.up.y < -0.9f))
                        {
                            carState = CarState.Flipping;
                        }

                        break;

                    case CarState.Reversing:
                        if (WheelsOnFloor() > 3)
                        {
                            if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined && speed > -maxSpeed)
                            {
                                speed -= acceleration;
                            }

                            if (Mathf.Abs(angleToWaypoint - 1f) > 1f)
                            {
                                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                            }

                            reverseTimer += Time.deltaTime;

                            if (reverseTimer > 2f)
                            {
                                carState = CarState.DrivingToWayPoint;
                                sameDirectionTimer = 0f;
                            }
                        }
                       
                        break;

                    case CarState.Flipping:
                        if (speed < 0.1f && speed > -0.1f)
                        {
                            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (300f * Time.deltaTime));

                            if (transform.up.y > 0.9f && WheelsOnFloor() > 3)
                            {
                                carState = CarState.DrivingToWayPoint;
                            }
                        }
                            
                        break;
                }

                if (speed != 0f)
                {
                    speed *= speedDecay;
                }
                else
                {
                    speed = 0;
                }

                speedx = Mathf.Sin(transform.eulerAngles.y * (Mathf.PI / 180)) * speed;
                speedy = Mathf.Cos(transform.eulerAngles.y * (Mathf.PI / 180)) * speed;
                transform.position += new Vector3(speedx, 0, speedy);

                if (Vector3.Distance(transform.position, TrafficController.Instance.wayPoints[currentWayPointIndex].position) < 5f)
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
        }

        public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        private int WheelsOnFloor()
        {
            int count = 0;

            if (backLeftWheel.position.y < 1f)
            {
                count++;
            }

            if (backRightWheel.position.y < 1f)
            {
                count++;
            }

            if (frontLeftWheel.position.y < 1f)
            {
                count++;
            }

            if (frontRightWheel.position.y < 1f)
            {
                count++;
            }

            return count;
        }
    }
}
