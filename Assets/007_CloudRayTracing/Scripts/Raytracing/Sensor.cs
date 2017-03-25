using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Sensor : MonoBehaviour
    {
        public SensorManager sensorManager;

        public GameObject sensorArea;
        public Transform rayPoint;

        [HideInInspector]
        public bool finishedRayCasting = false;

        private float leftX;
        private float rightX;
        private float topY;
        private float botY;

        private Vector3 topLeft;
        private Vector3 topRight;
        private Vector3 botRight;
        private Vector3 botLeft;

        private List<GameObject> sensorLines = new List<GameObject>();

        void Start()
        {
            UpdateValues();

            for (int i = 0; i < 8; i++)
            {
                GameObject sensorLine = Instantiate(sensorManager.linePrefab, transform);

                switch (i)
                {
                    case 0:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, transform.position);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, topLeft);
                        break;

                    case 1:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, transform.position);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, topRight);
                        break;

                    case 2:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, transform.position);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, botRight);
                        break;

                    case 3:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, transform.position);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, botLeft);
                        break;

                    case 4:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, topLeft);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, topRight);
                        break;

                    case 5:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, topLeft);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, botLeft);
                        break;

                    case 6:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, topRight);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, botRight);
                        break;

                    case 7:
                        sensorLine.GetComponent<LineRenderer>().SetPosition(0, botLeft);
                        sensorLine.GetComponent<LineRenderer>().SetPosition(1, botRight);
                        break;
                }
            }
        }

        void Update()
        {
            UpdateValues();
        }

        public void FireRays()
        {
            RaycastHit hit;
            Vector3 dir;

            finishedRayCasting = false;

            // Iterate through every point within the bounds
            for (float i = topY; i > botY; i -= sensorManager.gapBetweenPoints) // Go from top to bottom of the bounds
            {
                for (float j = leftX; j < rightX; j += sensorManager.gapBetweenPoints) // Go from left to right of the bounds
                {
                    dir = (MovePointerToLoc(j, i, sensorManager.sensorDepth) - transform.position).normalized; // Direction vector from sensor to point within bounds

                    // Fire a ray from the sensor to the current point in the bounds
                    if (Physics.Raycast(transform.position, dir, out hit, sensorManager.sensorDepth, sensorManager.toDetect.value))
                    {
                        // If it intersects with an object, add that point to the list of hit positions
                        sensorManager.hitPositions.Add(hit.point);
                    }
                }
            }

            finishedRayCasting = true;
        }

        private void UpdateValues()
        {
            sensorArea.transform.localScale = new Vector3(sensorManager.sensorWidth / 10, sensorManager.sensorHeight / 10, 0f);
            sensorArea.transform.localPosition = new Vector3(0f, 0f, sensorManager.sensorDepth);

            leftX = (-(sensorManager.sensorWidth / 2) / 10f) / sensorArea.transform.localScale.x;
            rightX = ((sensorManager.sensorWidth / 2) / 10f) / sensorArea.transform.localScale.x;
            topY = ((sensorManager.sensorHeight / 2) / 10f) / sensorArea.transform.localScale.y;
            botY = (-(sensorManager.sensorHeight / 2) / 10f) / sensorArea.transform.localScale.y;

            topLeft = MovePointerToLoc(leftX, topY, sensorManager.sensorDepth);
            topRight = MovePointerToLoc(rightX, topY, sensorManager.sensorDepth);
            botRight = MovePointerToLoc(rightX, botY, sensorManager.sensorDepth);
            botLeft = MovePointerToLoc(leftX, botY, sensorManager.sensorDepth);
        }

        //void OnDrawGizmos()
        //{
        //    if (sensorArea != null)
        //    {
        //        UpdateValues(); // Should this be called in this function?

        //        Gizmos.color = Color.blue;

        //        Gizmos.DrawLine(transform.position, topLeft); 
        //        Gizmos.DrawLine(transform.position, topRight);
        //        Gizmos.DrawLine(transform.position, botRight);
        //        Gizmos.DrawLine(transform.position, botLeft);

        //        Gizmos.DrawLine(topLeft, topRight);//
        //        Gizmos.DrawLine(topLeft, botLeft); //
        //        Gizmos.DrawLine(topRight, botRight);
        //        Gizmos.DrawLine(botLeft, botRight);
        //    }
        //}

        Vector3 MovePointerToLoc(float x, float y, float z)
        {
            rayPoint.localPosition = new Vector3(x, y, z);
            return rayPoint.position;
        }
    }
}