using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasType : MonoBehaviour
    {
        public DataController.ClientCanvasButtonType type;

        public Button Button
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }

                return _button;
            }
        }

        public Image Image
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }

                return _image;
            }
        }

        private Image _image;

        private Button _button;
    }
}
