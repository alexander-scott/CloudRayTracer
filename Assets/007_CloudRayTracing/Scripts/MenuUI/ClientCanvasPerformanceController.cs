using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasPerformanceController : MonoBehaviour
    {
        [Header("Label references")]
        public Text fps;
        public Text averageFps;
        public Text minFps;
        public Text maxFps;
        public Text memTotal;
        public Text memAlloc;

        [Space(10)]
        [Header("Options")]

        public float refreshRate = 0.5f;

        private float count = 0f;
        private float totalFps = 0f;

        private float minFPS = 60f;
        private float maxFPS = 60f;

        private PerformanceCounter currentMemCounter;
        private PerformanceCounter ramCounter;

        private float totalMemory;

        void Start()
        {
            currentMemCounter = new PerformanceCounter("Memory", "Available MBytes");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            totalMemory = ramCounter.NextValue();

            memTotal.text = "MEM TOTAL: " + totalMemory + " MB";
            memTotal.color = Color.yellow;

            Timing.RunCoroutine(DisplayFPS(), "FPSCounter");
        }

        private IEnumerator<float> DisplayFPS()
        {
            while (true)
            {
                float fpsVal = 1.0f / Time.deltaTime;

                CalcFPS(fpsVal);
                CalcAVG(fpsVal);
                CalcMINMAX(fpsVal);
                CalcMemAlloc();

                yield return Timing.WaitForSeconds(refreshRate);
            }
        }

        private void CalcFPS(float fpsVal)
        {
            fps.text = "FPS: " + Mathf.Floor(fpsVal);

            if (fpsVal > 60f)
            {
                fps.color = Color.green;
            }
            else if (fpsVal < 30f)
            {
                fps.color = Color.red;
            }
            else
            {
                fps.color = Color.yellow;
            }
        }

        private void CalcAVG(float fpsVal)
        {
            totalFps += fpsVal;

            count++;

            float avgFpsVal = totalFps / count;
            averageFps.text = "AVG: " + Mathf.Floor(avgFpsVal);

            if (avgFpsVal > 60f)
            {
                averageFps.color = Color.green;
            }
            else if (avgFpsVal < 30f)
            {
                averageFps.color = Color.red;
            }
            else
            {
                averageFps.color = Color.yellow;
            }
        }

        private void CalcMINMAX(float fpsVal)
        {
            if (fpsVal < minFPS)
            {
                minFPS = fpsVal;
            }

            if (fpsVal > maxFPS)
            {
                maxFPS = fpsVal;
            }

            if (minFPS > 60f)
            {
                minFps.color = Color.green;
            }
            else if (minFPS < 30f)
            {
                minFps.color = Color.red;
            }
            else
            {
                minFps.color = Color.yellow;
            }

            if (maxFPS > 60f)
            {
                maxFps.color = Color.green;
            }
            else if (maxFPS < 30f)
            {
                maxFps.color = Color.red;
            }
            else
            {
                maxFps.color = Color.yellow;
            }

            minFps.text = "MIN: " + Mathf.Floor(minFPS);

            maxFps.text = "MAX: " + Mathf.Floor(maxFPS);
        }

        private void CalcMemAlloc()
        {
            float memoryAlloc = totalMemory - currentMemCounter.NextValue();

            memAlloc.text = "MEM ALLOC: " + memoryAlloc + " MB";
            memAlloc.color = Color.yellow;
        }
    }
}
