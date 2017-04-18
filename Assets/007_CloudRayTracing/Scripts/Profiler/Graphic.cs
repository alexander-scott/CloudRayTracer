using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Graphic
    {
        private Color32[] background;
        private Color32[] data;
        private int size;
        private Texture2D texture;
        private int width;

        public Graphic(int width, int height, Color32 color)
        {
            this.width = Mathf.Max(width, 1);
            height = Mathf.Max(height, 1);
            Texture2D textured1 = new Texture2D(this.width, height) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
            texture = textured1;
            size = this.width * height;
            background = new Color32[size];
            data = new Color32[size];
            Clear(color, true);
        }

        public void Apply()
        {
            texture.SetPixels32(data);
            texture.Apply();
        }

        public void Clear(Color32 color, bool apply)
        {
            if (!Color32Equal(color, background[0]))
            {
                SetBackgroundColor(color);
            }
            Array.Copy(background, data, size);
            if (apply)
            {
                Apply();
            }
        }

        private static bool Color32Equal(Color32 a, Color32 b)
        {
            return ((((a.r == b.r) && (a.g == b.g)) && (a.b == b.b)) && (a.a == b.a));
        } 

        public void Destroy()
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }

        public void DrawRect(int x, int y, int with, int height, Color32 color, int average, Color32 averageColor)
        {
            int num = x + with;
            while (x < num)
            {
                for (int i = y; i < height; i++)
                {
                    data[(i * width) + x] = color;
                }
                data[(average * width) + x] = averageColor;
                x++;
            }
        }

        public void SetBackgroundColor(Color32 color)
        {
            for (int i = 0; i < size; i++)
            {
                background[i] = color;
            }
        }

        public Texture2D Texture
        {
            get { return texture; }
        }
    }
}

