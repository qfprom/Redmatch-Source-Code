using System;

namespace Photon.Voice
{
	public class WebRTCAudioProcessor : WebRTCAudioLib, IProcessor<short>, IDisposable
	{
		private int reverseStreamDelayMs;

		private bool aec;

		private bool aecm;

		private int aecmRoutingMode;

		private bool aecmComfortNoise;

		private bool highPass;

		private bool ns;

		private bool agc;

		private bool vad;

		private int inFrameSize;

		private int processFrameSize;

		private int samplingRate;

		private int channels;

		private IntPtr proc;

		private bool disposed;

		private Framer<float> reverseFramer;

		private short[] reverseBuf;

		private int reverseSamplingRate;

		private int reverseChannels;

		private ILogger logger;

		private const int supportedFrameLenMs = 10;

		private int[] supportedSamplingRates = new int[4] { 8000, 16000, 32000, 48000 };

		private bool aecInited;

		private int lastProcessErr;

		private int lastProcessReverseErr;

		public int AECStreamDelayMs
		{
			set
			{
				if (reverseStreamDelayMs != value)
				{
					reverseStreamDelayMs = value;
					if (proc != IntPtr.Zero)
					{
						setParam(1, value);
					}
				}
			}
		}

		public bool AEC
		{
			set
			{
				if (aec != value)
				{
					aec = value;
					InitReverseStream();
					if (proc != IntPtr.Zero)
					{
						setParam(10, aec ? 1 : 0);
					}
					aecm = !aec && aecm;
				}
			}
		}

		public bool AECMobile
		{
			set
			{
				if (aecm != value)
				{
					aecm = value;
					InitReverseStream();
					if (proc != IntPtr.Zero)
					{
						setParam(20, aecm ? 1 : 0);
					}
					aec = !aecm && aec;
				}
			}
		}

		public int AECMRoutingMode
		{
			set
			{
				if (aecmRoutingMode != value)
				{
					aecmRoutingMode = value;
					if (proc != IntPtr.Zero)
					{
						setParam(21, value);
					}
				}
			}
		}

		public bool AECMComfortNoise
		{
			set
			{
				if (aecmComfortNoise != value)
				{
					aecmComfortNoise = value;
					if (proc != IntPtr.Zero)
					{
						setParam(22, value ? 1 : 0);
					}
				}
			}
		}

		public bool HighPass
		{
			set
			{
				if (highPass != value)
				{
					highPass = value;
					if (proc != IntPtr.Zero)
					{
						setParam(31, value ? 1 : 0);
					}
				}
			}
		}

		public bool NoiseSuppression
		{
			set
			{
				if (ns != value)
				{
					ns = value;
					if (proc != IntPtr.Zero)
					{
						setParam(41, value ? 1 : 0);
					}
				}
			}
		}

		public bool AGC
		{
			set
			{
				if (agc != value)
				{
					agc = value;
					if (proc != IntPtr.Zero)
					{
						setParam(51, value ? 1 : 0);
					}
				}
			}
		}

		public bool VAD
		{
			set
			{
				if (vad != value)
				{
					vad = value;
					if (proc != IntPtr.Zero)
					{
						setParam(61, value ? 1 : 0);
					}
				}
			}
		}

		public bool Bypass { private get; set; }

		public WebRTCAudioProcessor(ILogger logger, int frameSize, int samplingRate, int channels, int reverseSamplingRate, int reverseChannels)
		{
			bool flag = false;
			int[] array = supportedSamplingRates;
			foreach (int num in array)
			{
				if (samplingRate == num)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				logger.LogError("WebRTCAudioProcessor: input sampling rate ({0}) must be 8000, 16000, 32000 or 48000", samplingRate);
				disposed = true;
				return;
			}
			this.logger = logger;
			inFrameSize = frameSize;
			processFrameSize = samplingRate * 10 / 1000;
			if (inFrameSize / processFrameSize * processFrameSize != inFrameSize)
			{
				logger.LogError("WebRTCAudioProcessor: input frame size ({0} samples / {1} ms) must be equal to or N times more than webrtc processing frame size ({2} samples / 10 ms)", inFrameSize, 1000f * (float)inFrameSize / (float)samplingRate, processFrameSize);
				disposed = true;
				return;
			}
			this.samplingRate = samplingRate;
			this.channels = channels;
			this.reverseSamplingRate = reverseSamplingRate;
			this.reverseChannels = reverseChannels;
			proc = WebRTCAudioLib.webrtc_audio_processor_create(samplingRate, channels, processFrameSize, samplingRate, reverseChannels);
			setConfigParam(12, 1);
			setConfigParam(13, 1);
			WebRTCAudioLib.webrtc_audio_processor_init(proc);
			if (inFrameSize != processFrameSize)
			{
				logger.LogWarning("WebRTCAudioProcessor: Frame size is {0} ms. For efficency, set it to 10 ms.", 1000 * inFrameSize / samplingRate);
			}
			logger.LogInfo("WebRTCAudioProcessor create sampling rate {0}, frame samples {1}", samplingRate, inFrameSize / this.channels);
		}

		private void InitReverseStream()
		{
			lock (this)
			{
				if (!aecInited && !disposed)
				{
					int frameSize = processFrameSize * reverseSamplingRate / samplingRate * reverseChannels;
					reverseFramer = new Framer<float>(frameSize);
					reverseBuf = new short[processFrameSize * reverseChannels / channels];
					if (reverseSamplingRate != samplingRate)
					{
						logger.LogWarning("WebRTCAudioProcessor AEC: output sampling rate {0} != {1} capture sampling rate. For better AEC, set audio source (microphone) and audio output samping rates to the same value.", reverseSamplingRate, samplingRate);
					}
					aecInited = true;
				}
			}
		}

		public short[] Process(short[] buf)
		{
			if (Bypass)
			{
				return buf;
			}
			if (disposed)
			{
				return buf;
			}
			if (proc == IntPtr.Zero)
			{
				return buf;
			}
			if (buf.Length != inFrameSize)
			{
				logger.LogError("WebRTCAudioProcessor Process: frame size expected: {0}, passed: {1}", inFrameSize, buf);
				return buf;
			}
			bool flag = false;
			for (int i = 0; i < inFrameSize; i += processFrameSize)
			{
				bool voiceDetected = true;
				int num = WebRTCAudioLib.webrtc_audio_processor_process(proc, buf, i, out voiceDetected);
				if (voiceDetected)
				{
					flag = true;
				}
				if (lastProcessErr != num)
				{
					lastProcessErr = num;
					logger.LogError("WebRTCAudioProcessor Process: webrtc_audio_processor_process() error {0}", num);
					return buf;
				}
			}
			if (vad && !flag)
			{
				return null;
			}
			return buf;
		}

		public void OnAudioOutFrameFloat(float[] data)
		{
			if (disposed || proc == IntPtr.Zero)
			{
				return;
			}
			foreach (float[] item in reverseFramer.Frame(data))
			{
				if (item.Length != reverseBuf.Length)
				{
					AudioUtil.ResampleAndConvert(item, reverseBuf, reverseBuf.Length, reverseChannels);
				}
				else
				{
					AudioUtil.Convert(item, reverseBuf, reverseBuf.Length);
				}
				int num = WebRTCAudioLib.webrtc_audio_processor_process_reverse(proc, reverseBuf, reverseBuf.Length);
				if (lastProcessReverseErr != num)
				{
					lastProcessReverseErr = num;
					logger.LogError("WebRTCAudioProcessor OnAudioOutFrameFloat: webrtc_audio_processor_process_reverse() error {0}", num);
				}
			}
		}

		private int setParam(int param, int v)
		{
			return WebRTCAudioLib.webrtc_audio_processor_set_param(proc, param, v);
		}

		private int setConfigParam(int param, int v)
		{
			return WebRTCAudioLib.webrtc_audio_processor_set_config_param(proc, param, v);
		}

		public void Dispose()
		{
			lock (this)
			{
				if (!disposed)
				{
					disposed = true;
					if (proc != IntPtr.Zero)
					{
						WebRTCAudioLib.webrtc_audio_processor_destroy(proc);
					}
				}
			}
		}
	}
}
