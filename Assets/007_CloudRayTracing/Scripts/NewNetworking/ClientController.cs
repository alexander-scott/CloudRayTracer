using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{
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

    public Button connectToServer;
    public Button moveButton;
    public Transform cube;
    private Client client;

    // Use this for initialization
    void Start ()
    {
        connectToServer.onClick.AddListener(ConnectToServer);
        moveButton.onClick.AddListener(MoveButton);

        client = new Client();
        client.PersistConnection = true;

        client.OnConnected += Client_OnConnected;
        client.OnDisconnected += Client_OnDisconnected;
    }

    private void MoveButton()
    {
        cube.position = new Vector3(5f, 5f, 5f);
    }

    public void UpdateObjectPosition(Vector3 oldkey, Vector3 position, Vector3 rotation, Vector3 localScale)
    {
        client.Authenticator.UpdateObjectPosition(oldkey, position, rotation, localScale);
    }

    private void Client_OnDisconnected(byte disconnectMsg)
    {
        Debug.Log("Disconneted");
    }

    private void Client_OnConnected()
    {
        Debug.Log("Connected");
    }

    private void ConnectToServer()
    {
        GlobalVariables.isClient = true;
        GlobalVariables.activated = true;
        client.Connect("127.0.0.1", 7777);
    }
}
