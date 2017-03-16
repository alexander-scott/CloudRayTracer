
using NetworkScopes;

[Scope(typeof(ServerAuthenticator))]
public partial class ClientAuthenticator : ClientScope
{
	protected override void OnEnterScope ()
	{
		SendToServer.Authenticate("sour", "testpw");
	}

    [Signal]
    public void SendMessageToServer()
    {
        SendToServer.DebugString("TEST12323544");
    }
}
