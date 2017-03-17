
namespace NetworkScopes
{
	using UnityEngine;
	using UnityEngine.Networking;
	using System.Collections.Generic;

//	#define SHOW_SEND_ERRORS

	public static class ScopeUtils
	{
        public const int UNRELIABLE_SEQUENCED_CHANNEL = (int)QosType.Unreliable;

        public static void SendNetworkWriter(NetworkWriter writer, NetworkConnection connection)
		{
			if (!connection.isConnected)
				return;
			
			byte error;
			NetworkTransport.Send(connection.hostId, connection.connectionId, UNRELIABLE_SEQUENCED_CHANNEL, writer.ToArray(), writer.Position, out error);

			NetworkError nerror = (NetworkError)error;
			if (nerror != NetworkError.Ok)
				Debug.LogError("Network error: " + nerror);
		}

		public static void SendNetworkWriter<TPeer>(NetworkWriter writer, IEnumerable<TPeer> peers) where TPeer : NetworkPeer
		{
			byte error;

			byte[] bufferData = writer.AsArray();
			short bufferSize = writer.Position;

			foreach (TPeer peer in peers)
			{
				if (!peer.isConnected)
					return;
				
				NetworkTransport.Send(peer.hostId, peer.connectionId, UNRELIABLE_SEQUENCED_CHANNEL, bufferData, bufferSize, out error);

				NetworkError nerror = (NetworkError)error;
				if (nerror != NetworkError.Ok)
					Debug.LogError("Network error: " + nerror);
			}
		}


		public static void SendScopeSwitchedMessage<TPeer>(BaseServerScope<TPeer> prevScope, BaseServerScope<TPeer> newScope, NetworkConnection connection) where TPeer : NetworkPeer
		{
			NetworkWriter writer = new NetworkWriter();

			writer.StartMessage(ScopeMsgType.SwitchScope);

			// 1. msgType: Send prev scope channel
			writer.Write(prevScope.msgType);

			// 2. msgType: Send new scope channel
			writer.Write(newScope.msgType);

			// 2. scopeIdentifier: The value which identifier the counterpart (new) client scope
			writer.Write(newScope.scopeIdentifier);

			writer.FinishMessage();

			SendNetworkWriter(writer, connection);
		}

		public static void SendScopeEnteredMessage<TPeer>(BaseServerScope<TPeer> scope, NetworkConnection connection) where TPeer : NetworkPeer
		{
			NetworkWriter writer = new NetworkWriter();

			writer.StartMessage(ScopeMsgType.EnterScope);

			// 1. scopeIdentifier: The value which identifier the counterpart client class
			writer.Write(scope.msgType);

			// 2. msgType: Determines which channel to communicate on
			writer.Write(scope.scopeIdentifier);

			writer.FinishMessage();

			SendNetworkWriter(writer, connection);
		}

		public static void SendScopeExitedMessage<TPeer>(BaseServerScope<TPeer> scope, NetworkConnection connection) where TPeer : NetworkPeer
		{
			NetworkWriter writer = new NetworkWriter();

			writer.StartMessage(ScopeMsgType.ExitScope);

			// 1. msgType: Determines which channel to communicate on
			writer.Write(scope.msgType);

			writer.FinishMessage();

			SendNetworkWriter(writer, connection);
		}
	}
}