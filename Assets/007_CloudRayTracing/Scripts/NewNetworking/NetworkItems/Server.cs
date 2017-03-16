
using NetworkScopes;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Server : MasterServer<Peer>
{
    public ServerAuthenticator Authenticator { get; private set; }

    public Server()
	{
        // register a new authentication scope and set it as the default
        Authenticator = RegisterScope<ServerAuthenticator>(0, true);
    }
}