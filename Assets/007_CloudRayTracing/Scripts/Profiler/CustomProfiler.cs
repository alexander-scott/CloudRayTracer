using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class CustomProfiler : MonoBehaviour
    {
        public bool ColorizeText = true;
        public static Vector2 DefaultSize = new Vector2(200f, 64f);
        public Image Image;
        public float ReadInterval = 0.1f;
        public Text Text;

        private Color32 averageValueColor = new Color32(0xc9, 0xea, 0xfb, 0xff);
        private Color32 backgroundColor = new Color32(0x2c, 0x3e, 80, 0xff);
        private Color32 maxValueColor = new Color32(0xf7, 0x98, 50, 0xff);
        private Color32 minValueColor = new Color32(0xf4, 70, 0x47, 0xff);

        private float avgValue;
        private float minValue;
        private float maxValue;

        private float nextMaxValue;
        private float nextMinValue;
        private float nextValue;

        private bool colorizeTextBuffer;
        private float elapsed;
        private Graphic graphic;
        private float[] history;
        private RectTransform imageRectTransform;

        private int samples;
        private int textHeight = 12;
        private StringBuilder textLineBuffer;
        private string textLineFormat;
        private int textLineLength;
        private float total;
        private AbstractValueProvider valueProvider;
        private const int BlockSize = 5;
        private const string ColorizedStartTextLine = " <color=#F44647FF>▼";
        private const string ColorizedTextLineFormat = "{0}</color> <color=#C9EAFBFF>■{1}</color> <color=#F79832FF>▲{2}</color>";
        private const int Count = 40;
        private const string StartTextLine = " ▼";
        private const string TextLineFormat = "{0} ■{1} ▲{2}";
        private const int TextureHeight = 0x40;
        private const int TextureWidth = 200;

        private void AddToHistory(int index, float value)
        {
            history[index] = value;

            if (value < nextMinValue)
            {
                nextMinValue = value;
            }

            if (value > nextMaxValue)
            {
                nextMaxValue = value;
            }

            total += value;
            samples++;
        }

        private void OnDestroy()
        {
            if (graphic != null)
            {
                graphic.Destroy();
            }
        }

        private void OnEnable()
        {
            if (valueProvider == null)
            {
                valueProvider = base.GetComponent<AbstractValueProvider>();

                if (Application.isPlaying && (valueProvider == null))
                {
                    Debug.LogError("There is no value provider for Profile Panel '" + gameObject.name + "'. Disabling it.");
                    gameObject.SetActive(false);
                    return;
                }

            }
            if (graphic == null)
            {
                graphic = new Graphic(200, 0x40, backgroundColor);

                if (Image == null)
                {
                    Image = base.GetComponent<UnityEngine.UI.Image>();

                    if (Image == null)
                    {
                        Image = gameObject.AddComponent<Image>();
                        Image.rectTransform.sizeDelta = DefaultSize;
                    }
                }

                imageRectTransform = Image.rectTransform;
                Image.sprite = Sprite.Create(graphic.Texture, new Rect(0f, 0f, 200f, 64f), Vector2.zero);

                if (Text != null)
                {
                    Text.color = Color.white;
                    textHeight = Mathf.FloorToInt(Text.rectTransform.rect.height);
                }

                elapsed = ReadInterval;
                history = new float[0x29];
            }
        }

        private void SetValue(int index, float value, float ratio, int m, int textHeight)
        {
            AddToHistory(index, value);
            graphic.DrawRect(200 - (index * 5), textHeight, 5, ((int) (value * ratio)) + textHeight, Color32.LerpUnclamped(minValueColor, maxValueColor, value / maxValue), m + textHeight, averageValueColor);
        }

        private void Update()
        {
            if (valueProvider != null)
            {
                valueProvider.Refresh(ReadInterval);
                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= ReadInterval)
                {
                    elapsed = 0f;

                    int textheight = (imageRectTransform.rect.height > 0f) ? Mathf.CeilToInt(((float) (textHeight * 0x40)) / imageRectTransform.rect.height) : 0;
                    float ratio = (maxValue != 0f) ? (((float) ((0x40 - textheight) - 5)) / maxValue) : 0f;
                    int m = (int) (avgValue * ratio);

                    graphic.Clear(backgroundColor, false);

                    for (int i = 40; i > 0; i--)
                    {
                        SetValue(i, history[i - 1], ratio, m, textheight);
                    }

                    graphic.Apply();

                    nextValue = valueProvider.Value;
                    AddToHistory(0, nextValue);

                    minValue = nextMinValue;
                    maxValue = nextMaxValue;
                    avgValue = total / ((float)samples);

                    UpdateTextLine();

                    nextMaxValue = 0f;
                    nextMinValue = float.MaxValue;

                    total = 0f;
                    samples = 0;
                }
            }
        }

        private void UpdateTextLine()
        {
            if (colorizeTextBuffer != ColorizeText)
            {
                colorizeTextBuffer = ColorizeText;
                textLineBuffer = null;
            }

            if (textLineBuffer == null)
            {
                textLineBuffer = new StringBuilder();
                textLineBuffer.Append(valueProvider.Title);
                textLineBuffer.Append(ColorizeText ? " <color=#F44647FF>▼" : " ▼");
                textLineLength = textLineBuffer.Length;
                textLineFormat = ColorizeText ? "{0}</color> <color=#C9EAFBFF>■{1}</color> <color=#F79832FF>▲{2}</color>" : "{0} ■{1} ▲{2}";
            }

            textLineBuffer.Length = textLineLength;
            Text.text = textLineBuffer.AppendFormat(textLineFormat, minValue.ToString(valueProvider.NumberFormat), avgValue.ToString(valueProvider.NumberFormat), maxValue.ToString(valueProvider.NumberFormat)).ToString();
        }
    }
}

