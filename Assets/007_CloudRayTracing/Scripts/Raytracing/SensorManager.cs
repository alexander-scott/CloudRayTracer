using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Verification.CloudRayTracing
{
    public class SensorManager : MonoBehaviour
    {
        public GameObject pointCloudMesh;

        [Range(0, 500)]
        public float sensorWidth = 142f;

        [Range(0, 500)]
        public float sensorHeight = 36f;

        [Range(0, 500)]
        public float sensorDepth = 14f;

        [Range(0.001f, 1)]
        public float gapBetweenPoints = 0.02f;

        public LayerMask toDetect;

        [HideInInspector]
        public List<Vector3> hitPositions = new List<Vector3>();

        [HideInInspector]
        public Vector2[] uvPool;
        [HideInInspector]
        public Vector3[] vertexPool;
        [HideInInspector]
        public int[] trianglePool;

        private GameObject pointCloud;

        private const float pointRadius = 0.01f;

        private const int nbLong = 3;
        private const int nbLat = 2;
        private const float sphereRad = 0.1f;

        private const float _pi = Mathf.PI;
        private const float _2pi = _pi * 2f;

        private Sensor[] sensors;

        private List<GameObject> pcMeshes = new List<GameObject>();
        private List<List<CombineInstance>> listOfCInstanceArrays = new List<List<CombineInstance>>();
        private List<CombineInstance> combineInstances = new List<CombineInstance>();
        private CombineInstance tempInstance = new CombineInstance();

        struct PointCloudSphere
        {
            public Vector3[] verticies;
            public Vector3[] normals;
            public Vector2[] uvs;
            public int[] triangles;
        }

        public bool finishedRayTracing = false;
        public List<Mesh> listOfMeshes = new List<Mesh>();

        private bool rayTracing = false;

        void Start()
        {
            CreateUVs();
            CreateVerticies();
            CreateTriangles();

            sensors = GetComponentsInChildren<Sensor>();

            if (DataController.Instance.applicationType == DataController.ApplicationType.Client)
                pointCloud = ClientController.Instance.pointCloud;
            else
                pointCloud = HostController.Instance.pointCloud;

            CreateNewMeshFilter(); // Add one to start with
        }

        public void StartRayTracer()
        {
            rayTracing = true;
            StartCoroutine(StartRaytracing());
        }

        public void StopRayTracer()
        {
            rayTracing = false;
        }

        IEnumerator StartRaytracing()
        {
            while (rayTracing)
            {
                for (int i = 0; i < sensors.Length; i++)
                {
                    sensors[i].FireRays();
                }

                yield return new WaitUntil(() => sensors.All(b => b.finishedRayCasting));

                CreateMeshPixelsOnServer();

                yield return new WaitUntil(() => !finishedRayTracing);
                //yield return new WaitForSeconds(0.1f);
            }
        }

        void CreateMeshPixels()
        {
            Vector3[] points = new Vector3[hitPositions.Count];
            int[] indecies = new int[hitPositions.Count];
            Color[] colors = new Color[hitPositions.Count];

            for (int i = 0; i < hitPositions.Count; ++i) // MAX 65000
            {
                points[i] = hitPositions[i];
                indecies[i] = i;
                colors[i] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            }

            // Clear list of positions
            hitPositions.Clear();

            Mesh mesh = new Mesh();
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

            pcMeshes.First().GetComponent<MeshFilter>().mesh = mesh;
        }

        void CreateMeshPixelsOnServer()
        {
            Vector3[] points = new Vector3[hitPositions.Count];
            int[] indecies = new int[hitPositions.Count];
            Color[] colors = new Color[hitPositions.Count];

            for (int i = 0; i < hitPositions.Count; ++i) // MAX 65000
            {
                points[i] = hitPositions[i];
                indecies[i] = i;
                colors[i] = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            }

            // Clear list of positions
            hitPositions.Clear();

            Mesh mesh = new Mesh();
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

            listOfMeshes.Add(mesh);

            finishedRayTracing = true;
        }

        void CreateMeshPolygons()
        {
            int totalVerticies = 0; // Used to make sure we don't exceed the 65000 mesh vertex limit

            for (int i = 0; i < hitPositions.Count; ++i) // For each successful hit position
            {
                PointCloudSphere pcs = AddSphereToMesh2(hitPositions[i]); // Create the point based on the hit location

                totalVerticies += pcs.verticies.Length;

                Mesh mesh = new Mesh();
                mesh.vertices = pcs.verticies; 
                mesh.normals = pcs.normals;
                mesh.uv = pcs.uvs;
                mesh.triangles = pcs.triangles;

                tempInstance.mesh = mesh;
                tempInstance.transform = pointCloud.transform.localToWorldMatrix;

                combineInstances.Add(tempInstance); // Add it to the list of meshes that need to be combined into one

                // If we're close to the vertex limit or we've done every hit position
                if (totalVerticies > 60000 || i == hitPositions.Count - 1)
                {
                    // Add it to the main list, meaning that nothing else will be added and it is ready to be combined/rendered
                    listOfCInstanceArrays.Add(combineInstances);

                    // Each item in listOfCInstanceArrays represents a mesh. We need mesh filters to render meshes. 
                    // If we have more meshses than mesh filters we will need to create a new one
                    if (listOfCInstanceArrays.Count > pcMeshes.Count)
                    {
                        CreateNewMeshFilter();
                    }

                    totalVerticies = 0; // Reset the vertex count as we are now dealing with a new mesh

                    combineInstances = new List<CombineInstance>();
                }
            }

            // For each combine instance, combine it into a single mesh and assign it to a mesh filter
            for (int i = 0; i < listOfCInstanceArrays.Count; i++)
            {
                pcMeshes[i].GetComponent<MeshFilter>().mesh.Clear();
                pcMeshes[i].GetComponent<MeshFilter>().mesh.CombineMeshes(listOfCInstanceArrays[i].ToArray());

                for (int j = 0; j < listOfCInstanceArrays[i].Count; j++)
                {
                    Destroy(listOfCInstanceArrays[i][j].mesh); // Fixes memory leak. Need to destroy all the created meshes manually
                }

                var o_205_17_636252801903164863 = pcMeshes[i].GetComponent<MeshFilter>().mesh;
                listOfMeshes.Add(pcMeshes[i].GetComponent<MeshFilter>().mesh);
            }

            // If we have less meshes than mesh filters that means that there could be objects on screen from previous frames
            // so we will manually clear them.
            for (int i = pcMeshes.Count - 1; i > listOfCInstanceArrays.Count - 1; i--)
            {
                pcMeshes[i].GetComponent<MeshFilter>().mesh.Clear();
            }

            hitPositions.Clear();
            listOfCInstanceArrays.Clear();

            finishedRayTracing = true;
        }

        void CreateMeshPolygonsOnServer()
        {
            int totalVerticies = 0; // Used to make sure we don't exceed the 65000 mesh vertex limit

            for (int i = 0; i < hitPositions.Count; ++i) // For each successful hit position
            {
                PointCloudSphere pcs = AddSphereToMesh2(hitPositions[i]); // Create the point based on the hit location

                totalVerticies += pcs.verticies.Length;

                Mesh mesh = new Mesh();
                mesh.vertices = pcs.verticies;
                mesh.normals = pcs.normals;
                mesh.uv = pcs.uvs;
                mesh.triangles = pcs.triangles;

                tempInstance.mesh = mesh;
                tempInstance.transform = pointCloud.transform.localToWorldMatrix;

                combineInstances.Add(tempInstance); // Add it to the list of meshes that need to be combined into one

                // If we're close to the vertex limit or we've done every hit position
                if (totalVerticies > 30000 || i == hitPositions.Count - 1)
                {
                    // Add it to the main list, meaning that nothing else will be added and it is ready to be combined/rendered
                    listOfCInstanceArrays.Add(combineInstances);

                    totalVerticies = 0; // Reset the vertex count as we are now dealing with a new mesh

                    combineInstances = new List<CombineInstance>();
                }
            }

            Mesh newMesh = new Mesh();

            // For each combine instance, combine it into a single mesh and assign it to a mesh filter
            for (int i = 0; i < listOfCInstanceArrays.Count; i++)
            {
                newMesh = new Mesh();
                newMesh.CombineMeshes(listOfCInstanceArrays[i].ToArray());
                ;
                listOfMeshes.Add(newMesh);

                for (int j = 0; j < listOfCInstanceArrays[i].Count; j++)
                {
                    Destroy(listOfCInstanceArrays[i][j].mesh); // Fixes memory leak. Need to destroy all the created meshes manually
                }
            }

            hitPositions.Clear();
            listOfCInstanceArrays.Clear();

            finishedRayTracing = true;
        }

        PointCloudSphere AddSphereToMesh(Vector3 pos)
        {
            #region Vertices
            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];

            vertices[0] = (Vector3.up * pointRadius) + pos;

            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = (new Vector3(sin1 * cos2, cos1, sin1 * sin2) * pointRadius) + pos;
                }
            }

            vertices[vertices.Length - 1] = (Vector3.up * -pointRadius) + pos;
            #endregion

            #region Normales		
            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normales[n] = vertices[n].normalized;
            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
                for (int lon = 0; lon <= nbLong; lon++)
                    uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
            #endregion

            #region Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }
            #endregion

            PointCloudSphere pcs = new PointCloudSphere();
            pcs.verticies = vertices;
            pcs.normals = normales;
            pcs.uvs = uvs;
            pcs.triangles = triangles;

            return pcs;
        }

        PointCloudSphere AddSphereToMesh2(Vector3 pos)
        {
            PointCloudSphere pcs = new PointCloudSphere();
            pcs.uvs = uvPool;
            pcs.verticies = new Vector3[vertexPool.Length];
            pcs.normals = new Vector3[vertexPool.Length];

            for (int i = 0; i < vertexPool.Length; i++)
            {
                pcs.verticies[i] = vertexPool[i] + pos;
                pcs.normals[i] = vertexPool[i].normalized;
            }

            pcs.triangles = trianglePool;

            return pcs;
        }

        void CreateUVs()
        {
            #region UVs
            uvPool = new Vector2[(nbLong + 1) * nbLat + 2];
            uvPool[0] = Vector2.up;
            uvPool[uvPool.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
                for (int lon = 0; lon <= nbLong; lon++)
                    uvPool[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
            #endregion
        }

        void CreateVerticies()
        {
            #region Vertices
            vertexPool = new Vector3[(nbLong + 1) * nbLat + 2];

            vertexPool[0] = (Vector3.up * pointRadius);

            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertexPool[lon + lat * (nbLong + 1) + 1] = (new Vector3(sin1 * cos2, cos1, sin1 * sin2) * pointRadius);
                }
            }

            vertexPool[vertexPool.Length - 1] = (Vector3.up * -pointRadius);

            #endregion
        }

        void CreateTriangles()
        {
            #region Triangles
            int nbFaces = vertexPool.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            trianglePool = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                trianglePool[i++] = lon + 2;
                trianglePool[i++] = lon + 1;
                trianglePool[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    trianglePool[i++] = current;
                    trianglePool[i++] = current + 1;
                    trianglePool[i++] = next + 1;

                    trianglePool[i++] = current;
                    trianglePool[i++] = next + 1;
                    trianglePool[i++] = next;
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                trianglePool[i++] = vertexPool.Length - 1;
                trianglePool[i++] = vertexPool.Length - (lon + 2) - 1;
                trianglePool[i++] = vertexPool.Length - (lon + 1) - 1;
            }
            #endregion
        }

        /// <summary>
        /// Calculates the vector3 in direction of an given angle to an detected GameObject
        /// </summary>
        /// <param name="angleInDegrees"></param>
        /// <param name="angleIsGlobal"></param>
        /// <returns>Vector3 in direction of an given angle</returns>
        public Vector3 GetDirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegrees += transform.eulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        public void CreateNewMeshFilter()
        {
            GameObject newMesh = Instantiate(pointCloudMesh) as GameObject;
            newMesh.transform.parent = pointCloud.transform;
            newMesh.GetComponent<MeshFilter>().mesh = new Mesh();
            pcMeshes.Add(newMesh);
        }

        //void BuildSphere()
        //{
        //    float radius = 0.1f;
        //    // Longitude |||
        //    int nbLong = 24;
        //    // Latitude ---
        //    int nbLat = 16;

        //    #region Vertices
        //    Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
        //    float _pi = Mathf.PI;
        //    float _2pi = _pi * 2f;

        //    vertices[0] = Vector3.up * radius + new Vector3(5, 5, 5);

        //    for (int lat = 0; lat < nbLat; lat++)
        //    {
        //        float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
        //        float sin1 = Mathf.Sin(a1);
        //        float cos1 = Mathf.Cos(a1);

        //        for (int lon = 0; lon <= nbLong; lon++)
        //        {
        //            float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
        //            float sin2 = Mathf.Sin(a2);
        //            float cos2 = Mathf.Cos(a2);

        //            vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius + new Vector3(5, 5, 5);
        //        }
        //    }

        //    vertices[vertices.Length - 1] = Vector3.up * -radius + new Vector3(5, 5, 5);
        //    #endregion

        //    #region Normales		
        //    Vector3[] normales = new Vector3[vertices.Length];
        //    for (int n = 0; n < vertices.Length; n++)
        //        normales[n] = vertices[n].normalized;
        //    #endregion

        //    #region UVs
        //    Vector2[] uvs = new Vector2[vertices.Length];
        //    uvs[0] = Vector2.up;
        //    uvs[uvs.Length - 1] = Vector2.zero;
        //    for (int lat = 0; lat < nbLat; lat++)
        //        for (int lon = 0; lon <= nbLong; lon++)
        //            uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        //    #endregion

        //    #region Triangles
        //    int nbFaces = vertices.Length;
        //    int nbTriangles = nbFaces * 2;
        //    int nbIndexes = nbTriangles * 3;
        //    int[] triangles = new int[nbIndexes];

        //    //Top Cap
        //    int i = 0;
        //    for (int lon = 0; lon < nbLong; lon++)
        //    {
        //        triangles[i++] = lon + 2;
        //        triangles[i++] = lon + 1;
        //        triangles[i++] = 0;
        //    }

        //    //Middle
        //    for (int lat = 0; lat < nbLat - 1; lat++)
        //    {
        //        for (int lon = 0; lon < nbLong; lon++)
        //        {
        //            int current = lon + lat * (nbLong + 1) + 1;
        //            int next = current + nbLong + 1;

        //            triangles[i++] = current;
        //            triangles[i++] = current + 1;
        //            triangles[i++] = next + 1;

        //            triangles[i++] = current;
        //            triangles[i++] = next + 1;
        //            triangles[i++] = next;
        //        }
        //    }

        //    //Bottom Cap
        //    for (int lon = 0; lon < nbLong; lon++)
        //    {
        //        triangles[i++] = vertices.Length - 1;
        //        triangles[i++] = vertices.Length - (lon + 2) - 1;
        //        triangles[i++] = vertices.Length - (lon + 1) - 1;
        //    }
        //    #endregion

        //    pointCloud.mesh.Clear();

        //    pointCloud.mesh.vertices = vertices;
        //    pointCloud.mesh.normals = normales;
        //    pointCloud.mesh.uv = uvs;
        //    pointCloud.mesh.triangles = triangles;

        //    pointCloud.mesh.RecalculateBounds();
        //    pointCloud.mesh.Optimize();
        //}
    }
}
