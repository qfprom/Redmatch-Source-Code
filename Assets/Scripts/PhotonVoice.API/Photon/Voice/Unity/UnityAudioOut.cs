using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Voice.Unity
{
	public class UnityAudioOut : ISyncAudioOut, IAudioOut
	{
		private int bufferSamples;

		private int streamSamplePos;

		private AudioSource source;

		private bool started;

		private int sourceTimeSamplesPrev;

		private int playLoopCount;

		private Queue<float[]> frameQueue = new Queue<float[]>();

		public const int FRAME_POOL_CAPACITY = 50;

		private PrimitiveArrayPool<float> framePool = new PrimitiveArrayPool<float>(50, "UnityAudioOut");

		public int Lag
		{
			get
			{
				return 0;
			}
		}

		public int PlaySamplePos
		{
			get
			{
				return started ? (playLoopCount * bufferSamples + source.timeSamples) : 0;
			}
			set
			{
				if (started)
				{
					int num = value % bufferSamples;
					if (num < 0)
					{
						num += bufferSamples;
					}
					source.timeSamples = num;
					playLoopCount = value / bufferSamples;
					sourceTimeSamplesPrev = source.timeSamples;
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				return source.isPlaying;
			}
		}

		public UnityAudioOut(AudioSource audioSource)
		{
			source = audioSource;
		}

		public void Start(int frequency, int channels, int frameSamples, int playDelayMs)
		{
			bufferSamples = playDelayMs * frequency / 1000 + frameSamples + frequency;
			source.loop = true;
			source.clip = AudioClip.Create("AudioStreamPlayer", bufferSamples, channels, frequency, false);
			started = true;
			streamSamplePos = 0;
			PlaySamplePos = 0;
			source.Play();
		}

		public void Service()
		{
			if (!started)
			{
				return;
			}
			lock (frameQueue)
			{
				while (frameQueue.Count > 0)
				{
					float[] array = frameQueue.Dequeue();
					source.clip.SetData(array, streamSamplePos % bufferSamples);
					streamSamplePos += array.Length / source.clip.channels;
					framePool.Release(array);
				}
			}
			if (source.isPlaying)
			{
				if (source.timeSamples < sourceTimeSamplesPrev)
				{
					playLoopCount++;
				}
				sourceTimeSamplesPrev = source.timeSamples;
			}
		}

		public void Push(float[] frame)
		{
			if (frame.Length == 0)
			{
				return;
			}
			if (framePool.Info != frame.Length)
			{
				framePool.Init(frame.Length);
			}
			float[] array = framePool.AcquireOrCreate();
			Buffer.BlockCopy(frame, 0, array, 0, frame.Length * 4);
			lock (frameQueue)
			{
				frameQueue.Enqueue(array);
			}
		}

		public void Stop()
		{
			started = false;
			if (source != null)
			{
				source.clip = null;
			}
		}

		public void Pause()
		{
			source.Pause();
		}

		public void UnPause()
		{
			source.UnPause();
		}
	}
}
