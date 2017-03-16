using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{
    public Button connectToServer;
    public Button sendMessageToServer;

    private Client client;

	// Use this for initialization
	void Start ()
    {
        connectToServer.onClick.AddListener(ConnectToServer);
        sendMessageToServer.onClick.AddListener(SendMessageToServer);

        client = new Client();
        client.PersistConnection = true;

        client.OnConnected += Client_OnConnected;
        client.OnConnectFailed += Client_OnConnectFailed;
        client.OnDisconnected += Client_OnDisconnected;
    }

    private void SendMessageToServer()
    {
        client.Authenticator.SendMessageToServer();
    }

    private void Client_OnDisconnected(byte disconnectMsg)
    {
        Debug.Log("Disconneted");
    }

    private void Client_OnConnectFailed()
    {
        Debug.Log("Failed to connect");
    }

    private void Client_OnConnected()
    {
        Debug.Log("Connected");
    }

    private void ConnectToServer()
    {
        client.Connect("127.0.0.1", 7777);
    }
}
