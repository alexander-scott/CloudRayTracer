using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Sensor : MonoBehaviour
    {
        #region Publics

        [Range(0, 500)]
        public float sensorWidth;

        [Range(0, 500)]
        public float sensorHeight;

        [Range(0, 500)]
        public float sensorDepth;

        [Range(0.001f, 1)]
        public float gapBetweenPoints;

        public LayerMask toDetect;

        public GameObject sensorArea;
        public Transform rayPoint;

        [HideInInspector]
        public bool finishedRayCasting = false;

        #endregion

        #region Firing Rays Privates

        private float leftX;
        private float rightX;
        private float topY;
        private float botY;

        private Vector3 topLeft;
        private Vector3 topRight;
        private Vector3 botRight;
        private Vector3 botLeft;

        #endregion

        private SensorManager sensorManager;

        public void Init(SensorManager sm)
        {
            sensorManager = sm;
        }

        public void Update()
        {
            UpdateValues();
        }

        public void FireRays()
        {
            RaycastHit hit;
            Vector3 dir;

            finishedRayCasting = false;

            // Iterate through every point within the bounds
            for (float i = topY; i > botY; i -= gapBetweenPoints) // Go from top to bottom of the bounds
            {
                for (float j = leftX; j < rightX; j += gapBetweenPoints) // Go from left to right of the bounds
                {
                    dir = (MovePointerToLoc(j, i, sensorDepth) - transform.position).normalized; // Direction vector from sensor to point within bounds

                    // Fire a ray from the sensor to the current point in the bounds
                    if (Physics.Raycast(transform.position, dir, out hit, sensorDepth, toDetect.value))
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

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topLeft, botLeft);
                Gizmos.DrawLine(topRight, botRight);
                Gizmos.DrawLine(botLeft, botRight);
            }
        }

        Vector3 MovePointerToLoc(float x, float y, float z)
        {
            rayPoint.localPosition = new Vector3(x, y, z);
            return rayPoint.position;
        }
    }
}