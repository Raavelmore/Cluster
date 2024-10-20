using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Sound
{
	internal class AudioOutput
	{
		float gain = 1.0f;
		public float Gain {
			get
			{
				return gain;
			}
			set
			{
				gain = value;
				volumeSampleProvider.Volume = gain;
			}
		}
		public DirectSoundOut Output { get; private set; } = new();
		BufferedWaveProvider waveProvider = new(Audio.Format);
		VolumeSampleProvider volumeSampleProvider;
		bool playing = false;
		DateTime lastSoundTime = DateTime.UtcNow;

		public void Start()
		{
			if (playing) return;
			volumeSampleProvider = new(waveProvider.ToSampleProvider());
			Output.Init(volumeSampleProvider);
			Output.Play();
			playing = true;
		}
		public void Stop()
		{
			if (!playing) return;
			Output.Stop();
			playing = false;
		}
		public void LoadSound(AudioSample sample)
		{
			if (!playing) return;
			if (sample.Type == SampleType.Garbage) return;
			LoadSound(sample.Data);
		}
		public void LoadSound(byte[] data)
		{
			if (!playing) return;
			lastSoundTime = DateTime.UtcNow;
			waveProvider.AddSamples(data, 0, data.Length);
		}
		public bool NeedDispose()
		{
			return (DateTime.UtcNow - lastSoundTime).TotalSeconds > 60;
		}
		public void Dispose()
		{
			Stop();
			Output.Dispose();
		}
	}
}
