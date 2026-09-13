using System;
using UnityEngine;

namespace Photon.Voice.Unity
{
	[RequireComponent(typeof(Recorder))]
	public class WebRtcAudioDsp : VoiceComponent
	{
		[SerializeField]
		private bool aec = true;

		[SerializeField]
		private bool aecMobile;

		[SerializeField]
		private bool agc = true;

		[SerializeField]
		private bool vad = true;

		[SerializeField]
		private bool highPass;

		[SerializeField]
		private bool bypass;

		[SerializeField]
		private bool noiseSuppression;

		[SerializeField]
		private int reverseStreamDelayMs = 120;

		private int reverseChannels;

		private WebRTCAudioProcessor proc;

		private AudioOutCapture ac;

		private bool started;

		public bool AEC
		{
			get
			{
				return aec;
			}
			set
			{
				if (value != aec)
				{
					if (value)
					{
						aecMobile = false;
					}
					aec = value;
					if (proc != null)
					{
						proc.AEC = aec;
						proc.AECMobile = aecMobile;
					}
					ToggleOutputListener();
				}
			}
		}

		public bool AECMobile
		{
			get
			{
				return aecMobile;
			}
			set
			{
				if (value != aecMobile)
				{
					if (value)
					{
						aec = false;
					}
					aecMobile = value;
					if (proc != null)
					{
						proc.AEC = aec;
						proc.AECMobile = aecMobile;
					}
					ToggleOutputListener();
				}
			}
		}

		public int ReverseStreamDelayMs
		{
			get
			{
				return reverseStreamDelayMs;
			}
			set
			{
				if (reverseStreamDelayMs != value)
				{
					reverseStreamDelayMs = value;
					if (proc != null)
					{
						proc.AECStreamDelayMs = ReverseStreamDelayMs;
					}
				}
			}
		}

		public bool NoiseSuppression
		{
			get
			{
				return noiseSuppression;
			}
			set
			{
				if (value != noiseSuppression)
				{
					noiseSuppression = value;
					if (proc != null)
					{
						proc.NoiseSuppression = noiseSuppression;
					}
				}
			}
		}

		public bool HighPass
		{
			get
			{
				return highPass;
			}
			set
			{
				if (value != highPass)
				{
					highPass = value;
					if (proc != null)
					{
						proc.HighPass = highPass;
					}
				}
			}
		}

		public bool Bypass
		{
			get
			{
				return bypass;
			}
			set
			{
				if (value != bypass)
				{
					bypass = value;
					if (proc != null)
					{
						proc.Bypass = bypass;
					}
				}
			}
		}

		public bool AGC
		{
			get
			{
				return agc;
			}
			set
			{
				if (value != agc)
				{
					agc = value;
					if (proc != null)
					{
						proc.AGC = agc;
					}
				}
			}
		}

		public bool VAD
		{
			get
			{
				return vad;
			}
			set
			{
				if (value != vad)
				{
					vad = value;
					if (proc != null)
					{
						proc.VAD = vad;
					}
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			AudioListener audioListener = UnityEngine.Object.FindObjectOfType<AudioListener>();
			if (audioListener != null)
			{
				ac = audioListener.gameObject.GetComponent<AudioOutCapture>();
				if (ac == null)
				{
					ac = audioListener.gameObject.AddComponent<AudioOutCapture>();
				}
			}
			else if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioListener component is required");
			}
		}

		private void OnEnable()
		{
			ToggleOutputListener();
		}

		private void OnDisable()
		{
			ToggleOutputListener(false);
		}

		private void ToggleOutputListener()
		{
			ToggleOutputListener(aec || aecMobile);
		}

		private void ToggleOutputListener(bool on)
		{
			if (ac != null && started != on && proc != null)
			{
				if (on)
				{
					started = true;
					ac.OnAudioFrame += OnAudioOutFrameFloat;
				}
				else
				{
					started = false;
					ac.OnAudioFrame -= OnAudioOutFrameFloat;
				}
			}
		}

		private void OnAudioOutFrameFloat(float[] data, int outChannels)
		{
			if (outChannels != reverseChannels)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("OnAudioOutFrame channel count {0} != initialized {1}.", outChannels, reverseChannels);
				}
			}
			else
			{
				proc.OnAudioOutFrameFloat(data);
			}
		}

		private void PhotonVoiceCreated(Recorder.PhotonVoiceCreatedParams p)
		{
			LocalVoice voice = p.Voice;
			if (voice.Info.Channels != 1)
			{
				throw new Exception("WebRTCAudioProcessor: only mono audio signals supported.");
			}
			if (!(voice is LocalVoiceAudioShort))
			{
				throw new Exception("WebRTCAudioProcessor: only short audio voice supported (Set Recorder.TypeConvert option).");
			}
			LocalVoiceAudioShort localVoiceAudioShort = voice as LocalVoiceAudioShort;
			reverseChannels = (int)AudioSettings.speakerMode;
			proc = new WebRTCAudioProcessor(base.Logger, voice.Info.FrameSize, voice.Info.SamplingRate, voice.Info.Channels, AudioSettings.outputSampleRate, reverseChannels);
			proc.AEC = AEC;
			proc.AECMobile = AECMobile;
			proc.AECMRoutingMode = 4;
			proc.AECStreamDelayMs = ReverseStreamDelayMs;
			proc.HighPass = HighPass;
			proc.NoiseSuppression = NoiseSuppression;
			proc.AGC = AGC;
			proc.VAD = VAD;
			proc.Bypass = Bypass;
			localVoiceAudioShort.AddPostProcessor(proc);
			ToggleOutputListener();
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Initialized");
			}
		}

		private void PhotonVoiceRemoved()
		{
			Reset();
		}

		private void OnDestroy()
		{
			Reset();
		}

		private void Reset()
		{
			ToggleOutputListener(false);
			if (proc != null)
			{
				proc.Dispose();
				proc = null;
			}
		}
	}
}
