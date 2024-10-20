using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Utils;
using NAudio.Wave;
using System.Diagnostics;

namespace ClusterClient.Sound
{
	public class Audio
	{
		public static WaveFormat Format = new(44100, 16, 1);
		static WaveInEvent? RecordingDevice;
		static List<byte> RecordedData = new();
		static int MaxBufferSize = 16384;

		static Dictionary<int, AudioOutput> Outputs = new();
		
		public static void Init()
		{
			Thread clear = new(ClearOutputs);
			clear.Start();
			SelectRecordingDevice();
			StartRecording();
		}
		public static void SelectRecordingDevice()
		{
			if (RecordingDevice != null) RecordingDevice.Dispose();
			RecordingDevice = new();
			RecordingDevice.WaveFormat = Format;
			RecordingDevice.DataAvailable += (s, a) =>
			{
				RecordedData.AddRange(a.Buffer);
				if (RecordedData.Count > MaxBufferSize) RecordedData.RemoveRange(0, RecordedData.Count - MaxBufferSize);
			};
		}
		public static void StartRecording()
		{
			RecordingDevice.StartRecording();
		}
		public static void StopRecording()
		{
			RecordingDevice.StopRecording();
		}
		public static byte[] GetRecorded()
		{
			byte[] data = RecordedData.ToArray();
			RecordedData.Clear();
			return data;
		}
		public static void PlaySound(AudioSample sample, int output)
		{
			CreateOutput(output);
			Outputs[output].LoadSound(sample);
		}
		public static void PlaySound(byte[] data, int output)
		{
			CreateOutput(output);
			Outputs[output].LoadSound(data);
		}
		static void CreateOutput(int id)
		{
			if (Outputs.ContainsKey(id)) return;
			Outputs.Add(id, new AudioOutput());
			Outputs[id].Start();
		}
		static void ClearOutputs()
		{
			while (true)
			{
				List<int> disposed = new();
				foreach(int key in Outputs.Keys)
				{
					if (!Outputs[key].NeedDispose()) continue;
					Outputs[key].Dispose();
					disposed.Add(key);
				}
				lock (Outputs)
				{
					foreach (int key in disposed)
					{
						Outputs.Remove(key);
					}
				}
				Thread.Sleep(10000);
			}
		}
	}
}
