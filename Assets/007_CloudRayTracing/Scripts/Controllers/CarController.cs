using UnityEngine;
using System.Collections;

namespace BMW.Verification.CloudRayTracing
{
    public class CarController : TrafficCar
    {
        protected override void DrivingCalculations()
        {
            if (!DataController.Instance.aiMovement)
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
            else
            {
                base.DrivingCalculations();
            }
        }

        protected override void RandomAttributes()
        {
            // We don't want random attributes
            //base.RandomAttributes();
        }
    }
}
