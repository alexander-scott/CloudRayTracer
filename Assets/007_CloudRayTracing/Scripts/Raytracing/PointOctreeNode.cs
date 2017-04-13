using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class PointOctreeNode
    {
        // Centre of this node
        public Vector3 Center { get; private set; }
        // Length of the sides of this node
        public float SideLength { get; private set; }
        // Objects in this node
        public List<Vector3> objects = new List<Vector3>();
        // Child nodes, if any
        public PointOctreeNode[] children = null;

        // Minimum size for a node in this octree
        private float minSize;
        // Bounding box that represents this node
        private Bounds bounds = default(Bounds);
        // bounds of potential children to this node. These are actual size (with looseness taken into account), not base size
        private Bounds[] childBounds;
        // If there are already numObjectsAllowed in a node, we split it into children
        // A generally good number seems to be something around 8-15
        private const int NUM_OBJECTS_ALLOWED = 15;
        // For reverting the bounds size after temporary changes
        private Vector3 actualBoundsSize;

        public PointOctreeNode(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, centerVal);
        }

        public bool Add(Vector3 objPos)
        {
            if (!Encapsulates(bounds, objPos))
            {
                return false;
            }
            SubAdd(objPos);
            return true;
        }

        public bool Remove(Vector3 objPos)
        {
            bool removed = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Equals(objPos))
                {
                    removed = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!removed && children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    removed = children[i].Remove(objPos);
                    if (removed) break;
                }
            }

            if (removed && children != null)
            {
                // Check if we should merge nodes now that we've removed an item
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        public bool CheckNearby(ref bool result, ref Vector3 pos, ref float maxDistance)
        {
            if (result)
                return result;

            // Does the point exist within this node?
            // Note: Expanding the bounds is not exactly the same as a real distance check, but it's fast.
            bounds.Expand(new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
            bool intersected = bounds.Contains(pos);
            bounds.size = actualBoundsSize;

            if (!intersected)
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
            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    children[i].CheckNearby(ref result, ref pos, ref maxDistance);
                }
            }

            return false;
        }

        public void SetChildren(PointOctreeNode[] childOctrees)
        {
            if (childOctrees.Length != 8)
            {
                Debug.LogError("Child octree array must be length 8. Was length: " + childOctrees.Length);
                return;
            }

            children = childOctrees;
        }

        public void GetPositionsIncludingChildren(ref List<Vector3> positions)
        {
            positions.AddRange(objects);

            if (children == null)
                return;

            foreach (PointOctreeNode node in children)
            {
                node.GetPositionsIncludingChildren(ref positions);
            }
        }

        public void ClearAll()
        {
            objects.Clear();

            if (children == null)
                return;

            foreach (PointOctreeNode node in children)
            {
                node.ClearAll();
            }
        }

        public PointOctreeNode ShrinkIfPossible(float minLength)
        {
            if (SideLength < (2 * minLength))
            {
                return this;
            }
            if (objects.Count == 0 && children.Length == 0)
            {
                return this;
            }

            // Check objects in root
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
                    return this; // Can't reduce - objects fit in different octants
                }
            }

            // Check objects in children if there are any
            if (children != null)
            {
                bool childHadContent = false;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].HasAnyObjects())
                    {
                        if (childHadContent)
                        {
                            return this; // Can't shrink - another child had content already
                        }
                        if (bestFit >= 0 && bestFit != i)
                        {
                            return this; // Can't reduce - objects in root are in a different octant to objects in child
                        }
                        childHadContent = true;
                        bestFit = i;
                    }
                }
            }

            // Can reduce
            if (children == null)
            {
                // We don't have any children, so just shrink this node to the new size
                // We already know that everything will still fit in it
                SetValues(SideLength / 2, minSize, childBounds[bestFit].center);
                return this;
            }

            // We have children. Use the appropriate child as the new root node
            return children[bestFit];
        }

        private void SetValues(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SideLength = baseLengthVal;
            minSize = minSizeVal;
            Center = centerVal;

            // Create the bounding box.
            actualBoundsSize = new Vector3(SideLength, SideLength, SideLength);
            bounds = new Bounds(Center, actualBoundsSize);

            float quarter = SideLength / 4f;
            float childActualLength = SideLength / 2;
            Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
            childBounds = new Bounds[8];
            childBounds[0] = new Bounds(Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
            childBounds[1] = new Bounds(Center + new Vector3(quarter, quarter, -quarter), childActualSize);
            childBounds[2] = new Bounds(Center + new Vector3(-quarter, quarter, quarter), childActualSize);
            childBounds[3] = new Bounds(Center + new Vector3(quarter, quarter, quarter), childActualSize);
            childBounds[4] = new Bounds(Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            childBounds[5] = new Bounds(Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
            childBounds[6] = new Bounds(Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
            childBounds[7] = new Bounds(Center + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        private void SubAdd(Vector3 objPos)
        {
            // We know it fits at this level if we've got this far
            // Just add if few objects are here, or children would be below min size
            if (objects.Count < NUM_OBJECTS_ALLOWED || (SideLength / 2) < minSize)
            {
                Vector3 newObj = new Vector3(objPos.x, objPos.y, objPos.z);
                //Debug.Log("ADD " + obj.name + " to depth " + depth);
                objects.Add(newObj);
            }
            else
            { // Enough objects in this node already: Create new children
              // Create the 8 children
                int bestFitChild;
                if (children == null)
                {
                    Split();
                    if (children == null)
                    {
                        Debug.Log("Child creation failed for an unknown reason. Early exit.");
                        return;
                    }

                    // Now that we have the new children, see if this node's existing objects would fit there
                    for (int i = objects.Count - 1; i >= 0; i--)
                    {
                        Vector3 existingObj = objects[i];
                        // Find which child the object is closest to based on where the
                        // object's center is located in relation to the octree's center.
                        bestFitChild = BestFitChild(existingObj);
                        children[bestFitChild].SubAdd(existingObj); // Go a level deeper					
                        objects.Remove(existingObj); // Remove from here
                    }
                }

                // Now handle the new object we're adding now
                bestFitChild = BestFitChild(objPos);
                children[bestFitChild].SubAdd(objPos);
            }
        }

        /// <summary>
        /// Splits the octree into eight children.
        /// </summary>
        private void Split()
        {
            float quarter = SideLength / 4f;
            float newLength = SideLength / 2;
            children = new PointOctreeNode[8];
            children[0] = new PointOctreeNode(newLength, minSize, Center + new Vector3(-quarter, quarter, -quarter));
            children[1] = new PointOctreeNode(newLength, minSize, Center + new Vector3(quarter, quarter, -quarter));
            children[2] = new PointOctreeNode(newLength, minSize, Center + new Vector3(-quarter, quarter, quarter));
            children[3] = new PointOctreeNode(newLength, minSize, Center + new Vector3(quarter, quarter, quarter));
            children[4] = new PointOctreeNode(newLength, minSize, Center + new Vector3(-quarter, -quarter, -quarter));
            children[5] = new PointOctreeNode(newLength, minSize, Center + new Vector3(quarter, -quarter, -quarter));
            children[6] = new PointOctreeNode(newLength, minSize, Center + new Vector3(-quarter, -quarter, quarter));
            children[7] = new PointOctreeNode(newLength, minSize, Center + new Vector3(quarter, -quarter, quarter));
        }

        /// </summary>
        private void Merge()
        {
            // Note: We know children != null or we wouldn't be merging
            for (int i = 0; i < 8; i++)
            {
                PointOctreeNode curChild = children[i];
                int numObjects = curChild.objects.Count;
                for (int j = numObjects - 1; j >= 0; j--)
                {
                    Vector3 curObj = curChild.objects[j];
                    objects.Add(curObj);
                }
            }
            // Remove the child nodes (and the objects in them - they've been added elsewhere now)
            children = null;
        }

        private static bool Encapsulates(Bounds outerBounds, Vector3 point)
        {
            return outerBounds.Contains(point);
        }

        private int BestFitChild(Vector3 objPos)
        {
            return (objPos.x <= Center.x ? 0 : 1) + (objPos.y >= Center.y ? 0 : 4) + (objPos.z <= Center.z ? 0 : 2);
        }

        private bool ShouldMerge()
        {
            int totalObjects = objects.Count;
            if (children != null)
            {
                foreach (PointOctreeNode child in children)
                {
                    if (child.children != null)
                    {
                        // If any of the *children* have children, there are definitely too many to merge,
                        // or the child woudl have been merged already
                        return false;
                    }
                    totalObjects += child.objects.Count;
                }
            }
            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }

        private bool HasAnyObjects()
        {
            if (objects.Count > 0) return true;

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i].HasAnyObjects()) return true;
                }
            }

            return false;
        }


        public static float DistanceToRay(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }
    }
}