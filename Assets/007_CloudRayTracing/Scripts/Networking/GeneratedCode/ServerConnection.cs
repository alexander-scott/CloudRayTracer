using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
	public partial class ServerConnection
	{
		private RemoteClientConnection _Remote;
		public void Receive_RecieveObjectPosition(NetworkReader reader)
		{
			Int32 objectID = reader.ReadInt32();
			Vector3 position = reader.ReadVector3();
			Vector3 rotation = reader.ReadVector3();
			Vector3 localScale = reader.ReadVector3();
			RecieveObjectPosition(objectID, position, rotation, localScale);
		}
		
		public void Receive_RecieveObjectState(NetworkReader reader)
		{
			Int32 objectID = reader.ReadInt32();
			Boolean active = reader.ReadBoolean();
			RecieveObjectState(objectID, active);
		}
		
		public void Receive_RecieveObjectStateAndPosition(NetworkReader reader)
		{
			Int32 objectID = reader.ReadInt32();
			Boolean active = reader.ReadBoolean();
			Vector3 position = reader.ReadVector3();
			Vector3 rotation = reader.ReadVector3();
			Vector3 localScale = reader.ReadVector3();
			RecieveObjectStateAndPosition(objectID, active, position, rotation, localScale);
		}
		
		public void Receive_RecievePacket(NetworkReader reader)
		{
			Int32 packetNum = reader.ReadInt32();
			String contents = reader.ReadString();
			RecievePacket(packetNum, contents);
		}
		
		public void Receive_RecieveNewCarSpawn(NetworkReader reader)
		{
			Int32 objectID = reader.ReadInt32();
			Boolean active = reader.ReadBoolean();
			RecieveNewCarSpawn(objectID, active);
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
			
			public void RecievePacket(Int32 packetNum, String contents)
			{
				NetworkWriter writer = _netSender.CreateWriter(55045539);
				writer.Write(packetNum);
				writer.Write(contents);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void RecieveServerPerformanceDictionary(Int32 performanceType, Single performanceVal)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1851791576);
				writer.Write(performanceType);
				writer.Write(performanceVal);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void ClientPrepareToRecieveTransmission(Int32 transmissionId, Int32 expectedSize)
			{
				NetworkWriter writer = _netSender.CreateWriter(-2015836376);
				writer.Write(transmissionId);
				writer.Write(expectedSize);
				_netSender.PrepareAndSendWriter(writer);
			}
			
			public void ClientRecieveTransmission(Int32 transmissionId, Byte[] recBuffer)
			{
				NetworkWriter writer = _netSender.CreateWriter(-1696830220);
				writer.Write(transmissionId);
				writer.Write(recBuffer.Length);
				for (int _arrCounter = 0; _arrCounter < recBuffer.Length; _arrCounter++)
				writer.Write(recBuffer[_arrCounter]);
				_netSender.PrepareAndSendWriter(writer);
			}
			
		}
	}
}
