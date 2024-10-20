using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cluster.Network;
using Cluster;

namespace ClusterServer
{
    class Server
    {
        public static Dictionary<int, Client?> Clients { get; private set; } = new();
		public static int MaxClients { get; private set; } = 100;
		static TcpListener TcpListener;
        static UdpClient UdpListener;
        static int Port = 42420;

        public static void Start()
        {
            Clients.Clear();
            for(int i = 1; i < MaxClients + 1; i++)
            {
                Clients.Add(i, null);
            }
            PacketHandler.Start();

            UserDatabase.VoiceChatChanging = TrackVoiceChat;

            TcpListener = new(IPAddress.Any, Port);
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(TcpConnectionCallback, null);

            UdpListener = new(Port);
            UdpListener.BeginReceive(UDPReceiveCallback, null);
        }

        static void TcpConnectionCallback(IAsyncResult result)
        {
            TcpClient client = TcpListener.EndAcceptTcpClient(result);
			TcpListener.BeginAcceptTcpClient(TcpConnectionCallback, null);

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}");

            for(int i = 1; i < MaxClients + 1; i++)
            {
                if (Clients[i] != null) continue;
                Clients[i] = new Client(client, i);
                PacketHandler.SendTCPHandshake(i);
                return;
            }
			Console.WriteLine($"Cannot process {client.Client.RemoteEndPoint}; All slots are full!");
			client.Close();
		}
        static void UDPReceiveCallback(IAsyncResult result)
        {
            IPEndPoint endPoint = new(IPAddress.Any, 0);
            byte[] data = UdpListener.EndReceive(result, ref endPoint);
            UdpListener.BeginReceive(UDPReceiveCallback, null);
            if (data.Length < 4) return;
            Packet packet = new Packet();
            packet.Write(data);

            PacketType type = (PacketType)packet.ReadInt();
            int id = packet.ReadInt();
            packet.MoveReadPosition(-8);
            if (id == 0) return;
            if (Clients[id] == null) return;
            if (Clients[id].Udp.EndPoint == null) 
            {
                Clients[id].Udp.Connect(endPoint);
            }
            PacketHandler.Execute(packet);
        }
        public static void SendUDPData(IPEndPoint endPoint, Packet packet)
        {
            UdpListener.BeginSend(packet.GetBytes(), packet.Length, endPoint, null, null);
        }
        public static void Disconnect(int id)
        {
            Client? client = Clients[id];
            if (client == null) return;
            client.Disconnect();
            Clients[id] = null;
        }
		static void TrackVoiceChat(User user, int from, int to)
		{
			if (from != 0)
			{
				PacketHandler.UserLeftVoiceChat(user, from);
			}
			if (to != 0)
			{
				PacketHandler.UserJoinedVoiceChat(user);
			}
		}
	}
}
