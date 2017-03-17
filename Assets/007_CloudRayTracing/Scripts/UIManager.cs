using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BMW.Verification.CloudRayTracing
{
    public class UIManager : MonoBehaviour
    {
        #region Singleton

        private static UIManager _instance;

        public static UIManager Instance { get { return _instance; } }

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
        public InputField ipAddress;

        [Space(10)]

        public GameObject menuCanvas;
        public Text subTitle;

        // Use this for initialization
        void Start()
        {
            connectToServer.onClick.AddListener(ConnectToServer);
            startServer.onClick.AddListener(StartServerClicked);

            ipAddress.text = GlobalVariables.ipAddress;
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
            ServerController.Instance.StartServer();
        }

        private void ConnectToServer()
        {
            subTitle.text = "Connecting to server...";
            menuCanvas.SetActive(false);
            ClientController.Instance.ConnectToServer();
        }
    }
}