using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasController : MonoBehaviour
    {
        [Header("Canvases")]
        public ClientCanvasType[] canvases;

        [Space(10)]
        [Header("Buttons")]

        public ClientCanvasType[] clientCanvasButtons;

        private DataController.ClientCanvasButton currentSelectedButtonType = DataController.ClientCanvasButton.Information; // Set as default

        // Use this for initialization
        void Start()
        {
            for (int i = 0; i < clientCanvasButtons.Length; i++)
            {
                switch (clientCanvasButtons[i].type)
                {
                    case DataController.ClientCanvasButton.Controls:
                        clientCanvasButtons[i].Button.onClick.AddListener(ControlsButtonClicked);
                        break;

                    case DataController.ClientCanvasButton.Information:
                        clientCanvasButtons[i].Button.onClick.AddListener(InfoButtonClicked);
                        break;

                    case DataController.ClientCanvasButton.Performance:
                        clientCanvasButtons[i].Button.onClick.AddListener(PerformButtonClicked);
                        break;

                    case DataController.ClientCanvasButton.Viewports:
                        clientCanvasButtons[i].Button.onClick.AddListener(ViewportsButtonClicked);
                        break;
                }
            }

            SwapCanvases();
        }

        private void PerformButtonClicked()
        {
            currentSelectedButtonType = DataController.ClientCanvasButton.Performance;
            SwapCanvases();
        }

        private void ViewportsButtonClicked()
        {
            currentSelectedButtonType = DataController.ClientCanvasButton.Viewports;
            SwapCanvases();
        }

        private void ControlsButtonClicked()
        {
            currentSelectedButtonType = DataController.ClientCanvasButton.Controls;
            SwapCanvases();
        }

        private void InfoButtonClicked()
        {
            currentSelectedButtonType = DataController.ClientCanvasButton.Information;
            SwapCanvases();
        }

        private void FadeColourOfButtons(DataController.ClientCanvasButton selectedButton)
        {
            for (int i = 0; i < clientCanvasButtons.Length; i++)
            {
                if (clientCanvasButtons[i].type != selectedButton)
                {
                    clientCanvasButtons[i].Image.color = new Color(1, 1, 1, 0.5f);
                }
                else
                {
                    clientCanvasButtons[i].Image.color = new Color(1, 1, 1, 1f);
                }
            }
        }

        private void SwapCanvases()
        {
            for (int i = 0; i < clientCanvasButtons.Length; i++)
            {
                if (clientCanvasButtons[i].type != currentSelectedButtonType)
                {
                    clientCanvasButtons[i].Image.color = new Color(1, 1, 1, 0.5f);
                }
                else
                {
                    clientCanvasButtons[i].Image.color = new Color(1, 1, 1, 1f);
                }
            }

            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].type != currentSelectedButtonType)
                {
                    canvases[i].gameObject.SetActive(false);
                }
                else
                {
                    canvases[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
