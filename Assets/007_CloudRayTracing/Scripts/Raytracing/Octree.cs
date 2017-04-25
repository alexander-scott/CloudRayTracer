using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Octree
    {
        public int ObjectCount { get; private set; }

        private OctreeNode rootNode;
        private float initialSize;
        private float minimumNodeSize;

        public Octree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                minNodeSize = initialWorldSize;
            }

            ObjectCount = 0;
            initialSize = initialWorldSize;
            minimumNodeSize = minNodeSize;
            rootNode = new OctreeNode(initialSize, minimumNodeSize, initialWorldPos);
        }

        public void Add(Vector3 objPos)
        {
            int count = 0; 

            while (!rootNode.AddObject(objPos))
            {
                Grow(objPos - rootNode.Origin);

                if (++count > 20)
                {
                    return;
                }
            }

            ObjectCount++;
        }

        public List<Vector3> GetAllPositions()
        {
            List<Vector3> returnList = new List<Vector3>();
            rootNode.GetAllObjects(ref returnList);

            return returnList;
        }

        public bool Remove(Vector3 objPos)
        {
            bool removed = rootNode.RemoveObject(objPos);

            if (removed)
            {
                ObjectCount--;
                Shrink();
            }

            return removed;
        }

        public bool CheckNearby(Vector3 point, float maxDistance)
        {
            bool result = false;
            rootNode.CheckNearbyObjects(ref result, ref point, ref maxDistance);

            return result;
        }

        private void Grow(Vector3 direction)
        {
            int xDirection = direction.x >= 0 ? 1 : -1;
            int yDirection = direction.y >= 0 ? 1 : -1;
            int zDirection = direction.z >= 0 ? 1 : -1;

            OctreeNode oldRoot = rootNode;
            float half = rootNode.NodeSideLength / 2;
            float newLength = rootNode.NodeSideLength * 2;

            Vector3 newCenter = rootNode.Origin + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            rootNode = new OctreeNode(newLength, minimumNodeSize, newCenter);

            int rootPos = GetRootPosIndex(xDirection, yDirection, zDirection);
            OctreeNode[] children = new OctreeNode[8];

            for (int i = 0; i < 8; i++)
            {
                if (i == rootPos)
                {
                    children[i] = oldRoot;
                }
                else
                {
                    xDirection = i % 2 == 0 ? -1 : 1;
                    yDirection = i > 3 ? -1 : 1;
                    zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                    children[i] = new OctreeNode(rootNode.NodeSideLength, minimumNodeSize, newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                }
            }

            rootNode.SetChildNodes(children);
        }

        private void Shrink()
        {
            rootNode = rootNode.ShrinkOctree(initialSize);
        }

        private static int GetRootPosIndex(int xDir, int yDir, int zDir)
        {
            int result = xDir > 0 ? 1 : 0;
            if (yDir < 0) result += 4;
            if (zDir > 0) result += 2;
            return result;
        }
    }
}