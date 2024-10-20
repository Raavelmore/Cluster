using Cluster;
using Cluster.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClusterServer
{
	internal class PacketHandler
	{
		static Thread HandlerThread;
		static List<Packet> ExecuteQueue = new();
		static Dictionary<PacketType, Action<Packet>> PacketActions = new()
		{
			{PacketType.UDPHandshake, UDPHandshakeReceived},
			{PacketType.TCPPing, TCPPingReceived},
			{PacketType.UDPPing, UDPPingReceived},
			{PacketType.Voice, VoiceReceived},
			{PacketType.UserDataResponse, UserDataResponseReceived },
			{PacketType.RoomCreationRequest, RoomCreationRequestReceived },
			{PacketType.RoomRequest, RoomRequestReceived },
			{PacketType.RoomPreviewRequest, RoomPreviewRequestReceived },
			{PacketType.VoiceChatConnectionRequest, VoiceChatConnectionReceived },
			{PacketType.DisconnectFromVoiceChat, DisconnectFromVoiceChatReceived },
		};
		public static void Execute(Packet packet)
		{
			ExecuteQueue.Add(packet);
		}
		public static void Start()
		{
			HandlerThread = new(HandlePackets);
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
		public static void SendTCPPacket(Packet packet, int toClient)
		{
			Server.Clients[toClient].Tcp.SendData(packet);
		}
		public static void SendTCPPacket(Packet packet)
		{
			foreach (Client? c in Server.Clients.Values)
			{
				c.Tcp.SendData(packet);
			}
		}
		public static void SendUDPPacket(Packet packet, int toClient, bool except = false)
		{
			if(!except) Server.Clients[toClient].Udp.SendData(packet);
			else
			{
				for(int i = 1; i < Server.MaxClients + 1; i++)
				{
					if (i == toClient) continue;
					if (Server.Clients[i] == null) continue;
					SendUDPPacket(packet, i);
				}
			}
		}
		public static void SendUDPPacket(Packet packet)
		{
			foreach (Client? c in Server.Clients.Values)
			{
				if (c == null) continue;
				c.Udp.SendData(packet);
			}
		}

		public static void SendUserDataRequest(int toClient)
		{
			Console.WriteLine($"Sending user data request to user {toClient}");
			Packet packet = new();
			packet.WriteType(PacketType.UserDataRequest);
			packet.WriteLength();
			SendTCPPacket(packet, toClient);
		}
		public static void SendTCPHandshake(int toClient)
		{
			Console.WriteLine($"Sending TCP handshake to client with id {toClient}");
			Packet packet = new Packet();
			packet.Write(toClient);
			packet.WriteType(PacketType.TCPHandshake);
			packet.WriteLength();
			SendTCPPacket(packet, toClient);
		}
		public static void SendPing(int toClient)
		{
			Packet tcpPing = new Packet();
			tcpPing.Write("TCP Ping!");
			tcpPing.WriteType(PacketType.TCPPing);
			tcpPing.WriteLength();
			SendTCPPacket(tcpPing, toClient);

			Packet udpPing = new();
			udpPing.Write("UDP Ping!");
			udpPing.WriteType(PacketType.UDPPing);
			SendUDPPacket(udpPing, toClient);
		}
		public static void SendRoomPreview(int toClient, int roomId)
		{
			Console.WriteLine($"Sending room preview to client {toClient}");
			Room? room = RoomDatabase.GetRoom(roomId);
			if (room == null) return;
			Packet packet = new();
			packet.Write(room.Name);
			packet.Write(room.Id);
			packet.WriteType(PacketType.RoomPreview);
			packet.WriteLength();
			SendTCPPacket(packet, toClient);

			if (room.Users.Contains(Server.Clients[toClient].User.Name)) return;
			room.Users.Add(Server.Clients[toClient].User.Name);
		}

		public static void UDPHandshakeReceived(Packet packet)
		{
			int fromClient = packet.ReadInt();
			Console.WriteLine($"Got UDP response from client {fromClient}");
			SendUserDataRequest(fromClient);
		}
		public static void TCPPingReceived(Packet packet)
		{
			int id = packet.ReadInt();
			string msg = packet.ReadString();
			Console.WriteLine($"{id}: {msg}");
			SendPing(id);
		}
		public static void UDPPingReceived(Packet packet)
		{
			int id = packet.ReadInt();
			string msg = packet.ReadString();
			Console.WriteLine($"{id}: {msg}");
		}
		public static void VoiceReceived(Packet packet)
		{
			int fromClient = packet.ReadInt();
			packet.WriteType(PacketType.Voice);
			User user = Server.Clients[fromClient].User;
			if (user == null) return;
			VoiceChat? chat = VoiceChatDatabase.GetChat(user.CurrentVoiceChat);
			if (chat == null) return;
			foreach(string s in chat.Users)
			{
				User? otherUser = UserDatabase.GetUser(s);
				if (otherUser == null) continue;
				if(otherUser.Id == 0) continue;
				if (otherUser.Id == fromClient) continue;
				SendUDPPacket(packet, otherUser.Id);
			}
		}

		public static void UserDataResponseReceived(Packet packet)
		{
			int id = packet.ReadInt();
			string username = packet.ReadString();
			Console.WriteLine($"User data from client {id} received! Username: {username}");
			string password = packet.ReadString();
			User? user = UserDatabase.GetUser(username);
			if(user == null)
			{
				Console.WriteLine($"Creating user with name {username}");
				user = new User(username, password);
			}
			Console.WriteLine($"Checking password of {username}");
			if (!UserDatabase.CheckPassword(username, password)) return;
			Console.WriteLine($"User {username} successfully authorized!");
			Server.Clients[id].User = user;

			Packet success = new Packet();
			success.WriteType(PacketType.SuccessfullLogin);
			success.WriteLength();
			Console.WriteLine($"Sending login result to {id}, username {username}");
			SendTCPPacket(success, id);
			foreach (int room in user.Rooms)
			{
				SendRoomPreview(id, room);
			}
		}
		public static void RoomCreationRequestReceived(Packet packet)
		{
			int id = packet.ReadInt();
			string roomName = packet.ReadString();
			Room room = new Room(roomName);
			Console.WriteLine($"Creating room with name {roomName}; Request from client {id}, username: {Server.Clients[id].User.Name}");
			User? user = Server.Clients[id].User;
			user.Rooms.Add(room.Id);
			SendRoomPreview(id, room.Id);
		}
		public static void RoomPreviewRequestReceived(Packet packet)
		{
			int id = packet.ReadInt();
			int roomId = packet.ReadInt();
			User? user = Server.Clients[id].User;
			Room? room = RoomDatabase.GetRoom(roomId);
			Console.WriteLine($"Got room preview request from {id}; Username: {Server.Clients[id].User.Name}; Room: {roomId}");
			if (room == null) return;
			user.Rooms.Add(roomId);
			SendRoomPreview(id, roomId);
		}
		public static void RoomRequestReceived(Packet packet)
		{
			int id = packet.ReadInt();
			int roomId = packet.ReadInt();
			Room? room = RoomDatabase.GetRoom(roomId);
			Console.WriteLine($"Got room request from {id}; Username: {Server.Clients[id].User.Name}; Room: {roomId}");
			if (room == null) return;

			User user = Server.Clients[id].User;

			Packet roomPacket = new();
			roomPacket.Write(room.Id);
			roomPacket.Write(room.TextChats.Count);
			foreach(int textChatID in room.TextChats)
			{
				TextChat? textChat = TextChatDatabase.GetChat(textChatID);
				if (textChat == null) continue;
				roomPacket.Write(textChat.Name);
				roomPacket.Write(textChat.Id);
			}

			roomPacket.Write(room.VoiceChats.Count);
			foreach (int voiceChatId in room.VoiceChats)
			{
				VoiceChat? voiceChat = VoiceChatDatabase.GetChat(voiceChatId);
				if (voiceChat == null) continue;
				roomPacket.Write(voiceChat.Name);
				roomPacket.Write(voiceChat.Id);

				roomPacket.Write(voiceChat.Users.Count);
				for(int j = 0; j < voiceChat.Users.Count; j++)
				{
					roomPacket.Write(voiceChat.Users[j]);
				}
			}

			roomPacket.Write(room.Users.Count);
			for (int i = 0; i < room.Users.Count; i++)
			{
				roomPacket.Write(room.Users[i]);
			}
			roomPacket.WriteType(PacketType.Room);
			roomPacket.WriteLength();
			SendTCPPacket(roomPacket, id);
		}
		public static void VoiceChatConnectionReceived(Packet packet)
		{
			int id = packet.ReadInt();
			int chatId = packet.ReadInt();
			Console.WriteLine($"Voice chat connection request from {id} to chat {chatId}");
			User user = Server.Clients[id].User;
			user.CurrentVoiceChat = chatId;

			SendVoiceChatConnectionResponse(chatId, id);
		}
		public static void SendVoiceChatConnectionResponse(int chatId, int toClient) 
		{
			Packet packet = new();
			packet.Write(chatId);
			packet.WriteType(PacketType.VoiceChatConnectionResponse);
			packet.WriteLength();
			SendTCPPacket(packet, toClient);
		}

		public static void UserJoinedVoiceChat(User user)
		{
			Packet packet = new();
			packet.Write(user.Name);
			VoiceChat? chat = VoiceChatDatabase.GetChat(user.CurrentVoiceChat);
			if (chat == null) return;
			packet.Write(chat.RoomId);
			packet.Write(chat.Id);
			packet.WriteType(PacketType.UserJoinedVoiceChat);

			foreach(string s in RoomDatabase.GetRoom(chat.RoomId).Users)
			{
				User otherUser = UserDatabase.GetUser(s);
				if (otherUser.Id == 0) continue;
				SendUDPPacket(packet, otherUser.Id);
			}
		}
		public static void UserLeftVoiceChat(User user, int chatId)
		{
			Packet packet = new();
			packet.Write(user.Name);
			VoiceChat? chat = VoiceChatDatabase.GetChat(chatId);
			if (chat == null) return;
			packet.Write(chat.RoomId);
			packet.Write(chat.Id);
			packet.WriteType(PacketType.UserLeftVoiceChat);

			foreach(string s in RoomDatabase.GetRoom(chat.RoomId).Users)
			{
				User otherUser = UserDatabase.GetUser(s);
				if (otherUser.Id == 0) continue;
				SendUDPPacket(packet, otherUser.Id);
			}
		}
		public static void DisconnectFromVoiceChatReceived(Packet packet)
		{
			int id = packet.ReadInt();
			Server.Clients[id].User.CurrentVoiceChat = 0;
		}
	}
}
