using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerController : MonoBehaviour
{
    public Button startServer;

    private Server server;

	// Use this for initialization
	void Start ()
    {
        startServer.onClick.AddListener(StartServerClicked);
        server = new Server();

        server.OnPeerConnected += Server_OnPeerConnected;
    }

    private void Server_OnPeerConnected(Peer obj)
    {
        Debug.Log("Peer connected!");
    }

    private void StartServerClicked()
    {
        server.StartServer(7777);
    }
}
