using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public static class Texture2DUtil
    {
        private static Color32[] background;

        public static void Clear(Texture2D texture, Color32 color)
        {
            if (texture != null)
            {
                int num = texture.width * texture.height;
                if ((background == null) || (background.Length != num))
                {
                    background = new Color32[num];
                }
                if (num != 0)
                {
                    for (int i = 0; i < background.Length; i++)
                    {
                        background[i] = color;
                    }
                    texture.SetPixels32(background);
                    texture.Apply();
                }
            }
        }
    }
}

