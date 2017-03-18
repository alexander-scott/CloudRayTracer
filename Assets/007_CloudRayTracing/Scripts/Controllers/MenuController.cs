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

        public Button connectToServer;
        public Button startServer;
        public Button host;
        public InputField ipAddress;
        public Text ourIpAddress;

        [Space(10)]

        public GameObject menuCanvas;
        public Text subTitle;

        // Use this for initialization
        void Start()
        {
            connectToServer.onClick.AddListener(ConnectToServer);
            startServer.onClick.AddListener(StartServerClicked);
            host.onClick.AddListener(HostClicked);

            ipAddress.text = GlobalVariables.ipAddress;
            ourIpAddress.text = GlobalVariables.LocalIPAddress();
        }

        public void IPAddressChanged(string ipaddress)
        {
            GlobalVariables.ipAddress = ipaddress;

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

            GlobalVariables.applicationType = GlobalVariables.ApplicationType.Server;
        }

        private void ConnectToServer()
        {
            subTitle.text = "Connecting to server...";
            menuCanvas.SetActive(false);
            Destroy(ServerController.Instance); Destroy(HostController.Instance);

            ClientController.Instance.ConnectToServer();

            GlobalVariables.applicationType = GlobalVariables.ApplicationType.Client;
        }

        private void HostClicked()
        {
            menuCanvas.SetActive(false);
            Destroy(ServerController.Instance); Destroy(ClientController.Instance);

            HostController.Instance.HostSelected();

            GlobalVariables.applicationType = GlobalVariables.ApplicationType.Host;
        }
    }
}