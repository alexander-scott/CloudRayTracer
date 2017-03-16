using System.Collections.Generic;
using NetworkScopes;
using UnityEngine.Networking;
using System;

public partial class ServerAuthenticator
{
	private RemoteClientAuthenticator _Remote;
	public void Receive_Authenticate(NetworkReader reader)
	{
		String userName = reader.ReadString();
		String passwordHash = reader.ReadString();
		Authenticate(userName, passwordHash);
	}
	
	public void Receive_DebugString(NetworkReader reader)
	{
		String debugString = reader.ReadString();
		DebugString(debugString);
	}
	
	public RemoteClientAuthenticator SendToPeer(Peer targetPeer)
	{
		if (_Remote == null)
			_Remote = new RemoteClientAuthenticator(this);
		TargetPeer = targetPeer;
		return _Remote;
	}
	
	public RemoteClientAuthenticator ReplyToPeer()
	{
		if (_Remote == null)
			_Remote = new RemoteClientAuthenticator(this);
		TargetPeer = SenderPeer;
		return _Remote;
	}
	
	public RemoteClientAuthenticator SendToPeers(IEnumerable<Peer> targetPeerGroup)
	{
		if (_Remote == null)
			_Remote = new RemoteClientAuthenticator(this);
		TargetPeerGroup = targetPeerGroup;
		return _Remote;
	}
	
	public class RemoteClientAuthenticator
	{
		private INetworkSender _netSender;
		public RemoteClientAuthenticator(INetworkSender netSender)
		{
			_netSender = netSender;
		}
		
		public void SendMessageToServer()
		{
			NetworkWriter writer = _netSender.CreateWriter(-1958855171);
			_netSender.PrepareAndSendWriter(writer);
		}
		
	}
}
