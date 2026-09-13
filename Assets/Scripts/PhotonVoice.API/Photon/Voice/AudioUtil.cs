using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Timers;

namespace Photon.Voice
{
	public static class AudioUtil
	{
		public class ToneAudioReader<T> : IAudioReader<T>, IDataReader<T>, IAudioDesc, IDisposable
		{
			private double k;

			private long timeSamples;

			private Func<double> clockSec;

			private int samplingRate;

			private int channels;

			public int Channels
			{
				get
				{
					return channels;
				}
			}

			public int SamplingRate
			{
				get
				{
					return samplingRate;
				}
			}

			public string Error { get; private set; }

			public ToneAudioReader(Func<double> clockSec = null, double frequency = 440.0, int samplingRate = 441000, int channels = 2)
			{
				this.clockSec = ((clockSec != null) ? clockSec : ((Func<double>)(() => (double)DateTime.Now.Ticks / 10000000.0)));
				this.samplingRate = samplingRate;
				this.channels = channels;
				k = Math.PI * 2.0 * frequency / (double)SamplingRate;
			}

			public void Dispose()
			{
			}

			public bool Read(T[] buf)
			{
				int num = buf.Length / Channels;
				long num2 = (long)(clockSec() * (double)SamplingRate);
				long num3 = num2 - timeSamples;
				if (Math.Abs(num3) > SamplingRate / 4)
				{
					num3 = num;
					timeSamples = num2 - num;
				}
				if (num3 < num)
				{
					return false;
				}
				int num4 = 0;
				if (buf is float[])
				{
					for (int i = 0; i < num; i++)
					{
						float[] array = buf as float[];
						float num5 = (float)(Math.Sin((double)timeSamples++ * this.k) * 0.20000000298023224);
						for (int j = 0; j < Channels; j++)
						{
							array[num4++] = num5;
						}
					}
				}
				else if (buf is short[])
				{
					short[] array2 = buf as short[];
					for (int k = 0; k < num; k++)
					{
						short num6 = (short)(Math.Sin((double)timeSamples++ * this.k) * 0.20000000298023224);
						for (int l = 0; l < Channels; l++)
						{
							array2[num4++] = num6;
						}
					}
				}
				return true;
			}
		}

		public class ToneAudioPusher<T> : IAudioPusher<T>, IAudioDesc, IDisposable
		{
			private double k;

			private System.Timers.Timer timer;

			private Action<T[]> callback;

			private ObjectFactory<T[], int> bufferFactory;

			private int cntFrame;

			private int posSamples;

			private int bufSizeSamples;

			private int samplingRate;

			private int channels;

			public int Channels
			{
				get
				{
					return channels;
				}
			}

			public int SamplingRate
			{
				get
				{
					return samplingRate;
				}
			}

			public string Error { get; private set; }

			public ToneAudioPusher(int frequency = 440, int bufSizeMs = 100, int samplingRate = 441000, int channels = 2)
			{
				this.samplingRate = samplingRate;
				this.channels = channels;
				bufSizeSamples = bufSizeMs * SamplingRate / 1000;
				k = Math.PI * 2.0 * (double)frequency / (double)SamplingRate;
			}

			public void SetCallback(Action<T[]> callback, ObjectFactory<T[], int> bufferFactory)
			{
				if (timer != null)
				{
					Dispose();
				}
				this.callback = callback;
				this.bufferFactory = bufferFactory;
				timer = new System.Timers.Timer(1000.0 * (double)bufSizeSamples / (double)SamplingRate);
				timer.Elapsed += OnTimedEvent;
				timer.Enabled = true;
			}

			private void OnTimedEvent(object source, ElapsedEventArgs e)
			{
				T[] array = bufferFactory.New(bufSizeSamples * Channels);
				int num = 0;
				if (array is float[])
				{
					float[] array2 = array as float[];
					for (int i = 0; i < bufSizeSamples; i++)
					{
						float num2 = (float)(Math.Sin((double)(posSamples + i) * this.k) / 2.0);
						for (int j = 0; j < Channels; j++)
						{
							array2[num++] = num2;
						}
					}
				}
				else if (array is short[])
				{
					short[] array3 = array as short[];
					for (int k = 0; k < bufSizeSamples; k++)
					{
						short num3 = (short)(Math.Sin((double)(posSamples + k) * this.k) * 32767.0 / 2.0);
						for (int l = 0; l < Channels; l++)
						{
							array3[num++] = num3;
						}
					}
				}
				cntFrame++;
				posSamples += bufSizeSamples;
				callback(array);
			}

			public void Dispose()
			{
				timer.Close();
			}
		}

		public class Resampler<T> : IProcessor<T>, IDisposable
		{
			protected T[] frameResampled;

			private int channels;

			public Resampler(int dstSize, int channels)
			{
				frameResampled = new T[dstSize];
				this.channels = channels;
			}

			public T[] Process(T[] buf)
			{
				Resample(buf, frameResampled, frameResampled.Length, channels);
				return frameResampled;
			}

			public void Dispose()
			{
			}
		}

		public interface ILevelMeter
		{
			float CurrentAvgAmp { get; }

			float CurrentPeakAmp { get; }

			float AccumAvgPeakAmp { get; }

			void ResetAccumAvgPeakAmp();
		}

		public class LevelMeterDummy : ILevelMeter
		{
			public float CurrentAvgAmp
			{
				get
				{
					return 0f;
				}
			}

			public float CurrentPeakAmp
			{
				get
				{
					return 0f;
				}
			}

			public float AccumAvgPeakAmp
			{
				get
				{
					return 0f;
				}
			}

			public void ResetAccumAvgPeakAmp()
			{
			}
		}

		public abstract class LevelMeter<T> : IProcessor<T>, ILevelMeter, IDisposable
		{
			protected float ampSum;

			protected float ampPeak;

			protected int bufferSize;

			protected float[] prevValues;

			protected int prevValuesHead;

			protected float accumAvgPeakAmpSum;

			protected int accumAvgPeakAmpCount;

			public float CurrentAvgAmp
			{
				get
				{
					return ampSum / (float)bufferSize;
				}
			}

			public float CurrentPeakAmp { get; protected set; }

			public float AccumAvgPeakAmp
			{
				get
				{
					return (accumAvgPeakAmpCount != 0) ? (accumAvgPeakAmpSum / (float)accumAvgPeakAmpCount) : 0f;
				}
			}

			internal LevelMeter(int samplingRate, int numChannels)
			{
				bufferSize = samplingRate * numChannels / 2;
				prevValues = new float[bufferSize];
			}

			public void ResetAccumAvgPeakAmp()
			{
				accumAvgPeakAmpSum = 0f;
				accumAvgPeakAmpCount = 0;
				ampPeak = 0f;
			}

			public abstract T[] Process(T[] buf);

			public void Dispose()
			{
			}
		}

		public class LevelMeterFloat : LevelMeter<float>
		{
			public LevelMeterFloat(int samplingRate, int numChannels)
				: base(samplingRate, numChannels)
			{
			}

			public override float[] Process(float[] buf)
			{
				foreach (float num in buf)
				{
					float num2 = num;
					if (num2 < 0f)
					{
						num2 = 0f - num2;
					}
					ampSum = ampSum + num2 - prevValues[prevValuesHead];
					prevValues[prevValuesHead] = num2;
					if (ampPeak < num2)
					{
						ampPeak = num2;
					}
					if (prevValuesHead == 0)
					{
						base.CurrentPeakAmp = ampPeak;
						ampPeak = 0f;
						accumAvgPeakAmpSum += base.CurrentPeakAmp;
						accumAvgPeakAmpCount++;
					}
					prevValuesHead = (prevValuesHead + 1) % bufferSize;
				}
				return buf;
			}
		}

		public class LevelMeterShort : LevelMeter<short>
		{
			public LevelMeterShort(int samplingRate, int numChannels)
				: base(samplingRate, numChannels)
			{
			}

			public override short[] Process(short[] buf)
			{
				foreach (short num in buf)
				{
					short num2 = num;
					if (num2 < 0)
					{
						num2 = (short)(-num2);
					}
					ampSum = ampSum + (float)num2 - prevValues[prevValuesHead];
					prevValues[prevValuesHead] = num2;
					if (ampPeak < (float)num2)
					{
						ampPeak = num2;
					}
					if (prevValuesHead == 0)
					{
						base.CurrentPeakAmp = ampPeak;
						ampPeak = 0f;
						accumAvgPeakAmpSum += base.CurrentPeakAmp;
						accumAvgPeakAmpCount++;
					}
					prevValuesHead = (prevValuesHead + 1) % bufferSize;
				}
				return buf;
			}
		}

		public interface IVoiceDetector
		{
			bool On { get; set; }

			float Threshold { get; set; }

			bool Detected { get; }

			DateTime DetectedTime { get; }

			int ActivityDelayMs { get; set; }

			event Action OnDetected;
		}

		public class VoiceDetectorCalibration<T> : IProcessor<T>, IDisposable
		{
			private IVoiceDetector voiceDetector;

			private ILevelMeter levelMeter;

			private int valuesPerSec;

			protected int calibrateCount;

			public bool IsCalibrating
			{
				get
				{
					return calibrateCount > 0;
				}
			}

			public VoiceDetectorCalibration(IVoiceDetector voiceDetector, ILevelMeter levelMeter, int samplingRate, int channels)
			{
				valuesPerSec = samplingRate * channels;
				this.voiceDetector = voiceDetector;
				this.levelMeter = levelMeter;
			}

			public void Calibrate(int durationMs)
			{
				calibrateCount = valuesPerSec * durationMs / 1000;
				levelMeter.ResetAccumAvgPeakAmp();
			}

			public T[] Process(T[] buf)
			{
				if (calibrateCount != 0)
				{
					calibrateCount -= buf.Length;
					if (calibrateCount <= 0)
					{
						calibrateCount = 0;
						voiceDetector.Threshold = levelMeter.AccumAvgPeakAmp * 2f;
					}
				}
				return buf;
			}

			public void Dispose()
			{
			}
		}

		public class VoiceDetectorDummy : IVoiceDetector
		{
			public bool On
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			public float Threshold
			{
				get
				{
					return 0f;
				}
				set
				{
				}
			}

			public bool Detected
			{
				get
				{
					return false;
				}
			}

			public int ActivityDelayMs
			{
				get
				{
					return 0;
				}
				set
				{
				}
			}

			public DateTime DetectedTime { get; private set; }

			public event Action OnDetected
			{
				add
				{
				}
				remove
				{
				}
			}
		}

		public abstract class VoiceDetector<T> : IProcessor<T>, IVoiceDetector, IDisposable
		{
			private bool detected;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private Action OnDetected__BackingField;

			protected int activityDelay;

			protected int autoSilenceCounter;

			protected int valuesCountPerSec;

			protected int activityDelayValuesCount;

			public bool On { get; set; }

			public float Threshold { get; set; }

			public bool Detected
			{
				get
				{
					return detected;
				}
				protected set
				{
					if (detected != value)
					{
						detected = value;
						DetectedTime = DateTime.Now;
						if (detected && this.OnDetected != null)
						{
							this.OnDetected();
						}
					}
				}
			}

			public DateTime DetectedTime { get; private set; }

			public int ActivityDelayMs
			{
				get
				{
					return activityDelay;
				}
				set
				{
					activityDelay = value;
					activityDelayValuesCount = value * valuesCountPerSec / 1000;
				}
			}

			public event Action OnDetected
			{
				add
				{
					Action action = this.OnDetected;
					Action action2;
					do
					{
						action2 = action;
						action = Interlocked.CompareExchange(ref this.OnDetected, (Action)Delegate.Combine(action2, value), action);
					}
					while ((object)action != action2);
				}
				remove
				{
					Action action = this.OnDetected;
					Action action2;
					do
					{
						action2 = action;
						action = Interlocked.CompareExchange(ref this.OnDetected, (Action)Delegate.Remove(action2, value), action);
					}
					while ((object)action != action2);
				}
			}

			internal VoiceDetector(int samplingRate, int numChannels)
			{
				valuesCountPerSec = samplingRate * numChannels;
				ActivityDelayMs = 500;
				On = true;
			}

			public abstract T[] Process(T[] buf);

			public void Dispose()
			{
			}
		}

		public class VoiceDetectorFloat : VoiceDetector<float>
		{
			public VoiceDetectorFloat(int samplingRate, int numChannels)
				: base(samplingRate, numChannels)
			{
				base.Threshold = 0.01f;
			}

			public override float[] Process(float[] buffer)
			{
				if (base.On)
				{
					foreach (float num in buffer)
					{
						if (num > base.Threshold)
						{
							base.Detected = true;
							autoSilenceCounter = 0;
						}
						else
						{
							autoSilenceCounter++;
						}
					}
					if (autoSilenceCounter > activityDelayValuesCount)
					{
						base.Detected = false;
					}
					return (!base.Detected) ? null : buffer;
				}
				return buffer;
			}
		}

		public class VoiceDetectorShort : VoiceDetector<short>
		{
			public VoiceDetectorShort(int samplingRate, int numChannels)
				: base(samplingRate, numChannels)
			{
				base.Threshold = 327.66998f;
			}

			public override short[] Process(short[] buffer)
			{
				if (base.On)
				{
					foreach (short num in buffer)
					{
						if ((float)num > base.Threshold)
						{
							base.Detected = true;
							autoSilenceCounter = 0;
						}
						else
						{
							autoSilenceCounter++;
						}
					}
					if (autoSilenceCounter > activityDelayValuesCount)
					{
						base.Detected = false;
					}
					return (!base.Detected) ? null : buffer;
				}
				return buffer;
			}
		}

		public class VoiceLevelDetectCalibrate<T> : IProcessor<T>, IDisposable
		{
			private VoiceDetectorCalibration<T> calibration;

			public ILevelMeter LevelMeter { get; private set; }

			public IVoiceDetector VoiceDetector { get; private set; }

			public VoiceLevelDetectCalibrate(int samplingRate, int channels)
			{
				T[] array = new T[1];
				if (array[0] is float)
				{
					LevelMeter = new LevelMeterFloat(samplingRate, channels);
					VoiceDetector = new VoiceDetectorFloat(samplingRate, channels);
				}
				else
				{
					if (!(array[0] is short))
					{
						throw new Exception("VoiceLevelDetectCalibrate: type not supported: " + array[0].GetType());
					}
					LevelMeter = new LevelMeterShort(samplingRate, channels);
					VoiceDetector = new VoiceDetectorShort(samplingRate, channels);
				}
				calibration = new VoiceDetectorCalibration<T>(VoiceDetector, LevelMeter, samplingRate, channels);
			}

			public void Calibrate(int durationMs)
			{
				calibration.Calibrate(durationMs);
			}

			public T[] Process(T[] buf)
			{
				buf = (LevelMeter as IProcessor<T>).Process(buf);
				buf = ((IProcessor<T>)calibration).Process(buf);
				buf = (VoiceDetector as IProcessor<T>).Process(buf);
				return buf;
			}

			public void Dispose()
			{
				(LevelMeter as IProcessor<T>).Dispose();
				(VoiceDetector as IProcessor<T>).Dispose();
				calibration.Dispose();
			}
		}

		public static void Resample<T>(T[] src, T[] dst, int dstCount, int channels)
		{
			switch (channels)
			{
			case 1:
			{
				for (int i = 0; i < dstCount; i++)
				{
					dst[i] = src[i * src.Length / dstCount];
				}
				return;
			}
			case 2:
			{
				for (int j = 0; j < dstCount / 2; j++)
				{
					int num = j * src.Length / dstCount;
					int num2 = j * 2;
					int num3 = num * 2;
					dst[num2++] = src[num3++];
					dst[num2] = src[num3];
				}
				return;
			}
			}
			for (int k = 0; k < dstCount / channels; k++)
			{
				int num4 = k * src.Length / dstCount;
				int num5 = k * channels;
				int num6 = num4 * channels;
				for (int l = 0; l < channels; l++)
				{
					dst[num5++] = src[num6++];
				}
			}
		}

		public static void ResampleAndConvert(short[] src, float[] dst, int dstCount, int channels)
		{
			switch (channels)
			{
			case 1:
			{
				for (int i = 0; i < dstCount; i++)
				{
					dst[i] = (float)src[i * src.Length / dstCount] / 32767f;
				}
				return;
			}
			case 2:
			{
				for (int j = 0; j < dstCount / 2; j++)
				{
					int num = j * src.Length / dstCount;
					int num2 = j * 2;
					int num3 = num * 2;
					dst[num2++] = (float)src[num3++] / 32767f;
					dst[num2] = (float)src[num3] / 32767f;
				}
				return;
			}
			}
			for (int k = 0; k < dstCount / channels; k++)
			{
				int num4 = k * src.Length / dstCount;
				int num5 = k * channels;
				int num6 = num4 * channels;
				for (int l = 0; l < channels; l++)
				{
					dst[num5++] = (float)src[num6++] / 32767f;
				}
			}
		}

		public static void ResampleAndConvert(float[] src, short[] dst, int dstCount, int channels)
		{
			switch (channels)
			{
			case 1:
			{
				for (int i = 0; i < dstCount; i++)
				{
					dst[i] = (short)(src[i * src.Length / dstCount] * 32767f);
				}
				return;
			}
			case 2:
			{
				for (int j = 0; j < dstCount / 2; j++)
				{
					int num = j * src.Length / dstCount;
					int num2 = j * 2;
					int num3 = num * 2;
					dst[num2++] = (short)(src[num3++] * 32767f);
					dst[num2] = (short)(src[num3] * 32767f);
				}
				return;
			}
			}
			for (int k = 0; k < dstCount / channels; k++)
			{
				int num4 = k * src.Length / dstCount;
				int num5 = k * channels;
				int num6 = num4 * channels;
				for (int l = 0; l < channels; l++)
				{
					dst[num5++] = (short)(src[num6++] * 32767f);
				}
			}
		}

		public static void Convert(float[] src, short[] dst, int dstCount)
		{
			for (int i = 0; i < dstCount; i++)
			{
				dst[i] = (short)(src[i] * 32767f);
			}
		}

		public static void Convert(short[] src, float[] dst, int dstCount)
		{
			for (int i = 0; i < dstCount; i++)
			{
				dst[i] = (float)src[i] / 32767f;
			}
		}

		public static void ForceToStereo<T>(T[] src, T[] dst, int srcChannels)
		{
			int num = 0;
			for (int i = 0; i < dst.Length - 1; i += 2)
			{
				dst[i] = src[num];
				dst[i + 1] = ((srcChannels <= 1) ? src[num] : src[num + 1]);
				num += srcChannels;
			}
		}

		internal static string tostr<T>(T[] x, int lim = 10)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < ((x.Length >= lim) ? lim : x.Length); i++)
			{
				stringBuilder.Append("-");
				stringBuilder.Append(x[i]);
			}
			return stringBuilder.ToString();
		}
	}
}
