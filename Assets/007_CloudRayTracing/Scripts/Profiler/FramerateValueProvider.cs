 using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class FramerateValueProvider : AbstractValueProvider
    {
        private float dt;
        private float fps;
        private int frameCount;

        public override void Refresh(float readInterval)
        {
            if (readInterval < Time.unscaledDeltaTime)
            {
                fps = 1f / Time.unscaledDeltaTime;
            }
            else
            {
                frameCount++;
                dt += Time.unscaledDeltaTime;
                if (dt >= (1f * readInterval))
                {
                    fps = frameCount / dt;
                    frameCount = 0;
                    dt -= 1f * readInterval;
                }
            }
        }

        public override float Value
        {
            get { return fps; }
        }
    }
}

