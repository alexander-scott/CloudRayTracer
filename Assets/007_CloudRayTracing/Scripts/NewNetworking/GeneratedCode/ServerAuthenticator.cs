using System.Collections.Generic;
using NetworkScopes;
using UnityEngine.Networking;
using UnityEngine;
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
	
	public void Receive_UpdateObjectPosition(NetworkReader reader)
	{
		Vector3 oldKey = reader.ReadVector3();
		Vector3 position = reader.ReadVector3();
		Vector3 rotation = reader.ReadVector3();
		Vector3 localScale = reader.ReadVector3();
		UpdateObjectPosition(oldKey, position, rotation, localScale);
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
		
		public void UpdateObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
		{
			NetworkWriter writer = _netSender.CreateWriter(-1383950639);
			writer.Write(oldKey);
			writer.Write(position);
			writer.Write(rotation);
			writer.Write(localScale);
			_netSender.PrepareAndSendWriter(writer);
		}
		
	}
}
