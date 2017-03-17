using System.Collections.Generic;
using NetworkScopes;
using UnityEngine.Networking;
using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
	public partial class ClientConnection
	{
		private RemoteServerConnection _Remote;
		public RemoteServerConnection SendToServer
		{
			get
			{
				if (_Remote == null)
					_Remote = new RemoteServerConnection(this);
				return _Remote;
			}
		}
		public void Receive_SendPacket(NetworkReader reader)
		{
			Int32 packetNum = reader.ReadInt32();
			String contents = reader.ReadString();
			SendPacket(packetNum, contents);
		}
		
		public void Receive_UpdateObjectPosition(NetworkReader reader)
		{
			Vector3 oldKey = reader.ReadVector3();
			Vector3 position = reader.ReadVector3();
			Vector3 rotation = reader.ReadVector3();
			Vector3 localScale = reader.ReadVector3();
			UpdateObjectPosition(oldKey, position, rotation, localScale);
		}
		
		public void Receive_RecievePacket(NetworkReader reader)
		{
			Int32 packetNum = reader.ReadInt32();
			String contents = reader.ReadString();
			RecievePacket(packetNum, contents);
		}
		
		public void Receive_RecieveSeriliasedMesh(NetworkReader reader)
		{
			Int32 mesh_count = reader.ReadInt32();
			System.Byte[] mesh = new System.Byte[mesh_count];
			for (int mesh_index = 0; mesh_index < mesh_count; mesh_index++)
			mesh[mesh_index] = reader.ReadByte();
			RecieveSeriliasedMesh(mesh);
		}
		
		public class RemoteServerConnection
		{
			private INetworkSender _netSender;
			public RemoteServerConnection(INetworkSender netSender)
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
			
			public void SendPacket(Int32 packetNum, String contents)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1920393648);
				writer.Write(packetNum);
				writer.Write(contents);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void SendSeriliasedMesh(Byte[] mesh)
			{
				NetworkWriter writer = _netSender.CreateWriter(419541228);
				writer.Write(mesh.Length);
				for (int _arrCounter = 0; _arrCounter < mesh.Length; _arrCounter++)
				writer.Write(mesh[_arrCounter]);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecieveObjectPosition(Vector3 oldKey, Vector3 position, Vector3 rotation, Vector3 localScale)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1684256861);
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
			
		}
	}
}
