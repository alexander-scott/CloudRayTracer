using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class Graphic
    {
        private readonly Color32[] _background;
        private readonly Color32[] _data;
        private readonly int _size;
        private readonly Texture2D _texture;
        private readonly int _width;

        public Graphic(int width, int height, Color32 color)
        {
            this._width = Mathf.Max(width, 1);
            height = Mathf.Max(height, 1);
            Texture2D textured1 = new Texture2D(this._width, height) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
            this._texture = textured1;
            this._size = this._width * height;
            this._background = new Color32[this._size];
            this._data = new Color32[this._size];
            this.Clear(color, true);
        }

        public void Apply()
        {
            this._texture.SetPixels32(this._data);
            this._texture.Apply();
        }

        public void Clear(Color32 color, bool apply)
        {
            if (!Color32Equal(color, this._background[0]))
            {
                this.SetBackgroundColor(color);
            }
            Array.Copy(this._background, this._data, this._size);
            if (apply)
            {
                this.Apply();
            }
        }

        private static bool Color32Equal(Color32 a, Color32 b)
        {
            return ((((a.r == b.r) && (a.g == b.g)) && (a.b == b.b)) && (a.a == b.a));
        } 

        public void Destroy()
        {
            UnityEngine.Object.DestroyImmediate(this._texture);
        }

        public void DrawRect(int x, int y, int with, int height, Color32 color, int average, Color32 averageColor)
        {
            int num = x + with;
            while (x < num)
            {
                for (int i = y; i < height; i++)
                {
                    this._data[(i * this._width) + x] = color;
                }
                this._data[(average * this._width) + x] = averageColor;
                x++;
            }
        }

        public void SetBackgroundColor(Color32 color)
        {
            for (int i = 0; i < this._size; i++)
            {
                this._background[i] = color;
            }
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }
    }
}

