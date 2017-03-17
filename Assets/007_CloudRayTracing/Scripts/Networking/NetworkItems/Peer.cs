
using NetworkScopes;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace BMW.Verification.CloudRayTracing
{
    public class Peer : NetworkPeer
    {
        public string UserName { get; private set; }

        public void SetAuthenticated(string userName)
        {
            UserName = userName;
        }
    }
}