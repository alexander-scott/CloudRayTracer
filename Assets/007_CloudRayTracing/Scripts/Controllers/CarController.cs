using UnityEngine;
using System.Collections;

namespace BMW.Verification.CloudRayTracing
{
    public class CarController : MonoBehaviour
    {
        public float acceleration = 0.1f;

        public float speedDecay = 0.96f;

        public float rotationStep = 1;

        public float maxSpeed = 1;

        private float speed = 0;

        private float rotation = 0;

        private float speedx = 0;

        private float speedy = 0;

        // Update is called once per frame
        void Update()
        {
            if (DataController.Instance.applicationType != DataController.ApplicationType.Undefined)
            {
                if (speed > 0.001f || speed < -0.001f)
                {
                    speed *= speedDecay;
                }
                else
                {
                    speed = 0;
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
                    rotation -= rotationStep * (speed / maxSpeed);
                    transform.rotation = Quaternion.Euler(0, rotation, 0);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    rotation += rotationStep * (speed / maxSpeed);
                    transform.rotation = Quaternion.Euler(0, rotation, 0);
                }

                speedx = Mathf.Sin(transform.eulerAngles.y * (Mathf.PI / 180)) * speed;
                speedy = Mathf.Cos(transform.eulerAngles.y * (Mathf.PI / 180)) * speed;
                transform.position += new Vector3(speedx, 0, speedy);
            }
        }
    }
}
