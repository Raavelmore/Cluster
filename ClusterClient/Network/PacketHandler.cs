using Cluster.Network;
using ClusterClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using ClusterClient.Sound;
using Cluster;


namespace ClusterClient.Network
{
	internal class PacketHandler
	{
		static Thread HandlerThread = new(HandlePackets);
		static List<Packet> ExecuteQueue = new();
		static Dictionary<PacketType, Action<Packet>> PacketActions = new()
		{
			{PacketType.TCPHandshake, TCPHandshakeReceived},
			{PacketType.TCPPing, TCPPingReceived},
			{PacketType.UDPPing, UDPPingReceived},
			{PacketType.Voice, VoiceReceived},
			{PacketType.UserDataRequest, UserDataRequestReceived },
			{PacketType.SuccessfullLogin,  SuccessfullLoginReceived},
			{PacketType.RoomPreview, RoomPreviewReceived },
			{PacketType.Room, RoomReceived },
			{PacketType.VoiceChatConnectionResponse, VoiceChatConnectionResponseReceived },
			{PacketType.UserJoinedVoiceChat,  UserJoinedVoiceChat},
			{PacketType.UserLeftVoiceChat,  UserLeftVoiceChat},
		};
		public static void Execute(Packet packet)
		{
			ExecuteQueue.Add(packet);
		}
		public static void Start()
		{
			HandlerThread.Start();
		}
		static void HandlePackets()
		{
			while (true)
			{
				if (ExecuteQueue.Count < 1) 
				{
					Thread.Sleep(50);
					continue;
				}
				Packet packet = ExecuteQueue[0];

				ExecuteQueue.RemoveAt(0);
				PacketType type = (PacketType)packet.ReadInt();
				packet.RemoveRead();
				PacketActions[type].Invoke(packet);
			}
		}
		public static void SendTCPPacket(Packet packet)
		{
			Client.Instance.Tcp.SendData(packet);
		}
		public static void SendUDPPacket(Packet packet)
		{
			Client.Instance.Udp.SendData(packet);
		}
		public static void SendUDPHandshake()
		{
			Packet packet = new Packet();
			packet.Write(Client.Instance.Id);
			packet.WriteType(PacketType.UDPHandshake);
			SendUDPPacket(packet);
		}

		public static void SendPing()
		{
			Packet tcpPing = new Packet();
			tcpPing.Write(Client.Instance.Id);
			tcpPing.Write("TCP Ping!");
			tcpPing.WriteType(PacketType.TCPPing);
			tcpPing.WriteLength();
			SendTCPPacket(tcpPing);

			Packet udpPing = new();
			udpPing.Write(Client.Instance.Id);
			udpPing.Write("UDP Ping!");
			udpPing.WriteType(PacketType.UDPPing);
			SendUDPPacket(udpPing);
		}
		public static void SendVoice(byte[] data)
		{
			Packet packet = new();
			packet.Write(Client.Instance.Id);
			packet.Write(data);
			packet.WriteType(PacketType.Voice);
			SendUDPPacket(packet);
		}
		public static void SendRoomRequest(int roomId)
		{
			Trace.WriteLine($"Sending room request, id is {roomId}");
			Packet packet = new Packet();
			packet.Write(Client.Instance.Id);
			packet.Write(roomId);
			packet.WriteType(PacketType.RoomRequest);
			packet.WriteLength();
			SendTCPPacket(packet);
		}

		public static void SendRoomCreationRequest(string roomName)
		{
			Trace.WriteLine($"Sending room creation request, name: {roomName}");
			Packet packet = new Packet();
			packet.Write(Client.Instance.Id);
			packet.Write(roomName);
			packet.WriteType(PacketType.RoomCreationRequest);
			packet.WriteLength();
			SendTCPPacket(packet);
		}
		public static void SendRoomPreviewRequest(int roomId)
		{

			Trace.WriteLine($"Sending room preview request, id: {roomId}");
			Packet packet = new Packet();
			packet.Write(Client.Instance.Id);
			packet.Write(roomId);
			packet.WriteType(PacketType.RoomPreviewRequest);
			packet.WriteLength();
			SendTCPPacket(packet);
		}
		public static void SendVoiceChatConnectionRequest(int chatId)
		{
			Trace.WriteLine($"Sending voice chat connection request, id: {chatId}");
			Packet packet = new Packet();
			packet.Write(Client.Instance.Id);
			packet.Write(chatId);
			packet.WriteType(PacketType.VoiceChatConnectionRequest);
			packet.WriteLength();
			SendTCPPacket(packet);
		}
		public static void DisconnectFromVoiceChat()
		{
			Packet packet = new();
			packet.Write(Client.Instance.Id);
			packet.WriteType(PacketType.DisconnectFromVoiceChat);
			packet.WriteLength();
			SendTCPPacket(packet);
		}


		public static void TCPHandshakeReceived(Packet packet)
		{
			int myId = packet.ReadInt();
			Client.Instance.Id = myId;
			Trace.WriteLine($"TCP handshake received. My id is {myId}. Sending UDP handshake");
			SendUDPHandshake();
		}
		public static void TCPPingReceived(Packet packet)
		{
			string msg = packet.ReadString();
			Trace.WriteLine(msg);
		}
		public static void UDPPingReceived(Packet packet)
		{
			string msg = packet.ReadString();
			Trace.WriteLine(msg);
		}
		public static void VoiceReceived(Packet packet)
		{
			int fromClient = packet.ReadInt();
			packet.RemoveRead();
			byte[] data = packet.GetBytes();
			Audio.PlaySound(data, fromClient);
		}
		public static void UserDataRequestReceived(Packet packet)
		{
			Trace.WriteLine($"User data request received. Sending my data");
			Packet response = new();
			response.Write(Client.Instance.Id);
			response.Write(Client.Instance.Username);
			response.Write(Client.Instance.Password);
			response.WriteType(PacketType.UserDataResponse);
			response.WriteLength();
			SendTCPPacket(response);
		}
		public static void SuccessfullLoginReceived(Packet packet)
		{
			Trace.WriteLine($"Successfull login!!!");
			ConnectionPage.Close();
			MainPage.UpdateUsernameLabel();
		}
		public static void RoomPreviewReceived(Packet packet)
		{
			Trace.WriteLine($"Room preview received");
			Room room = new(packet.ReadString(), packet.ReadInt());
			MainPage.AddRoomPreview(room);
		}
		public static void RoomReceived(Packet packet)
		{
			Trace.WriteLine($"Room received");
			MainPage.ClearRoom();
			int roomId = packet.ReadInt();
			MainPage.AddRoomId(roomId);
			int textChatsCount = packet.ReadInt();
			for (int i = 0; i < textChatsCount; i++)
			{
				TextChat textChat = new(packet.ReadString(), packet.ReadInt());
				MainPage.AddTextChat(textChat);
			}
			int voiceChatsCount = packet.ReadInt();
			for (int i = 0; i < voiceChatsCount; i++)
			{
				VoiceChat voiceChat = new(packet.ReadString(), packet.ReadInt());
				int currentUsersCount = packet.ReadInt();
				for(int j  = 0; j < currentUsersCount; j++)
				{
					voiceChat.Users.Add(packet.ReadString());
				}
				MainPage.AddVoiceChat(voiceChat);
			}
			int usersCount = packet.ReadInt();
			for (int i = 0; i < usersCount; i++)
			{
				MainPage.AddUser(packet.ReadString());
			}
		}
		public static void VoiceChatConnectionResponseReceived(Packet packet)
		{
			int chatId = packet.ReadInt();
			Trace.WriteLine($"Successfully connected to voice chat {chatId}");
			MainPage.VoiceChatId = chatId;
			MainPage.EnableDisconnectButton();
		}
		public static void UserJoinedVoiceChat(Packet packet)
		{
			string username = packet.ReadString();
			int roomId = packet.ReadInt();
			int chatId = packet.ReadInt();
			if (roomId != MainPage.SelectedRoom) return;
			MainPage.UserJoinedVoiceChat(username, chatId);
		}
		public static void UserLeftVoiceChat(Packet packet)
		{
			string username = packet.ReadString();
			int roomId = packet.ReadInt();
			int chatId = packet.ReadInt();
			if (roomId != MainPage.SelectedRoom) return;
			MainPage.UserLeftVoiceChat(username, chatId);
		}
	}
}
