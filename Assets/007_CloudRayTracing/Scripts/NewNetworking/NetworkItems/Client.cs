
using NetworkScopes;

public class Client : MasterClient
{ 
    public ClientAuthenticator Authenticator { get; private set; }

    public Client()
	{
        // register Scopes to receive Signals from the server
        Authenticator = RegisterScope<ClientAuthenticator>(0);
	}
}
