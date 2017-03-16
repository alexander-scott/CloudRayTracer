using System.Collections.Generic;
using NetworkScopes;
using UnityEngine.Networking;
using System;

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
	public void Receive_SendMessageToServer(NetworkReader reader)
	{
		SendMessageToServer();
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
		
		public void DebugString(String debugString)
		{
			NetworkWriter writer = _netSender.CreateWriter(112279588);
			writer.Write(debugString);
			_netSender.PrepareAndSendWriter(writer);
		}
		
	}
}
