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

        public Button connectToServerButton;
        public Text ipAddressLabel;
        public GameObject clientCanvas;
        public Button startRaytracerButton;

        [Space(10)]

        public Button startServer;
        public InputField ipAddress;

        [Space(10)]

        public Button host;

        [Space(10)]

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

        private void StartServerClicked()
        {
            subTitle.text = "Starting server...";
            menuCanvas.SetActive(false);
            Destroy(ClientController.Instance); Destroy(HostController.Instance);

            ServerController.Instance.StartServer();

            DataController.Instance.applicationType = DataController.ApplicationType.Server;
        }

        private void ConnectToServer()
        {
            subTitle.text = "Connecting to server...";

            ClientController.Instance.ConnectToServer();

            DataController.Instance.applicationType = DataController.ApplicationType.Client;
        }

        private void HostClicked()
        {
            menuCanvas.SetActive(false);
            Destroy(ServerController.Instance); Destroy(ClientController.Instance);

            HostController.Instance.HostSelected();

            DataController.Instance.applicationType = DataController.ApplicationType.Host;
        }
    }
}