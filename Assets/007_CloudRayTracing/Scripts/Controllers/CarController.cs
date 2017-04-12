using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class CarController : MonoBehaviour
    {
        [Header("References")]
        public Transform backLeftWheel;
        public Transform backRightWheel;
        public Transform frontRightWheel;
        public Transform frontLeftWheel;

        public GameObject colliderParent;
        public Transform firstPersonCam;

        [Space(10)]
        [Header("Options")]

        public float acceleration = 0.1f;
        public float speedDecay = 0.96f;
        public float rotationStep = 1;
        public float maxSpeed;

        [Space(10)]
        [Header("Variables")]
        public int currentWayPointIndex = 0;

        public float speed = 0;
        public float speedx = 0;
        public float speedy = 0;

        public float sameDirectionTimer = 0f;
        public bool turningLeft = false;

        public bool isFocusCar = false;

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

            RandomAttributes();
        }

        void Update()
        {
            if (DataController.Instance.applicationState == DataController.ApplicationState.Server)
            {
                Destroy(backLeftWheel.GetComponent<SphereCollider>());
                Destroy(frontLeftWheel.GetComponent<SphereCollider>());
                Destroy(frontRightWheel.GetComponent<SphereCollider>());
                Destroy(backRightWheel.GetComponent<SphereCollider>());
                Destroy(colliderParent);
            }

            if (DataController.Instance.applicationState != DataController.ApplicationState.Undefined)
            {
                DrivingCalculations();

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
                //GetComponent<Rigidbody>().AddForce(new Vector3(speedx, 0, speedy));

                if (Vector3.Distance(transform.position, TrafficController.Instance.wayPoints[currentWayPointIndex].position) < 15f)
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

        private float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        private void RandomAttributes()
        {
            maxSpeed = Random.Range(0.2f, 0.5f);
            rotationStep = Random.Range(1f, 1.5f);
            transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        private void DrivingCalculations()
        {
            if (DataController.Instance.aiMovement)
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
                            else
                            {
                                sameDirectionTimer = 0f;
                            }

                            if (sameDirectionTimer > 3f)
                            {
                                carState = CarState.Reversing;
                            }
                        }

                        if ((transform.up.y < 0f))
                        {
                            carState = CarState.Flipping;

                        }

                        break;

                    case CarState.Reversing:
                        if (WheelsOnFloor() > 3)
                        {
                            if (DataController.Instance.applicationState != DataController.ApplicationState.Undefined && speed > -maxSpeed)
                            {
                                speed -= acceleration;
                            }

                            if (Mathf.Abs(angleToWaypoint - 1f) > 1f)
                            {
                                if (angleToWaypoint < 0f)
                                {
                                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                                }
                                else
                                {
                                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                                }
                            }
                            else
                            {
                                carState = CarState.DrivingToWayPoint;
                                sameDirectionTimer = 0f;
                            }
                        }

                        break;

                    case CarState.Flipping:
                        if (speed < 0.1f && speed > -0.1f)
                        {
                            float angleToUp = AngleSigned(transform.up, Vector3.up, transform.forward);

                            if (angleToUp > 20f)
                            {
                                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (300f * Time.deltaTime));
                            }
                            else if (angleToUp < -20f)
                            {
                                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - (300f * Time.deltaTime));
                            }

                            if (transform.up.y > 0.5f && WheelsOnFloor() > 3)
                            {
                                carState = CarState.DrivingToWayPoint;
                            }
                        }

                        break;
                }
            }
            else
            {
                if (isFocusCar)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                    }

                    if (Input.GetKey(KeyCode.W) && speed < maxSpeed)
                    {
                        speed += acceleration;
                    }

                    if (Input.GetKey(KeyCode.S) && speed > -maxSpeed)
                    {
                        speed -= acceleration;
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        //rotation -= ;
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                    }

                    if (Input.GetKey(KeyCode.D))
                    {
                        //rotation += ;
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationStep * (speed / maxSpeed), transform.eulerAngles.z);
                    }
                }
            }
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
