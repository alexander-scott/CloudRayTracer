 using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class FramerateValueProvider : AbstractValueProvider
    {
        private float _dt;
        private float _fps;
        private int _frameCount;

        public override void Refresh(float readInterval)
        {
            if (readInterval < Time.unscaledDeltaTime)
            {
                this._fps = 1f / Time.unscaledDeltaTime;
            }
            else
            {
                this._frameCount++;
                this._dt += Time.unscaledDeltaTime;
                if (this._dt >= (1f * readInterval))
                {
                    this._fps = ((float) this._frameCount) / this._dt;
                    this._frameCount = 0;
                    this._dt -= 1f * readInterval;
                }
            }
        }

        public override float Value
        {
            get { return _fps; }
        }
    }
}

