using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Sound
{
	public class AudioSample
	{
		public AudioSample(byte[] data, SampleType type)
		{
			Data = data;
			Type = type;
		}
		public byte[] Data { get; set; }
		public SampleType Type { get; set; }
		public void Join(byte[] data)
		{
			List<byte> newData = new();
			newData.AddRange(Data);
			newData.AddRange(data);
			Data = newData.ToArray();
		}
	}
}
