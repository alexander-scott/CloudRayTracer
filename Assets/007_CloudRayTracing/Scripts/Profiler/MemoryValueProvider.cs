using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class MemoryValueProvider : AbstractValueProvider
    {
        public override float Value
        {
            get { return ((float)GC.GetTotalMemory(false)); }
        }
    }
}

