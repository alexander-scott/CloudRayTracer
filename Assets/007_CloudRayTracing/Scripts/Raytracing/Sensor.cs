using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Sensor : MonoBehaviour
    {
        [Header("Sensor Config")]
        [Range(0, 500)]
        public float sensorWidth = 142f;
        [Range(0, 100)]
        public float sensorHeight = 36f;
        [Range(0, 100)]
        public float sensorDepth = 14f;
        [Range(-5, 5)]
        public float sensorCurve = 0f;

        public GameObject sensorArea;
        public Transform rayPoint;

        public bool finishedRayCasting = false;

        private SensorManager sensorManager;

        private float leftX;
        private float rightX;
        private float topY;
        private float botY;

        private Vector3 topLeft;
        private Vector3 topRight;
        private Vector3 botRight;
        private Vector3 botLeft;
        private Vector3 topCentre;
        private Vector3 botCentre;

        private List<SensorLine> sensorLines = new List<SensorLine>();

        void Start()
        {
            sensorManager = GetComponentInParent<SensorManager>();

            UpdateValues();

            for (int i = 0; i < 8; i++)
            {
                GameObject sensorLine = Instantiate(sensorManager.linePrefab, transform);
                sensorLines.Add(sensorLine.GetComponent<SensorLine>());
            }

            RearrangeLines();
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
            for (float i = topY; i > botY; i -= DataController.Instance.rayTracerGap) // Go from top to bottom of the bounds
            {
                for (float j = leftX; j < rightX; j += DataController.Instance.rayTracerGap) // Go from left to right of the bounds
                {
                    dir = (MovePointerToLoc(j, i, sensorDepth) - transform.position).normalized; // Direction vector from sensor to point within bounds

                    // Fire a ray from the sensor to the current point in the bounds
                    if (Physics.Raycast(transform.position, dir, out hit, sensorDepth, sensorManager.toDetect.value))
                    {
                        // If it intersects with an object, add that point to the list of hit positions
                        sensorManager.hitPositions.Add(hit.point);
                    }
                }
            }

            finishedRayCasting = true;
        }

        private void RearrangeLines()
        {
            for (int i = 0; i < sensorLines.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        sensorLines[i].LineRenderer.SetPosition(0, transform.position);
                        sensorLines[i].LineRenderer.SetPosition(1, topLeft);
                        break;

                    case 1:
                        sensorLines[i].LineRenderer.SetPosition(0, transform.position);
                        sensorLines[i].LineRenderer.SetPosition(1, topRight);
                        break;

                    case 2:
                        sensorLines[i].LineRenderer.SetPosition(0, transform.position);
                        sensorLines[i].LineRenderer.SetPosition(1, botRight);
                        break;

                    case 3:
                        sensorLines[i].LineRenderer.SetPosition(0, transform.position);
                        sensorLines[i].LineRenderer.SetPosition(1, botLeft);
                        break;

                    case 4:
                        if (Mathf.Abs(0.5f - sensorCurve) < 0.5f) // If the depth is so small don't bother smoothing the curve
                        {
                            sensorLines[i].LineRenderer.SetPosition(0, topLeft);
                            sensorLines[i].LineRenderer.SetPosition(1, topRight);
                        }
                        else
                        {
                            Vector3[] topPositionsArray = new Vector3[3];
                            topPositionsArray[0] = topLeft;
                            topPositionsArray[1] = topCentre;
                            topPositionsArray[2] = topRight;

                            Vector3[] topPositionsSmoothed = MakeSmoothCurve(topPositionsArray, 3f);
                            sensorLines[i].LineRenderer.numPositions = topPositionsSmoothed.Length;

                            for (int j = 0; j < topPositionsSmoothed.Length; j++)
                            {
                                sensorLines[i].LineRenderer.SetPosition(j, topPositionsSmoothed[j]);
                            }
                        }

                        break;

                    case 5:
                        sensorLines[i].LineRenderer.SetPosition(0, topLeft);
                        sensorLines[i].LineRenderer.SetPosition(1, botLeft);
                        break;

                    case 6:
                        sensorLines[i].LineRenderer.SetPosition(0, topRight);
                        sensorLines[i].LineRenderer.SetPosition(1, botRight);
                        break;

                    case 7:
                        if (Mathf.Abs(0.5f - sensorCurve) < 0.5f) // If the depth is so small don't bother smoothing the curve
                        {
                            sensorLines[i].LineRenderer.SetPosition(0, botLeft);
                            sensorLines[i].LineRenderer.SetPosition(1, botRight);
                        }
                        else
                        {
                            Vector3[] botPositionsArray = new Vector3[3];
                            botPositionsArray[0] = botLeft;
                            botPositionsArray[1] = botCentre;
                            botPositionsArray[2] = botRight;

                            Vector3[] botPositionsSmoothed = MakeSmoothCurve(botPositionsArray, 3f);
                            sensorLines[i].LineRenderer.numPositions = botPositionsSmoothed.Length;

                            for (int j = 0; j < botPositionsSmoothed.Length; j++)
                            {
                                sensorLines[i].LineRenderer.SetPosition(j, botPositionsSmoothed[j]);
                            }
                        }

                        break;
                }
            }
        }

        private void UpdateValues()
        {
            if (sensorManager == null)
            {
                sensorManager = GetComponentInParent<SensorManager>();
            }
            sensorArea.transform.localScale = new Vector3(sensorWidth / 10, sensorHeight / 10, 0f);
            sensorArea.transform.localPosition = new Vector3(0f, 0f, sensorDepth);

            leftX = (-(sensorWidth / 2) / 10f) / sensorArea.transform.localScale.x;
            rightX = ((sensorWidth / 2) / 10f) / sensorArea.transform.localScale.x;
            topY = ((sensorHeight / 2) / 10f) / sensorArea.transform.localScale.y;
            botY = (-(sensorHeight / 2) / 10f) / sensorArea.transform.localScale.y;

            topLeft = MovePointerToLoc(leftX, topY, sensorDepth);
            topRight = MovePointerToLoc(rightX, topY, sensorDepth);
            botRight = MovePointerToLoc(rightX, botY, sensorDepth);
            botLeft = MovePointerToLoc(leftX, botY, sensorDepth);

            topCentre = new Vector3((Mathf.Abs(topLeft.x - topRight.x) / 2) + topLeft.x, topLeft.y, topLeft.z + sensorCurve);

            botCentre = new Vector3((Mathf.Abs(botLeft.x - botRight.x) / 2) + botLeft.x, botLeft.y, botLeft.z + sensorCurve);
        }

        void OnDrawGizmos()
        {
            if (sensorArea != null)
            {
                UpdateValues(); // Should this be called in this function?

                Gizmos.color = Color.blue;

                Gizmos.DrawLine(transform.position, topLeft);
                Gizmos.DrawLine(transform.position, topRight);
                Gizmos.DrawLine(transform.position, botRight);
                Gizmos.DrawLine(transform.position, botLeft);

                // DO WE WANT TO CURVE THIS LINE?
                Gizmos.DrawLine(topLeft, topCentre);
                Gizmos.DrawLine(topCentre, topRight);

                Gizmos.DrawLine(topLeft, botLeft); 
                Gizmos.DrawLine(topRight, botRight);

                Gizmos.DrawLine(botLeft, botCentre);
                Gizmos.DrawLine(botCentre, botRight);
            }
        }

        private Vector3 MovePointerToLoc(float x, float y, float z)
        {
            rayPoint.localPosition = new Vector3(x, y, z);
            return rayPoint.position;
        }

        private Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
        {
            List<Vector3> points;
            List<Vector3> curvedPoints;
            int pointsLength = 0;
            int curvedLength = 0;

            if (smoothness < 1.0f) smoothness = 1.0f;

            pointsLength = arrayToCurve.Length;

            curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
            curvedPoints = new List<Vector3>(curvedLength);

            float t = 0.0f;
            for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
            {
                t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

                points = new List<Vector3>(arrayToCurve);

                for (int j = pointsLength - 1; j > 0; j--)
                {
                    for (int i = 0; i < j; i++)
                    {
                        points[i] = (1 - t) * points[i] + t * points[i + 1];
                    }
                }

                curvedPoints.Add(points[0]);
            }

            return (curvedPoints.ToArray());
        }
    }
}