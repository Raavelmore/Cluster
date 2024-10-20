using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Cluster.Network
{
	public class Packet
	{
		public int Length 
		{ 
			get 
			{
				return data.Count;
			}
		}
		public int ReadPosition { get; private set; }
		public int UnreadLength { get { return Length - ReadPosition; } }

		List<byte> data = new();
		
		
		public void Clear()
		{
			data.Clear();
			ReadPosition = 0;
		}
		public byte[] GetBytes()
		{
			return data.ToArray();
		}
		public void MoveReadPosition(int value)
		{
			ReadPosition += value;
			if (ReadPosition < 0) ReadPosition = 0;
		}
		public void RemoveRead()
		{
			data.RemoveRange(0, ReadPosition);
			ReadPosition = 0;
		}
		public void Set(byte[] bytes)
		{
			Clear();
			data.AddRange(bytes);
		}
		public void WriteLength()
		{
			data.InsertRange(0, BitConverter.GetBytes(Length));
		}
		public void WriteType(PacketType type)
		{
			data.InsertRange(0, BitConverter.GetBytes((int)type));
		}
		public void Write(int value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}
		public void Write(float value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}
		public void Write(double value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}
		public void Write(byte value)
		{
			data.Add(value);
		}
		public void Write(byte[] value)
		{
			data.AddRange(value);
		}
		public void Write(byte[] value, int count)
		{
			data.AddRange(value.Take(count).ToArray());
		}
		public void Write(char value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}
		public void Write(string value)
		{
			Write(value.Length * 4);
			Write(Encoding.UTF32.GetBytes(value));
		}
		public void Write(bool value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}


		public int ReadInt()
		{
			int value = BitConverter.ToInt32(ReadBytes(4));
			return value;
		}
		public float ReadFloat()
		{
			float value = BitConverter.ToSingle(ReadBytes(4));
			return value;
		}
		public double ReadDouble()
		{
			double value = BitConverter.ToDouble(ReadBytes(8));
			return value;
		}
		public byte ReadByte()
		{
			byte value = data[ReadPosition];
			ReadPosition += 1;
			return value;
		}
		public byte[] ReadBytes(int count)
		{
			byte[] value = data.Skip(ReadPosition).Take(count).ToArray();
			ReadPosition += count;
			return value;
		}
		public char ReadChar()
		{
			char value = BitConverter.ToChar(ReadBytes(2));
			return value;
		}
		public string ReadString()
		{
			int count = ReadInt();
			string value = Encoding.UTF32.GetString(ReadBytes(count));
			return value;
		}
		public bool ReadBool()
		{
			bool value = BitConverter.ToBoolean(ReadBytes(1));
			return value;
		}
	}
}
