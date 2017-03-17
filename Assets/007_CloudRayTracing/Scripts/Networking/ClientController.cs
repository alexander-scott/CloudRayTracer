using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{
    #region Singleton

    private static ClientController _instance;

    public static ClientController Instance { get { return _instance; } }

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

    private Client client;

    // Use this for initialization
    void Start ()
    {
        client = new Client();
        client.PersistConnection = true;

        client.OnConnected += Client_OnConnected;
        client.OnDisconnected += Client_OnDisconnected;
        client.OnConnectFailed += Client_OnConnectFailed;
    }

    public void UpdateObjectPositionOnServer(Vector3 oldkey, Vector3 position, Vector3 rotation, Vector3 localScale)
    {
        client.Connection.UpdateObjectPosition(oldkey, position, rotation, localScale);
    }

    public void ConnectToServer()
    {
        GlobalVariables.isClient = true;
        GlobalVariables.activated = true;
        client.Connect(GlobalVariables.ipAddress, 7777);
    }

    private void Client_OnConnectFailed()
    {
        UIManager.Instance.UpdateSubTitleText("Failed to connect to the server");
    }

    private void Client_OnDisconnected(byte disconnectMsg)
    {
        UIManager.Instance.UpdateSubTitleText("Disconnected from the server");
    }

    private void Client_OnConnected()
    {
        Debug.Log("Connected");
        UIManager.Instance.UpdateSubTitleText("You are the CLIENT");
    }
}
