using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class MenuController : MonoBehaviour
    {
        #region Singleton

        private static MenuController _instance;

        public static MenuController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        #endregion

        [Header("Client objects")]
        public CanvasGroup clientCanvas;
        public Button connectToServerButton;
        public Text ipAddressLabel;
        public Button startRaytracerButton;

        [Space(10)]
        [Header("Server objects")]
        public CanvasGroup serverCanvas;
        public Button startServer;
        public InputField ipAddress;

        [Space(10)]
        [Header("Host objects")]
        public Button host;

        [Space(10)]
        [Header("Config menu objects")]
        public GameObject menuCanvas;
        public Text subTitle;

        // Use this for initialization
        void Start()
        {
            connectToServerButton.onClick.AddListener(ConnectToServer);
            startServer.onClick.AddListener(StartServerClicked);
            host.onClick.AddListener(HostClicked);

            ipAddress.text = DataController.Instance.ipAddress;
            ipAddressLabel.text = DataController.Instance.LocalIPAddress();
        }

        public void IPAddressChanged(string ipaddress)
        {
            DataController.Instance.ipAddress = ipaddress;

            PlayerPrefs.SetString("IPAddress", ipaddress); // Save the new ip address locally on the device
            PlayerPrefs.Save();
        }

        public void UpdateSubTitleText(string text)
        {
            subTitle.text = text;
        }

        public void OnClientConnected()
        {
            subTitle.text = "You are the CLIENT";
            Destroy(ServerController.Instance); Destroy(HostController.Instance);

            StartCoroutine(FadeCanvasGroupIn(MenuController.Instance.clientCanvas, 0.5f));

            DataController.Instance.applicationType = DataController.ApplicationType.Client;
        }

        private void StartServerClicked()
        {
            subTitle.text = "Starting server...";
            Destroy(ClientController.Instance); Destroy(HostController.Instance);

            StartCoroutine(FadeCanvasGroupIn(serverCanvas, 0.5f));

            ServerController.Instance.StartServer();

            DataController.Instance.applicationType = DataController.ApplicationType.Server;
        }

        private void ConnectToServer()
        {
            subTitle.text = "Connecting to server...";

            ClientController.Instance.ConnectToServer();
        }

        private void HostClicked()
        {
            Destroy(ServerController.Instance); Destroy(ClientController.Instance);

            StartCoroutine(FadeCanvasGroupIn(clientCanvas, 0.5f));

            HostController.Instance.HostSelected();

            DataController.Instance.applicationType = DataController.ApplicationType.Host;
        }

        private IEnumerator FadeCanvasGroupIn(CanvasGroup canvasGroup, float duration)
        {
            float smoothness = 0.02f;
            float progress = 0; // This float will serve as the 3rd parameter of the lerp function.
            float increment = smoothness / duration; // The amount of change to apply.

            if (!canvasGroup.gameObject.activeInHierarchy)
            {
                canvasGroup.gameObject.SetActive(true);
                canvasGroup.alpha = 0f;
            }

            while (progress < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

                progress += increment;
                yield return new WaitForSeconds(smoothness);
            }
        }
    }
}