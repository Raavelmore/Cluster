using Cluster;
using Cluster.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClusterServer
{
	public class Client
	{
		public static readonly int DataBufferSize = 16384;
		User? user;
		public User? User
		{
			get
			{
				return user;
			}
			set
			{
				if (user != null) 
				{
					user.CurrentVoiceChat = 0;
					user.Id = 0;
				}
				user = value;
				if(user != null) user.Id = Id;
			}
		}
		public int Id { get; private set; }
		public TCP Tcp { get; private set; }
		public UDP Udp { get; private set; }

		public Client(TcpClient client, int id)
		{
			Id = id;
			Tcp = new(client, id);
			Udp = new();
		}

		public void Disconnect()
		{
			Console.WriteLine($"Disconnecting client with id {Id}");
			User = null;
			Tcp.TcpClient.Close();
			Udp.Disconnect();
		}
		public class TCP
		{
			public TcpClient TcpClient { get; private set; }
			byte[] receiveBuffer;
			Packet receiveData;
			NetworkStream stream;
			int id;
			public TCP(TcpClient client, int id)
			{
				this.id = id;
				TcpClient = client;
				TcpClient.SendBufferSize = DataBufferSize;
				TcpClient.ReceiveBufferSize = DataBufferSize;
				stream = TcpClient.GetStream();
				receiveBuffer = new byte[DataBufferSize];
				receiveData = new();
				stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
			}
			public void SendData(Packet packet)
			{
				if (stream == null)
				{
					Console.WriteLine("Cannot send data because connection isn't established!");
					return;
				}
				stream.BeginWrite(packet.GetBytes(), 0, packet.Length, null, null);
			}

			void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					int byteLength = stream.EndRead(result);
					if (byteLength <= 0)
					{
						Server.Disconnect(id);
						return;
					}
					receiveData.Write(receiveBuffer, byteLength);
					HandleData();
					stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
				}
				catch (Exception e)
				{
					Server.Disconnect(id);
					Console.WriteLine(e.ToString());
				}
			}

			void HandleData()
			{
				int packetLength;
				do
				{
					packetLength = 0;
					if (receiveData.UnreadLength < 4)
					{
						receiveData.Clear(); //Нечего читать
						return;
					}
					packetLength = receiveData.ReadInt();
					if(packetLength < 4)
					{
						receiveData.Clear();
						return;
					}
					if (packetLength > receiveData.UnreadLength)
					{
						receiveData.MoveReadPosition(-4);
						return;
					}
					Packet packet = new Packet(); //Считываем пакет
					packet.Write(receiveData.ReadBytes(packetLength));

					receiveData.RemoveRead(); //Удаляем, что считали

					PacketHandler.Execute(packet);
				}
				while (packetLength > 0);
			}
		}
		public class UDP
		{
			public IPEndPoint EndPoint { get; private set; }
			public void Connect(IPEndPoint endPoint)
			{
				this.EndPoint = endPoint;
			}
			public void SendData(Packet packet)
			{
				Server.SendUDPData(EndPoint, packet);
			}
			public void Disconnect()
			{
				EndPoint = null;
			}
		}
	}
}
