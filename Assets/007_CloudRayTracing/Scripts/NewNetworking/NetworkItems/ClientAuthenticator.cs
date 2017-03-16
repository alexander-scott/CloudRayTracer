
using NetworkScopes;
using UnityEngine;

[Scope(typeof(ServerAuthenticator))]
public partial class ClientAuthenticator : ClientScope
{
	protected override void OnEnterScope ()
	{
		//SendToServer.Authenticate("sour", "testpw");
	}

    [Signal]
    public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
    {
        SendToServer.UpdateObjectPosition(oldKey, position, rotation, localScale); 
    }
}
