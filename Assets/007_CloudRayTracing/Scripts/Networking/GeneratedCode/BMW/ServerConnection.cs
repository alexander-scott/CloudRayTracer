using System.Collections.Generic;
using NetworkScopes;
using UnityEngine.Networking;
using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
	public partial class ServerConnection
	{
		private RemoteClientConnection _Remote;
		public void Receive_Authenticate(NetworkReader reader)
		{
			String userName = reader.ReadString();
			String passwordHash = reader.ReadString();
			Authenticate(userName, passwordHash);
		}
		
		public void Receive_SendPacket(NetworkReader reader)
		{
			Int32 packetNum = reader.ReadInt32();
			String contents = reader.ReadString();
			SendPacket(packetNum, contents);
		}
		
		public void Receive_SendSeriliasedMesh(NetworkReader reader)
		{
			Int32 mesh_count = reader.ReadInt32();
			System.Byte[] mesh = new System.Byte[mesh_count];
			for (int mesh_index = 0; mesh_index < mesh_count; mesh_index++)
			mesh[mesh_index] = reader.ReadByte();
			SendSeriliasedMesh(mesh);
		}
		
		public void Receive_RecieveObjectPosition(NetworkReader reader)
		{
			Vector3 oldKey = reader.ReadVector3();
			Vector3 position = reader.ReadVector3();
			Vector3 rotation = reader.ReadVector3();
			Vector3 localScale = reader.ReadVector3();
			RecieveObjectPosition(oldKey, position, rotation, localScale);
		}
		
		public void Receive_RecievePacket(NetworkReader reader)
		{
			Int32 packetNum = reader.ReadInt32();
			String contents = reader.ReadString();
			RecievePacket(packetNum, contents);
		}
		
		public RemoteClientConnection SendToPeer(BMW.Verification.CloudRayTracing.Peer targetPeer)
		{
			if (_Remote == null)
				_Remote = new RemoteClientConnection(this);
			TargetPeer = targetPeer;
			return _Remote;
		}
		
		public RemoteClientConnection ReplyToPeer()
		{
			if (_Remote == null)
				_Remote = new RemoteClientConnection(this);
			TargetPeer = SenderPeer;
			return _Remote;
		}
		
		public RemoteClientConnection SendToPeers(IEnumerable<BMW.Verification.CloudRayTracing.Peer> targetPeerGroup)
		{
			if (_Remote == null)
				_Remote = new RemoteClientConnection(this);
			TargetPeerGroup = targetPeerGroup;
			return _Remote;
		}
		
		public class RemoteClientConnection
		{
			private INetworkSender _netSender;
			public RemoteClientConnection(INetworkSender netSender)
			{
				_netSender = netSender;
			}
			
			public void SendPacket(Int32 packetNum, String contents)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1920393648);
				writer.Write(packetNum);
				writer.Write(contents);
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
			
			public void RecievePacket(Int32 packetNum, String contents)
			{
				NetworkWriter writer = _netSender.CreateWriter(55045539);
				writer.Write(packetNum);
				writer.Write(contents);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecieveSeriliasedMesh(Byte[] mesh)
			{
				NetworkWriter writer = _netSender.CreateWriter(1488026943);
				writer.Write(mesh.Length);
				for (int _arrCounter = 0; _arrCounter < mesh.Length; _arrCounter++)
				writer.Write(mesh[_arrCounter]);
				_netSender.PrepareAndSendWriter(writer);
			}
			
		}
	}
}
