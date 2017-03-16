using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerController : MonoBehaviour
{
    private static ServerController _instance;

    public static ServerController Instance { get { return _instance; } }

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

    public Button startServer;
    private Server server;

	// Use this for initialization
	void Start ()
    {
        startServer.onClick.AddListener(StartServerClicked);
        server = new Server();

        server.OnPeerConnected += Server_OnPeerConnected;
    }

    public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
    {
        GameObject go = ObjectManager.Instance.GetGameObject(oldKey);
        go.transform.position = position;
        go.transform.eulerAngles = rotation;
        go.transform.localScale = localScale;

        ObjectManager.Instance.UpdateKey(oldKey);
    }

    private void Server_OnPeerConnected(Peer obj)
    {
        Debug.Log("Peer connected!");
    }

    private void StartServerClicked()
    {
        GlobalVariables.isClient = false;
        GlobalVariables.activated = true;
        server.StartServer(7777);
    }
}
