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
            _width = Mathf.Max(width, 1);
            height = Mathf.Max(height, 1);
            Texture2D textured1 = new Texture2D(_width, height) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
            _texture = textured1;
            _size = _width * height;
            _background = new Color32[_size];
            _data = new Color32[_size];
            Clear(color, true);
        }

        public void Apply()
        {
            _texture.SetPixels32(_data);
            _texture.Apply();
        }

        public void Clear(Color32 color, bool apply)
        {
            if (!Color32Equal(color, _background[0]))
            {
                SetBackgroundColor(color);
            }
            Array.Copy(_background, _data, _size);
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
            UnityEngine.Object.DestroyImmediate(_texture);
        }

        public void DrawRect(int x, int y, int with, int height, Color32 color, int average, Color32 averageColor)
        {
            int num = x + with;
            while (x < num)
            {
                for (int i = y; i < height; i++)
                {
                    _data[(i * _width) + x] = color;
                }
                _data[(average * _width) + x] = averageColor;
                x++;
            }
        }

        public void SetBackgroundColor(Color32 color)
        {
            for (int i = 0; i < _size; i++)
            {
                _background[i] = color;
            }
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }
    }
}

