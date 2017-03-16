using System.Collections.Generic;
using NetworkScopes;
using UnityEngine.Networking;
using System;
using UnityEngine;

public partial class ClientAuthenticator
{
	private RemoteServerAuthenticator _Remote;
	public RemoteServerAuthenticator SendToServer
	{
		get
		{
			if (_Remote == null)
				_Remote = new RemoteServerAuthenticator(this);
			return _Remote;
		}
	}
	public void Receive_UpdateObjectPosition(NetworkReader reader)
	{
		Vector3 oldKey = reader.ReadVector3();
		Vector3 position = reader.ReadVector3();
		Vector3 rotation = reader.ReadVector3();
		Vector3 localScale = reader.ReadVector3();
		UpdateObjectPosition(oldKey, position, rotation, localScale);
	}
	
	public class RemoteServerAuthenticator
	{
		private INetworkSender _netSender;
		public RemoteServerAuthenticator(INetworkSender netSender)
		{
			_netSender = netSender;
		}
		
		public void Authenticate(String userName, String passwordHash)
		{
			NetworkWriter writer = _netSender.CreateWriter(1885436661);
			writer.Write(userName);
			writer.Write(passwordHash);
			_netSender.PrepareAndSendWriter(writer);
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
