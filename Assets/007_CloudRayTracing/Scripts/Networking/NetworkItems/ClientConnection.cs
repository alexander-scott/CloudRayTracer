
using NetworkScopes;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    [Scope(typeof(ServerConnection))]
    public partial class ClientConnection : ClientScope
    {
        protected override void OnEnterScope()
        {
            //SendToServer.Authenticate("sour", "testpw");
        }

        [Signal]
        public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            SendToServer.UpdateObjectPosition(oldKey, position, rotation, localScale);
        }
    }
}
