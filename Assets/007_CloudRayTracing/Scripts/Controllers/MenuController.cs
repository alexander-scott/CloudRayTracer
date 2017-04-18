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

        [Header("Top UI")]
        public Text topTitle;
        public GameObject configCanvas;

        [Space(10)]
        [Header("Client objects")]
        public CanvasGroup clientCanvas;
        public Button connectToServerButton;
        public Text ipAddressLabel;
        public Text pubIpAddressLabel;

        [Space(10)]
        [Header("Server objects")]
        public CanvasGroup serverCanvas;
        public Button startServer;
        public InputField ipAddress;

        [Space(10)]
        [Header("Host objects")]
        public Button host;

        // Use this for initialization
        void Start()
        {
            connectToServerButton.onClick.AddListener(ConnectToServer);
            startServer.onClick.AddListener(StartServerClicked);
            host.onClick.AddListener(HostClicked);

            ipAddress.text = DataController.Instance.ipAddress;
            ipAddressLabel.text = "Loc: " + DataController.Instance.GetLocalIP();
            Timing.RunCoroutine(GetIPAddress(), "GetPublicIPAddress");

            // If we are running in headless mode go straight to start server
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Destroy(ClientController.Instance); Destroy(HostController.Instance);

                ServerController.Instance.StartServer();

                DataController.Instance.applicationState = DataController.ApplicationState.Server;
            }

            if (clientCanvas.gameObject.activeInHierarchy)
            {
                clientCanvas.gameObject.SetActive(false);
            }
        }

        public void IPAddressChanged(string ipaddress)
        {
            DataController.Instance.ipAddress = ipaddress;

            PlayerPrefs.SetString("IPAddress", ipaddress); // Save the new ip address locally on the device
            PlayerPrefs.Save();
        }

        public void UpdateSubTitleText(string text)
        {
            topTitle.text = text;
        }

        public void OnClientConnected()
        {
            topTitle.text = "You are the CLIENT";
            Destroy(ServerController.Instance); Destroy(HostController.Instance);

            clientCanvas.gameObject.SetActive(true); configCanvas.gameObject.SetActive(false);
        }

        private void StartServerClicked()
        {
            topTitle.text = "Starting server...";
            Destroy(ClientController.Instance); Destroy(HostController.Instance);

            ServerController.Instance.StartServer();

            clientCanvas.gameObject.SetActive(false); configCanvas.gameObject.SetActive(false);

            DataController.Instance.applicationState = DataController.ApplicationState.Server;
        }

        private void ConnectToServer()
        {
            topTitle.text = "Connecting to server...";

            ClientController.Instance.ConnectToServer();
        }

        private void HostClicked()
        {
            Destroy(ServerController.Instance); Destroy(ClientController.Instance);

            clientCanvas.gameObject.SetActive(true); configCanvas.gameObject.SetActive(false);

            UpdateSubTitleText("You are the HOST");

            TrafficController.Instance.SpawnCarsHost();

            DataController.Instance.applicationState = DataController.ApplicationState.Host;
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

        private IEnumerator<float> GetIPAddress()
        {
            Request someRequest = new Request("get", "http://checkip.dyndns.org");
            someRequest.Send();

            while (!someRequest.isDone && Application.isPlaying)
            {
                yield return 0f;
            }

            if (someRequest.response == null || someRequest.response.Text == null)
            {
                string savedPubIPAddress = PlayerPrefs.GetString("PublicIPAddress");
                if (string.IsNullOrEmpty(savedPubIPAddress))
                {
                    pubIpAddressLabel.text = "Failed to get Pub IP";
                }
                else
                {
                    pubIpAddressLabel.text = "Pub: " + savedPubIPAddress;
                }
            }
            else
            {
                string response = someRequest.response.Text;
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];

                pubIpAddressLabel.text = "Pub: " + a4;
                PlayerPrefs.SetString("PublicIPAddress", a4);
                PlayerPrefs.Save();
            }
        }
    }
}