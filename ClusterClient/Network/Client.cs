using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Cluster.Network;
using System.Diagnostics;
using System.IO;

namespace ClusterClient.Network
{
    public class Client
    {
        public static readonly int DataBufferSize = 16384;
        public static Client Instance { get; private set; } = new();
        public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public int Id { get; set; }
		public TCP Tcp { get; private set; }
        public UDP Udp { get; private set; }
        public bool Connected { get; set; }
        public void Connect(string ip, int port)
        {
            Trace.WriteLine($"Starting connection with {ip}:{port}");
            Udp = new(ip, port);
			Tcp = new(ip, port);
		}

        public void Disconnect()
        {
            Tcp.TcpClient.Close();
            Udp.UdpClient.Close();
			Instance.Connected = false;
		}
        public class TCP
        {
            public TcpClient TcpClient { get; private set; }
            byte[] receiveBuffer;
            Packet receiveData;
            NetworkStream stream;
            public TCP(string ip, int port)
            {
                TcpClient = new TcpClient() 
                {
                    ReceiveBufferSize = DataBufferSize,
                    SendBufferSize = DataBufferSize
                };
                TcpClient.BeginConnect(ip, port, ConnectionCallback, null);
            }
            public void SendData(Packet packet)
            {
                if (stream == null)
                {
                    Trace.TraceError("Cannot send data because connection isn't established!");
                    return;
                }
                stream.BeginWrite(packet.GetBytes(), 0, packet.Length, null, null);
            }
            void ConnectionCallback(IAsyncResult result)
            {
                try
                {
                    TcpClient.EndConnect(result);
                }
                catch(Exception e)
                {
					Trace.TraceError(e.ToString());
					Instance.Connected = false;
				}
                if (!TcpClient.Connected) return;
                Trace.WriteLine("Connection established successfully!");
                receiveBuffer = new byte[DataBufferSize];
                receiveData = new();
				stream = TcpClient.GetStream();
                stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                Instance.Connected = true;
			}

            void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if(byteLength <= 0)
                    {
                        Instance.Disconnect();
                        return;
                    }
                    receiveData.Write(receiveBuffer, byteLength);
                    HandleData();
					stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
				}
                catch(Exception e)
                {
                    Trace.TraceError(e.ToString());
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
					if (packetLength < 4)
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
            public UdpClient UdpClient { get; private set; }
            IPEndPoint endPoint;
            int id;
            Packet receiveData;
            public UDP(string ip, int port)
            {
                UdpClient = new UdpClient(ip, port);
                endPoint = new(IPAddress.Parse(ip), port);
                receiveData = new();
                UdpClient.Connect(IPAddress.Parse(ip), port);
                UdpClient.BeginReceive(ReceiveCallback, null);
            }
            public void SendData(Packet packet)
            {
                UdpClient.BeginSend(packet.GetBytes(), packet.Length, null, null);
            }

			void ReceiveCallback(IAsyncResult result)
			{
				try
				{
                    byte[] data = UdpClient.EndReceive(result, ref endPoint);
					UdpClient.BeginReceive(ReceiveCallback, null);
					if (data.Length < 4) return;
                    receiveData.Write(data);
                    HandleData();
				}
				catch (Exception e)
				{
					Trace.TraceError(e.ToString());
				}
			}

			void HandleData()
			{
				Packet packet = new Packet();
				packet.Write(receiveData.GetBytes());
				PacketHandler.Execute(packet);
				receiveData.Clear();
			}
		}
    }
}
