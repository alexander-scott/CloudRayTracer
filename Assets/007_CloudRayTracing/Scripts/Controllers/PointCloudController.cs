using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class PointCloudController : MonoBehaviour
    {
        #region Singleton

        private static PointCloudController _instance;

        public static PointCloudController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        #endregion

        public Mesh instanceMesh;
        public Material instanceMaterial;

        private int instanceCount = -1;
        private int cachedInstanceCount = -1;
        private ComputeBuffer positionBuffer;
        private ComputeBuffer argsBuffer;
        private ComputeBuffer colorBuffer;

        private Vector3[] hitPositions;

        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        void Update()
        {
            if (instanceCount <= 0)
                return;

            // Update starting position buffer
            if (cachedInstanceCount != instanceCount)
                UpdateBuffers();

            // Render
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f)), argsBuffer, 0, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 9);
        }

        public void UpdatePositions(Vector3[] positionData)
        {
            hitPositions = positionData;
            instanceCount = hitPositions.Length;
            UpdateBuffers();
        }

        public void StartRendering()
        {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        public void StopRendering()
        {
            if (positionBuffer != null) positionBuffer.Release();
            positionBuffer = null;

            if (colorBuffer != null) colorBuffer.Release();
            colorBuffer = null;

            if (argsBuffer != null) argsBuffer.Release();
            argsBuffer = null;

            instanceCount = -1;
        }

        void UpdateBuffers()
        {
            if (instanceCount < 1) instanceCount = 1;

            // Positions & Colors
            if (positionBuffer != null) positionBuffer.Release();
            if (colorBuffer != null) colorBuffer.Release();

            positionBuffer = new ComputeBuffer(instanceCount, 16);
            colorBuffer = new ComputeBuffer(instanceCount, 4 * 4);

            Vector4[] positions = new Vector4[instanceCount];
            Vector4[] colors = new Vector4[instanceCount];

            for (int i = 0; i < instanceCount; i++)
            {
                positions[i] = new Vector4(hitPositions[i].x, hitPositions[i].y, hitPositions[i].z, 0.05f);

                float distanceFromCentre = (DataController.Instance.centralCar.transform.position - (Vector3)positions[i]).sqrMagnitude;
                Color pointColor = Color.Lerp(Color.red, Color.blue, distanceFromCentre / (DataController.Instance.updateDistance * 3f));
                colors[i] = new Vector4(pointColor.r, pointColor.g, pointColor.b, 1f);
            }

            positionBuffer.SetData(positions);
            colorBuffer.SetData(colors);

            instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
            instanceMaterial.SetBuffer("colorBuffer", colorBuffer);

            // indirect args
            uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
            args[0] = numIndices;
            args[1] = (uint)instanceCount;
            argsBuffer.SetData(args);

            cachedInstanceCount = instanceCount;
        }

        void OnDisable()
        {
            if (positionBuffer != null) positionBuffer.Release();
            positionBuffer = null;

            if (colorBuffer != null) colorBuffer.Release();
            colorBuffer = null;

            if (argsBuffer != null) argsBuffer.Release();
            argsBuffer = null;
        }
    }
}