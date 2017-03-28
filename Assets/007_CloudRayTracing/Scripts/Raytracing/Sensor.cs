using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Sensor : MonoBehaviour
    {
        public DataController.SensorType sensorType;

        [Space(10)]
        [Header("Square Properties")]
        [Range(0, 100)]
        public float squareSensorWidth = 142f;
        [Range(0, 100)]
        public float squareSensorHeight = 36f;
        [Range(0, 20)]
        public float squareSensorDepth = 14f;
        [Range(-5, 5)]
        public float sensorCurve = 0f;

        [Space(10)]
        [Header("Curved Properties")]
        [Range(0, 20)]
        public float sensorDepth = 14f;
        [Range(0, 20)]
        public float sensorHeight = 10f;
        [Range(0, 180)]
        public float radius = 30f;
        [Range(0.5f, 2)]
        public float spaceBetweenWireFrameVerticies = 1f;

        [Space(10)]

        public GameObject sensorArea;

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

        private Vector3 centre;
        private List<Vector3> topCurvePositions = new List<Vector3>();
        private List<Vector3> botCurvePositions = new List<Vector3>();

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

            if (sensorType == DataController.SensorType.Square)
            {
                RearrangeLinesSquare();
            }
            else
            {
                RearrangeLinesCurved();
            }
        }

        void Update()
        {
            UpdateValues();
            if (sensorType == DataController.SensorType.Square)
            {
                RearrangeLinesSquare();
            }
            else
            {
                RearrangeLinesCurved();
            }
        }

        public void FireRays()
        {
            RaycastHit hit;
            Vector3 dir;

            finishedRayCasting = false;

            if (sensorType == DataController.SensorType.Square)
            {
                // Iterate through every point within the bounds
                for (float i = topY; i > botY; i -= DataController.Instance.rayTracerGap) // Go from top to bottom of the bounds
                {
                    for (float j = leftX; j < rightX; j += DataController.Instance.rayTracerGap) // Go from left to right of the bounds
                    {
                        dir = (sensorArea.transform.TransformPoint(j, i, sensorDepth) - transform.position).normalized; // Direction vector from sensor to point within bounds

                        // Fire a ray from the sensor to the current point in the bounds
                        if (Physics.Raycast(transform.position, dir, out hit, sensorDepth, sensorManager.toDetect.value))
                        {
                            // If it intersects with an object, add that point to the list of hit positions
                            sensorManager.hitPositions.Add(hit.point);
                        }
                    }
                }
            }
            else
            {
                // Iterate through every point within the bounds
                for (float i = sensorHeight/2; i > -sensorHeight/2; i -= DataController.Instance.rayTracerGap) // Go from top to bottom of the bounds
                {
                    for (float j = -radius/2; j < radius/2; j += DataController.Instance.rayTracerGap) // Go from left to right of the bounds
                    {
                        dir = ((transform.position + (Quaternion.Euler(0, j, 0) * centre) + new Vector3(0f, i, 0f)) - transform.position).normalized; // Direction vector from sensor to point within bounds

                        // Fire a ray from the sensor to the current point in the bounds
                        if (Physics.Raycast(transform.position, dir, out hit, sensorDepth, sensorManager.toDetect.value))
                        {
                            // If it intersects with an object, add that point to the list of hit positions
                            sensorManager.hitPositions.Add(hit.point);
                        }
                    }
                }
            }

            

            finishedRayCasting = true;
        }

        private void RearrangeLinesSquare()
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
                        sensorLines[i].LineRenderer.SetPosition(0, topLeft);
                        sensorLines[i].LineRenderer.SetPosition(1, topRight);
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
                        sensorLines[i].LineRenderer.SetPosition(0, botLeft);
                        sensorLines[i].LineRenderer.SetPosition(1, botRight);
                        break;
                }
            }
        }

        private void RearrangeLinesCurved()
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
                        sensorLines[i].LineRenderer.numPositions = topCurvePositions.Count;

                        for (int j = 0; j < topCurvePositions.Count; j++)
                        {
                            sensorLines[i].LineRenderer.SetPosition(j, topCurvePositions[j]);
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
                        sensorLines[i].LineRenderer.numPositions = botCurvePositions.Count;

                        for (int j = 0; j < botCurvePositions.Count; j++)
                        {
                            sensorLines[i].LineRenderer.SetPosition(j, botCurvePositions[j]);
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

            if (sensorType == DataController.SensorType.Square)
            {
                sensorArea.transform.localScale = new Vector3(squareSensorWidth / 10, sensorHeight / 10, 0f);
                sensorArea.transform.localPosition = new Vector3(0f, 0f, sensorDepth);

                leftX = (-(squareSensorWidth / 2) / 10f) / sensorArea.transform.localScale.x;
                rightX = ((squareSensorWidth / 2) / 10f) / sensorArea.transform.localScale.x;
                topY = ((sensorHeight / 2) / 10f) / sensorArea.transform.localScale.y;
                botY = (-(sensorHeight / 2) / 10f) / sensorArea.transform.localScale.y;

                topLeft = sensorArea.transform.TransformPoint(leftX, topY, sensorDepth);
                topRight = sensorArea.transform.TransformPoint(rightX, topY, sensorDepth);
                botRight = sensorArea.transform.TransformPoint(rightX, botY, sensorDepth);
                botLeft = sensorArea.transform.TransformPoint(leftX, botY, sensorDepth);
            }
            else
            {
                centre = ((transform.position + (transform.forward * sensorDepth)) - transform.position);

                topRight = transform.position + (Quaternion.Euler(0, +(radius / 2), 0) * centre) + new Vector3(0f, sensorHeight / 2, 0f);
                topLeft = transform.position + (Quaternion.Euler(0, -(radius / 2), 0) * centre) + new Vector3(0f, sensorHeight / 2, 0f);
                botRight = transform.position + (Quaternion.Euler(0, +(radius / 2), 0) * centre) + new Vector3(0f, -sensorHeight / 2, 0f);
                botLeft = transform.position + (Quaternion.Euler(0, -(radius / 2), 0) * centre) + new Vector3(0f, -sensorHeight / 2, 0f);

                topCurvePositions.Clear();
                botCurvePositions.Clear();

                for (float i = -(radius / 2); i < (radius / 2); i++)
                {
                    topCurvePositions.Add(transform.position + (Quaternion.Euler(0, i, 0) * centre) + new Vector3(0f, sensorHeight / 2, 0f));
                    botCurvePositions.Add(transform.position + (Quaternion.Euler(0, i + 1, 0) * centre) + new Vector3(0f, -sensorHeight / 2, 0f));
                }
            }
        }

        //void OnDrawGizmos()
        //{
        //    if (sensorArea != null)
        //    {
        //        UpdateValues(); // Should this be called in this function?

        //        Gizmos.color = Color.blue;

        //        if (sensorType == DataController.SensorType.Square)
        //        {
        //            Gizmos.DrawLine(transform.position, topLeft);
        //            Gizmos.DrawLine(transform.position, topRight);
        //            Gizmos.DrawLine(transform.position, botRight);
        //            Gizmos.DrawLine(transform.position, botLeft);

        //            // DO WE WANT TO CURVE THIS LINE?
        //            Gizmos.DrawLine(topLeft, topRight);
        //            Gizmos.DrawLine(topLeft, botLeft);
        //            Gizmos.DrawLine(topRight, botRight);
        //            Gizmos.DrawLine(botLeft, botRight);
        //        }
        //        else
        //        {
        //            for (int i = 0; i < botCurvePositions.Count - 1; i++)
        //            {
        //                Gizmos.DrawLine(botCurvePositions[i], botCurvePositions[i + 1]);
        //                Gizmos.DrawLine(topCurvePositions[i], topCurvePositions[i + 1]);
        //            }

        //            Gizmos.DrawLine(transform.position, topLeft);
        //            Gizmos.DrawLine(transform.position, topRight);
        //            Gizmos.DrawLine(transform.position, botRight);
        //            Gizmos.DrawLine(transform.position, botLeft);

        //            Gizmos.DrawLine(topLeft, botLeft);
        //            Gizmos.DrawLine(topRight, botRight);
        //        }
        //    }
        //}

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