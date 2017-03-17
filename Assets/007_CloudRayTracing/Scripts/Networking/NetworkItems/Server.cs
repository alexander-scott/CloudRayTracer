
using NetworkScopes;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Server : MasterServer<Peer>
{
    public ServerConnection Connection { get; private set; }

    public Server()
	{
        // register a new authentication scope and set it as the default
        Connection = RegisterScope<ServerConnection>(0, true);
    }
}