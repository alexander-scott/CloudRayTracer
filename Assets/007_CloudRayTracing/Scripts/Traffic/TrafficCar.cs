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

        private float speed = 0;

        private float rotation = 0;

        private float speedx = 0;

        private float speedy = 0;

        public int currentWayPointIndex = 0;

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

            if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined && speed < maxSpeed)
            {
                speed += acceleration;
            }

            Vector3 targetDir = (TrafficController.Instance.wayPoints[currentWayPointIndex].position - transform.position).normalized;
            float angleToWaypoint = AngleSigned(transform.forward, targetDir, transform.up);

            if (Mathf.Abs(angleToWaypoint - 1f) > 1f)
            {
                if (angleToWaypoint < 0f)
                {
                    rotation -= rotationStep * (speed / maxSpeed);
                    transform.rotation = Quaternion.Euler(0, rotation, 0);
                }
                else
                {
                    rotation += rotationStep * (speed / maxSpeed);
                    transform.rotation = Quaternion.Euler(0, rotation, 0);
                }
            }
            else
            {
                if (rotation > 0.1f)
                {
                    rotation -= rotationStep * (speed / maxSpeed);
                    transform.rotation = Quaternion.Euler(0, rotation, 0);
                }
                else if (rotation < -0.1f)
                {
                    rotation += rotationStep * (speed / maxSpeed);
                    transform.rotation = Quaternion.Euler(0, rotation, 0);
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
