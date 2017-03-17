
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

        #region Send to server

        [Signal]
        public void SendPacket(int packetNum, string contents)
        {
            SendToServer.RecievePacket(packetNum, contents);
        }

        [Signal]
        public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            SendToServer.RecieveObjectPosition(oldKey, position, rotation, localScale);
        }

        #endregion

        #region Recieve from server

        [Signal]
        public void RecievePacket(int packetNum, string contents)
        {
            ClientController.Instance.PacketRecieved((GlobalVariables.PacketType)packetNum, contents);
        }

        [Signal]
        public void RecieveSeriliasedMesh(byte[] mesh)
        {
            ClientController.Instance.RenderMesh(mesh);
        }

        #endregion
    }
}
