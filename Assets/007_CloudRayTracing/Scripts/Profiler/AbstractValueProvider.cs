using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    [DisallowMultipleComponent]
    public abstract class AbstractValueProvider : MonoBehaviour
    {
        public string NumberFormat = "#,##0";
        public string Title = "";

        internal AbstractValueProvider()
        {
        }

        public virtual void Refresh(float readInterval)
        {
        }

        public abstract float Value { get; }
    }
}

