using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Sound
{
	internal class SoundProcessing
	{
		public static float NoiseGateValue = 10.0f;
		static int NoiseGateActivated = 0;
		public static AudioSample NoiseGate(byte[] data)
		{
			short[] average = Average(ConvertBytesToShortArray(data));
			short[] dataShort = ConvertBytesToShortArray(data);

			if (NoiseGateActivated > 0)
			{
				NoiseGateActivated--;
				return new AudioSample(ConvertShortArrayToBytes(dataShort), SampleType.Voice);
			}

			for (int j = 0; j < average.Length; j++)
			{
				if (Math.Abs((double)dataShort[j] / short.MaxValue) > NoiseGateValue / 1000) 
				{
					NoiseGateActivated += 5;
					return new AudioSample(ConvertShortArrayToBytes(dataShort), SampleType.Voice);
				}
			}
			return new AudioSample(ConvertShortArrayToBytes(dataShort), SampleType.Garbage);
		}
		public static short[] Average(short[] data, int bufferLength = 100)
		{
			List<short> lastValues = new();
			short[] samples = data.ToArray();
			for (int i = 0; i < bufferLength; i++)
			{
				lastValues.Add(0);
			}
			for (int i = 0; i < samples.Length; i++)
			{
				lastValues.Add(samples[i]);
				lastValues.RemoveAt(0);
				double average = 0;
				for (int j = 0; j < lastValues.Count; j++)
				{
					average += Convert.ToDouble(lastValues[j]);
				}
				average /= lastValues.Count;
				double value = Convert.ToDouble(samples[i]);
				value = average;
				samples[i] = Convert.ToInt16(value);
			}
			return samples;
		}
		public static AudioSample[] Merge(AudioSample[] samples)
		{
			if (samples.Length < 2) return samples.ToArray();
			List<AudioSample> result = new();
			result.Add(samples[0]);
			int lastSample = 0;
			for(int i = 1; i < samples.Length; i++)
			{
				if (samples[i].Type == result[lastSample].Type)
				{
					result[lastSample].Join(samples[i].Data);
				}
				else
				{
					result.Add(samples[i]);
					lastSample++;
				}
			}
			return result.ToArray();
		}
		public static short[] ConvertBytesToShortArray(byte[] bytes)
		{
			List<short> result = new();
			for(int i = 0; i < bytes.Length; i += 2)
			{
				result.Add(BitConverter.ToInt16(bytes.Skip(i).Take(2).ToArray()));
			}
			return result.ToArray();
		}
		public static byte[] ConvertShortArrayToBytes(short[] data)
		{
			List<byte> result = new();
			for (int i = 0; i < data.Length; i++)
			{
				result.AddRange(BitConverter.GetBytes(data[i]));
			}
			return result.ToArray();
		}
	}
}
