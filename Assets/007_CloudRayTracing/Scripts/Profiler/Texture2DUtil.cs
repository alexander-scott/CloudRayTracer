using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public static class Texture2DUtil
    {
        private static Color32[] _background;

        public static void Clear(Texture2D texture, Color32 color)
        {
            if (texture != null)
            {
                int num = texture.width * texture.height;
                if ((_background == null) || (_background.Length != num))
                {
                    _background = new Color32[num];
                }
                if (num != 0)
                {
                    for (int i = 0; i < _background.Length; i++)
                    {
                        _background[i] = color;
                    }
                    texture.SetPixels32(_background);
                    texture.Apply();
                }
            }
        }
    }
}

