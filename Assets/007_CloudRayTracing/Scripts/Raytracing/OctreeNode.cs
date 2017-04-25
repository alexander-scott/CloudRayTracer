using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class OctreeNode
    {
        public Vector3 Origin { get; set; } 
        public float NodeSideLength { get; set; }

        private float minimumNodeSize;
        private Bounds nodeBounds = default(Bounds);
        private Bounds[] childNodeBounds;
        private Vector3 actualBoundsSize;

        private List<Vector3> objects = new List<Vector3>();
        private OctreeNode[] childNodes = null;

        public OctreeNode(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, centerVal);
        }

        public bool AddObject(Vector3 obj)
        {
            if (!EncapsulatesBounds(nodeBounds, obj))
            {
                return false;
            }
            SubAdd(obj);
            return true;
        }

        public bool RemoveObject(Vector3 obj)
        {
            bool objectRemoved = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Equals(obj))
                {
                    objectRemoved = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!objectRemoved && childNodes != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    objectRemoved = childNodes[i].RemoveObject(obj);

                    if (objectRemoved)
                    {
                        break;
                    }
                }
            }

            if (objectRemoved && childNodes != null)
            {
                if (CheckMergeNodes())
                {
                    MergeNodes();
                }
            }

            return objectRemoved;
        }

        // Returns true if a vector3 position is nearby to another vector3 position stored in this octree
        public bool CheckNearbyObjects(ref bool result, ref Vector3 pos, ref float maxDistance)
        {
            if (result)
            {
                return result;
            }
                
            nodeBounds.Expand(new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
            bool intersectedBounds = nodeBounds.Contains(pos);
            nodeBounds.size = actualBoundsSize;

            if (!intersectedBounds)
            {
                return false;
            }

            // Check against any objects in this node
            for (int i = 0; i < objects.Count; i++)
            {
                if ((pos - objects[i]).sqrMagnitude <= maxDistance)
                {
                    result = true;
                    return true;
                }
            }

            // Check children
            if (childNodes != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    childNodes[i].CheckNearbyObjects(ref result, ref pos, ref maxDistance);
                }
            }

            return false;
        }

        public void SetChildNodes(OctreeNode[] childOctrees)
        {
            if (childOctrees.Length != 8)
            {
                return;
            }

            childNodes = childOctrees;
        }

        // Returns all objects including children
        public void GetAllObjects(ref List<Vector3> objectList)
        {
            objectList.AddRange(objects);

            if (childNodes == null)
            {
                return;
            }
                
            foreach (OctreeNode node in childNodes)
            {
                node.GetAllObjects(ref objectList);
            }
        }

        public OctreeNode ShrinkOctree(float minSideLength)
        {
            if (NodeSideLength < (2 * minSideLength))
            {
                return this;
            }

            if (objects.Count == 0 && childNodes.Length == 0)
            {
                return this;
            }

            int bestFit = -1;
            for (int i = 0; i < objects.Count; i++)
            {
                Vector3 curObj = objects[i];
                int newBestFit = BestFitChild(curObj);
                if (i == 0 || newBestFit == bestFit)
                {
                    if (bestFit < 0)
                    {
                        bestFit = newBestFit;
                    }
                }
                else
                {
                    return this; 
                }
            }

            if (childNodes != null)
            {
                bool childHadContent = false;

                for (int i = 0; i < childNodes.Length; i++)
                {
                    if (childNodes[i].ContainsObjects())
                    {
                        if (childHadContent)
                        {
                            return this; 
                        }

                        if (bestFit >= 0 && bestFit != i)
                        {
                            return this; 
                        }

                        childHadContent = true;
                        bestFit = i;
                    }
                }
            }

            if (childNodes == null)
            {
                SetValues(NodeSideLength / 2, minimumNodeSize, childNodeBounds[bestFit].center);
                return this;
            }

            return childNodes[bestFit];
        }

        private void SetValues(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            NodeSideLength = baseLengthVal;
            minimumNodeSize = minSizeVal;
            Origin = centerVal;

            actualBoundsSize = new Vector3(NodeSideLength, NodeSideLength, NodeSideLength);
            nodeBounds = new Bounds(Origin, actualBoundsSize);

            float quarter = NodeSideLength / 4f;
            float childActualLength = NodeSideLength / 2;
            Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
            childNodeBounds = new Bounds[8];
            childNodeBounds[0] = new Bounds(Origin + new Vector3(-quarter, quarter, -quarter), childActualSize);
            childNodeBounds[1] = new Bounds(Origin + new Vector3(quarter, quarter, -quarter), childActualSize);
            childNodeBounds[2] = new Bounds(Origin + new Vector3(-quarter, quarter, quarter), childActualSize);
            childNodeBounds[3] = new Bounds(Origin + new Vector3(quarter, quarter, quarter), childActualSize);
            childNodeBounds[4] = new Bounds(Origin + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            childNodeBounds[5] = new Bounds(Origin + new Vector3(quarter, -quarter, -quarter), childActualSize);
            childNodeBounds[6] = new Bounds(Origin + new Vector3(-quarter, -quarter, quarter), childActualSize);
            childNodeBounds[7] = new Bounds(Origin + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        private void SubAdd(Vector3 objPos)
        {
            if (objects.Count < DataController.Instance.octreeMaxObjects || (NodeSideLength / 2) < minimumNodeSize)
            {
                Vector3 newObj = new Vector3(objPos.x, objPos.y, objPos.z);
                objects.Add(newObj);
            }
            else
            { 
                int bestFitChild;
                if (childNodes == null)
                {
                    Split();
                    if (childNodes == null)
                    {
                        return;
                    }

                    for (int i = objects.Count - 1; i >= 0; i--)
                    {
                        Vector3 existingObj = objects[i];
                        bestFitChild = BestFitChild(existingObj);

                        childNodes[bestFitChild].SubAdd(existingObj);			
                        objects.Remove(existingObj); 
                    }
                }

                bestFitChild = BestFitChild(objPos);
                childNodes[bestFitChild].SubAdd(objPos);
            }
        }


        private void Split()
        {
            float quarter = NodeSideLength / 4f;
            float newLength = NodeSideLength / 2;

            childNodes = new OctreeNode[8];

            childNodes[0] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(-quarter, quarter, -quarter));
            childNodes[1] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(quarter, quarter, -quarter));
            childNodes[2] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(-quarter, quarter, quarter));
            childNodes[3] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(quarter, quarter, quarter));
            childNodes[4] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(-quarter, -quarter, -quarter));
            childNodes[5] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(quarter, -quarter, -quarter));
            childNodes[6] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(-quarter, -quarter, quarter));
            childNodes[7] = new OctreeNode(newLength, minimumNodeSize, Origin + new Vector3(quarter, -quarter, quarter));
        }

        private void MergeNodes()
        {
            // Note: We know children != null or we wouldn't be merging
            for (int i = 0; i < 8; i++)
            {
                OctreeNode curChild = childNodes[i];
                int numObjects = curChild.objects.Count;
                for (int j = numObjects - 1; j >= 0; j--)
                {
                    Vector3 curObj = curChild.objects[j];
                    objects.Add(curObj);
                }
            }

            childNodes = null;
        }

        private bool CheckMergeNodes()
        {
            int totalObjects = objects.Count;
            if (childNodes != null)
            {
                foreach (OctreeNode child in childNodes)
                {
                    if (child.childNodes != null)
                    {
                        return false;
                    }
                    totalObjects += child.objects.Count;
                }
            }
            return totalObjects <= DataController.Instance.octreeMaxObjects;
        }

        private bool ContainsObjects()
        {
            if (objects.Count > 0)
            {
                return true;
            }

            if (childNodes != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (childNodes[i].ContainsObjects())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EncapsulatesBounds(Bounds outerBounds, Vector3 point)
        {
            return outerBounds.Contains(point);
        }

        private int BestFitChild(Vector3 objPos)
        {
            return (objPos.x <= Origin.x ? 0 : 1) + (objPos.y >= Origin.y ? 0 : 4) + (objPos.z <= Origin.z ? 0 : 2);
        }
    }
}