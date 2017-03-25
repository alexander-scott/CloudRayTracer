using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class ClientCanvasNavigation : MonoBehaviour
    {
        public ClientCanvasType[] canvases;
        public ClientCanvasType[] clientCanvasButtons;

        private DataController.ClientCanvasButtonType currentSelectedButtonType = DataController.ClientCanvasButtonType.Information; // Set as default

        // Use this for initialization
        void Start()
        {
            for (int i = 0; i < clientCanvasButtons.Length; i++)
            {
                // SWITCH STATEMENT ISN'T NECESSARY
                switch (clientCanvasButtons[i].type)
                {
                    case DataController.ClientCanvasButtonType.Controls:
                        clientCanvasButtons[i].Button.onClick.AddListener(() => ButtonClicked(DataController.ClientCanvasButtonType.Controls));
                        break;

                    case DataController.ClientCanvasButtonType.Disconnect:
                        clientCanvasButtons[i].Button.onClick.AddListener(() => ButtonClicked(DataController.ClientCanvasButtonType.Disconnect));
                        break;

                    case DataController.ClientCanvasButtonType.Information:
                        clientCanvasButtons[i].Button.onClick.AddListener(() => ButtonClicked(DataController.ClientCanvasButtonType.Information));
                        break;

                    case DataController.ClientCanvasButtonType.Performance:
                        clientCanvasButtons[i].Button.onClick.AddListener(() => ButtonClicked(DataController.ClientCanvasButtonType.Performance));
                        break;

                    case DataController.ClientCanvasButtonType.Viewports:
                        clientCanvasButtons[i].Button.onClick.AddListener(() => ButtonClicked(DataController.ClientCanvasButtonType.Viewports));
                        break;
                }
            }

            SwapCanvases();
        }

        private void ButtonClicked(DataController.ClientCanvasButtonType selectedButton)
        {
            currentSelectedButtonType = selectedButton;
            SwapCanvases();
        }

        private void SwapCanvases()
        {
            for (int i = 0; i < clientCanvasButtons.Length; i++)
            {
                if (clientCanvasButtons[i].type != currentSelectedButtonType)
                {
                    clientCanvasButtons[i].Image.CrossFadeAlpha(0.3f, 0.1f, true);
                }
                else
                {
                    clientCanvasButtons[i].Image.CrossFadeAlpha(1f, 0.1f, true);
                }
            }

            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].type != currentSelectedButtonType)
                {
                    canvases[i].GetComponent<CanvasGroup>().alpha = 0;
                    canvases[i].GetComponent<CanvasGroup>().interactable = false;
                    canvases[i].GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
                else
                {
                    canvases[i].GetComponent<CanvasGroup>().alpha = 1;
                    canvases[i].GetComponent<CanvasGroup>().interactable = true;
                    canvases[i].GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
            }
        }
    }
}
