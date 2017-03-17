
using NetworkScopes;

public class Client : MasterClient
{ 
    public ClientConnection Connection { get; private set; }

    public Client()
	{
        // register Scopes to receive Signals from the server
        Connection = RegisterScope<ClientConnection>(0);
	}
}
