
using System;
using NetworkScopes;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    [Scope(typeof(ClientConnection))]
    public partial class ServerConnection : ServerScope<Peer>
    {
        [Signal]
        public void Authenticate(string userName, string passwordHash)
        {
            // process the user's credentials - for now we're just checking for null/empty string to keep it simple for this example
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passwordHash))
            {
                // user authenticated successfully - assign player name within this peer's ExamplePeer instance
                SenderPeer.SetAuthenticated(userName);

                // ..then send peer to the Lobby scope
                //HandoverPeer(SenderPeer, ExampleServer.Lobby);
            }
            // if they couldn't be authenticated, we might as well disconnect them
            else
            {
                // TODO: we might want to send a disconnection reason rather than just closing the connection
                SenderPeer.Disconnect();
            }
        }

        #region Send to client

        [Signal]
        public void SendPacket(int packetNum, string contents)
        {
            SendToPeer(SenderPeer).RecievePacket(packetNum, contents);
        }

        [Signal]
        public void SendSeriliasedMesh(byte[] mesh)
        {
            SendToPeer(SenderPeer).RecieveSeriliasedMesh(mesh);
            
        }

        #endregion

        #region Recieve from clients

        [Signal]
        public void RecieveObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            ServerController.Instance.UpdateObjectPosition(oldKey, position, rotation, localScale);
        }

        [Signal]
        public void RecievePacket(int packetNum, string contents)
        {
            ServerController.Instance.PacketRecieved((GlobalVariables.PacketType)packetNum, contents);
        }

        #endregion
    }
}