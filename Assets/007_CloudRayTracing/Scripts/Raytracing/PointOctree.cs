using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class PointOctree
    {
        public int Count { get; private set; }

        private PointOctreeNode rootNode;
        private float initialSize;
        private float minSize;

        public PointOctree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }

            Count = 0;
            initialSize = initialWorldSize;
            minSize = minNodeSize;
            rootNode = new PointOctreeNode(initialSize, minSize, initialWorldPos);
        }

        public void Add(Vector3 objPos)
        {
            // Add object or expand the octree until it can be added
            int count = 0; // Safety check against infinite/excessive growth
            while (!rootNode.Add(objPos))
            {
                Grow(objPos - rootNode.Center);
                if (++count > 20)
                {
                    Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (count - 1) + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }

        public List<Vector3> GetAllPositions()
        {
            List<Vector3> returnList = new List<Vector3>();
            rootNode.GetPositionsIncludingChildren(ref returnList);
            return returnList;
        }

        public void ClearAll()
        {
            rootNode.ClearAll();
        }

        public bool Remove(Vector3 objPos)
        {
            bool removed = rootNode.Remove(objPos);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        public bool CheckNearby(Vector3 point, float maxDistance)
        {
            bool result = false;
            rootNode.CheckNearby(ref result, ref point, ref maxDistance);
            return result;
        }

        private void Grow(Vector3 direction)
        {
            int xDirection = direction.x >= 0 ? 1 : -1;
            int yDirection = direction.y >= 0 ? 1 : -1;
            int zDirection = direction.z >= 0 ? 1 : -1;
            PointOctreeNode oldRoot = rootNode;
            float half = rootNode.SideLength / 2;
            float newLength = rootNode.SideLength * 2;
            Vector3 newCenter = rootNode.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            rootNode = new PointOctreeNode(newLength, minSize, newCenter);

            // Create 7 new octree children to go with the old root as children of the new root
            int rootPos = GetRootPosIndex(xDirection, yDirection, zDirection);
            PointOctreeNode[] children = new PointOctreeNode[8];
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
                    children[i] = new PointOctreeNode(rootNode.SideLength, minSize, newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                }
            }

            // Attach the new children to the new root node
            rootNode.SetChildren(children);
        }

        private void Shrink()
        {
            rootNode = rootNode.ShrinkIfPossible(initialSize);
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