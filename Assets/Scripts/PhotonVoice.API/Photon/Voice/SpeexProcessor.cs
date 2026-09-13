using System;
using System.Collections.Generic;

namespace Photon.Voice
{
	public class SpeexProcessor : SpeexLib, IProcessor<short>, IDisposable
	{
		public struct AECLatencyResultType
		{
			public int LatencyMs;

			public int LatencyDelayedMs;

			public bool PlayDetected;

			public bool PlayDelayedDetected;

			public bool RecDetected;
		}

		private bool _AEC;

		private int _AECPlaybackDelayMs;

		private bool _AECLatencyDetect;

		private int frameSamples;

		private int samplingRate;

		private int channels;

		private int playDelayFrames;

		private int playDelayMaxFrames;

		private IntPtr stEcho;

		private IntPtr st;

		private bool disposed;

		private short[] resultBuf;

		private PrimitiveArrayPool<short> playbackBufPool;

		private Queue<short[]> playBufQueue = new Queue<short[]>();

		private Framer<float> playFramer;

		private int playSamplingRate;

		private int playChannels;

		private ILogger logger;

		private Func<long> clockMs;

		private AudioUtil.VoiceLevelDetectCalibrate<float> detectPlay;

		private AudioUtil.VoiceLevelDetectCalibrate<short> detectPlayCorr;

		private AudioUtil.VoiceLevelDetectCalibrate<short> detectRec;

		private long detectTimePlay;

		private long detectTimePlayDelayed;

		private long detectTimeRec;

		private int frameCntRec;

		private int frameCntPlay;

		public bool AEC
		{
			get
			{
				return _AEC;
			}
			set
			{
				if (_AEC != value)
				{
					_AEC = value;
					playBufQueue.Clear();
				}
			}
		}

		public int AECFilterLengthMs { get; set; }

		public int AECPlaybackDelayMs
		{
			get
			{
				return _AECPlaybackDelayMs;
			}
			set
			{
				if (_AECPlaybackDelayMs != value)
				{
					_AECPlaybackDelayMs = value;
					InitPlayDelay(value);
				}
			}
		}

		public int AECurrentPlayDelayFrames
		{
			get
			{
				return playBufQueue.Count;
			}
		}

		public bool AECLatencyDetect
		{
			get
			{
				return _AECLatencyDetect;
			}
			set
			{
				if (_AECLatencyDetect != value)
				{
					_AECLatencyDetect = value;
					if (detectPlay == null)
					{
						InitLatencyDetect();
					}
				}
			}
		}

		public AECLatencyResultType AECLatencyResult
		{
			get
			{
				return new AECLatencyResultType
				{
					LatencyMs = (int)(detectTimeRec - detectTimePlay),
					LatencyDelayedMs = (int)(detectTimeRec - detectTimePlayDelayed),
					PlayDetected = (detectPlay != null && detectPlay.VoiceDetector.Detected),
					PlayDelayedDetected = (detectPlayCorr != null && detectPlayCorr.VoiceDetector.Detected),
					RecDetected = (detectRec != null && detectRec.VoiceDetector.Detected)
				};
			}
		}

		public bool Denoise
		{
			get
			{
				return getBool(1);
			}
			set
			{
				set(0, value);
			}
		}

		public bool AGC
		{
			get
			{
				return getBool(3);
			}
			set
			{
				set(2, value);
			}
		}

		public float AGCLevel
		{
			get
			{
				return getFloat(7);
			}
			set
			{
				set(6, value);
			}
		}

		public SpeexProcessor(ILogger logger, Func<long> clockMs, int frameSize, int samplingRate, int channels, int playSamplingRate, int playChannels, int playBufSize)
		{
			this.clockMs = ((clockMs != null) ? clockMs : ((Func<long>)(() => DateTime.Now.Millisecond)));
			this.logger = logger;
			frameSamples = frameSize / channels;
			this.samplingRate = samplingRate;
			this.channels = channels;
			this.playSamplingRate = playSamplingRate;
			this.playChannels = playChannels;
			resultBuf = new short[frameSize];
			st = SpeexLib.speex_preprocess_state_init(frameSamples, samplingRate);
			logger.LogInfo("SpeexProcessor state: create sampling rate {0}, frame samples {1}", samplingRate, frameSamples);
		}

		public void ResetAEC()
		{
			lock (this)
			{
				DestroyEchoState();
			}
		}

		public void AECLatecnyDetectCaliberate()
		{
			detectPlayCorr.Calibrate(2000);
			detectPlay.Calibrate(2000);
			detectRec.Calibrate(2000);
		}

		private void set(int param, bool val)
		{
			int value = (val ? 1 : 0);
			SpeexLib.speex_preprocess_ctl(st, param, ref value);
		}

		private void set(int param, float val)
		{
			SpeexLib.speex_preprocess_ctl(st, param, ref val);
		}

		private bool getBool(int param)
		{
			int value = 0;
			SpeexLib.speex_preprocess_ctl(st, param, ref value);
			return value != 0;
		}

		private float getFloat(int param)
		{
			float value = 0f;
			SpeexLib.speex_preprocess_ctl(st, param, ref value);
			return value;
		}

		private void InitLatencyDetect()
		{
			detectPlay = new AudioUtil.VoiceLevelDetectCalibrate<float>(playSamplingRate, playChannels);
			detectPlayCorr = new AudioUtil.VoiceLevelDetectCalibrate<short>(playSamplingRate, playChannels);
			detectRec = new AudioUtil.VoiceLevelDetectCalibrate<short>(samplingRate, channels);
			detectPlay.VoiceDetector.OnDetected += delegate
			{
				detectTimePlay = clockMs();
			};
			detectPlayCorr.VoiceDetector.OnDetected += delegate
			{
				detectTimePlayDelayed = clockMs();
			};
			detectRec.VoiceDetector.OnDetected += delegate
			{
				detectTimeRec = clockMs();
			};
		}

		public void InitAEC()
		{
			lock (this)
			{
				if (!disposed)
				{
					playFramer = new Framer<float>(frameSamples * playSamplingRate / samplingRate * playChannels);
					InitPlayDelay(AECPlaybackDelayMs);
					int num = samplingRate * AECFilterLengthMs / 1000;
					DestroyEchoState();
					stEcho = SpeexLib.speex_echo_state_init_mc(frameSamples, num, channels, playChannels);
					SpeexLib.speex_echo_ctl(stEcho, 24, ref samplingRate);
					SpeexLib.speex_preprocess_ctl(st, 24, stEcho);
					logger.LogInfo("SpeexProcessor AEC: create sampling rate {0}, frame samples {1}, filter length {2}ms={3}frames, mic channels {4}, out channels {5}, playback delay {6}ms={7}:{8}frames", samplingRate, frameSamples, AECFilterLengthMs, num, channels, playChannels, AECPlaybackDelayMs, playDelayFrames, playDelayMaxFrames);
					logger.LogInfo("SpeexProcessor AEC: output sampling rate {0}", playSamplingRate);
					if (playSamplingRate != samplingRate)
					{
						logger.LogWarning("SpeexProcessor AEC: output sampling rate {0} != {1} capture sampling rate. For better AEC, set audio source (microphone) and audio output samping rates to the same value.", playSamplingRate, samplingRate);
					}
				}
			}
		}

		private void InitPlayDelay(int ms)
		{
			playDelayFrames = ms * samplingRate / frameSamples / 1000;
			playDelayMaxFrames = playDelayFrames * 3;
			playbackBufPool = new PrimitiveArrayPool<short>(playDelayMaxFrames, "Speex playback pool", frameSamples * playChannels);
		}

		public short[] Process(short[] buf)
		{
			if (disposed)
			{
				return buf;
			}
			if (_AECLatencyDetect)
			{
				detectRec.Process(buf);
			}
			if (AEC)
			{
				if (stEcho == IntPtr.Zero)
				{
					InitAEC();
				}
				lock (playBufQueue)
				{
					frameCntRec++;
					if (playBufQueue.Count > playDelayFrames)
					{
						short[] array = playBufQueue.Dequeue();
						SpeexLib.speex_echo_cancellation(stEcho, buf, array, resultBuf);
						if (_AECLatencyDetect)
						{
							detectPlayCorr.Process(array);
						}
						playbackBufPool.Release(array);
						buf = resultBuf;
					}
					else
					{
						logger.LogWarning("SpeexProcessor AEC: playbackBufQueue underrun: {0}", playBufQueue.Count);
					}
				}
			}
			SpeexLib.speex_preprocess_run(st, buf);
			return buf;
		}

		public void OnAudioOutFrame(float[] data, int outChannels)
		{
			if (disposed)
			{
				return;
			}
			lock (this)
			{
				if (stEcho == IntPtr.Zero)
				{
					return;
				}
			}
			if (outChannels != playChannels)
			{
				logger.LogError("SpeexProcessor AEC: OnAudioOutFrame channel count {0} != {1} AudioSettings.speakerMode channel count.", outChannels, playChannels);
				return;
			}
			if (_AECLatencyDetect)
			{
				detectPlay.Process(data);
			}
			foreach (float[] item in playFramer.Frame(data))
			{
				lock (playBufQueue)
				{
					if (playBufQueue.Count > playDelayMaxFrames)
					{
						logger.LogWarning("SpeexProcessor AEC: playbackBufQueue overrun: {0}", playBufQueue.Count);
						while (playBufQueue.Count > playDelayMaxFrames)
						{
							playbackBufPool.Release(playBufQueue.Dequeue());
						}
					}
				}
				short[] array = playbackBufPool.AcquireOrCreate();
				if (item.Length != array.Length)
				{
					AudioUtil.ResampleAndConvert(item, array, array.Length, outChannels);
				}
				else
				{
					AudioUtil.Convert(item, array, array.Length);
				}
				lock (playBufQueue)
				{
					playBufQueue.Enqueue(array);
					frameCntPlay++;
				}
			}
		}

		public void PrintInfo()
		{
		}

		private void DestroyEchoState()
		{
			if (stEcho != IntPtr.Zero)
			{
				SpeexLib.speex_preprocess_ctl(st, 24, IntPtr.Zero);
				SpeexLib.speex_echo_state_destroy(stEcho);
				stEcho = IntPtr.Zero;
			}
		}

		public void Dispose()
		{
			lock (this)
			{
				if (!disposed)
				{
					disposed = true;
					if (st != IntPtr.Zero)
					{
						SpeexLib.speex_preprocess_state_destroy(st);
					}
					DestroyEchoState();
				}
			}
		}
	}
}
