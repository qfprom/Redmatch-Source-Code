using System;
using System.Linq;
using POpusCodec.Enums;
using Photon.Voice.Windows;
using UnityEngine;
using UnityEngine.Serialization;

namespace Photon.Voice.Unity
{
	[AddComponentMenu("Photon Voice/Recorder")]
	[HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/recorder")]
	public class Recorder : VoiceComponent
	{
		public enum InputSourceType
		{
			Microphone = 0,
			AudioClip = 1,
			Factory = 2
		}

		public enum MicType
		{
			Unity = 0,
			Photon = 1
		}

		public enum SampleTypeConv
		{
			None = 0,
			Short = 1
		}

		public class PhotonVoiceCreatedParams
		{
			public LocalVoice Voice { get; internal set; }

			public IAudioDesc AudioDesc { get; internal set; }
		}

		[SerializeField]
		private bool voiceDetection;

		[SerializeField]
		private float voiceDetectionThreshold = 0.01f;

		[SerializeField]
		private int voiceDetectionDelayMs = 500;

		private object userData;

		private LocalVoice voice = LocalVoiceAudioDummy.Dummy;

		private string unityMicrophoneDevice;

		private int photonMicrophoneDeviceId = -1;

		private IAudioDesc inputSource;

		private VoiceClient client;

		private bool forceShort;

		[SerializeField]
		[FormerlySerializedAs("audioGroup")]
		private byte interestGroup;

		[SerializeField]
		private bool debugEchoMode;

		[SerializeField]
		private bool reliableMode;

		[SerializeField]
		private bool encrypt;

		[SerializeField]
		private bool transmitEnabled;

		[SerializeField]
		private SamplingRate samplingRate = SamplingRate.Sampling24000;

		[SerializeField]
		private OpusCodec.FrameDuration frameDuration = OpusCodec.FrameDuration.Frame20ms;

		[SerializeField]
		private int bitrate = 30000;

		[SerializeField]
		private InputSourceType sourceType;

		[SerializeField]
		private MicType microphoneType;

		[SerializeField]
		private SampleTypeConv typeConvert;

		[SerializeField]
		private AudioClip audioClip;

		[SerializeField]
		private bool loopAudioClip = true;

		private bool requiresInit = true;

		private Func<IAudioDesc> inputFactory;

		private static AudioInEnumerator photonMicrophoneEnumerator;

		public static AudioInEnumerator PhotonMicrophoneEnumerator
		{
			get
			{
				if (photonMicrophoneEnumerator == null)
				{
					VoiceLogger voiceLogger = new VoiceLogger("AudioInEnumerator");
					photonMicrophoneEnumerator = new AudioInEnumerator(voiceLogger);
					if (photonMicrophoneEnumerator.Error != null && voiceLogger.IsErrorEnabled)
					{
						voiceLogger.LogError("PhotonVoice: Can't create instance of AudioInEnumerator: {0}", photonMicrophoneEnumerator.Error);
					}
				}
				return photonMicrophoneEnumerator;
			}
		}

		public bool IsInitialized { get; private set; }

		public bool RequiresInit
		{
			get
			{
				return requiresInit;
			}
			private set
			{
				requiresInit = value;
			}
		}

		public bool TransmitEnabled
		{
			get
			{
				return transmitEnabled;
			}
			set
			{
				if (value != transmitEnabled)
				{
					transmitEnabled = value;
					if (voice != LocalVoiceAudioDummy.Dummy)
					{
						voice.TransmitEnabled = value;
					}
				}
			}
		}

		public bool Encrypt
		{
			get
			{
				return encrypt;
			}
			set
			{
				if (encrypt != value)
				{
					encrypt = value;
					voice.Encrypt = value;
				}
			}
		}

		public bool DebugEchoMode
		{
			get
			{
				return debugEchoMode;
			}
			set
			{
				if (debugEchoMode == value)
				{
					return;
				}
				if (InterestGroup != 0)
				{
					if (base.Logger.IsWarningEnabled)
					{
						base.Logger.LogWarning("Cannot enable DebugEchoMode when AudioGroup value ({0}) is different than 0.", InterestGroup);
					}
				}
				else
				{
					debugEchoMode = value;
					voice.DebugEchoMode = value;
				}
			}
		}

		public bool ReliableMode
		{
			get
			{
				return reliableMode;
			}
			set
			{
				if (voice != LocalVoiceAudioDummy.Dummy)
				{
					voice.Reliable = value;
				}
				reliableMode = value;
			}
		}

		public bool VoiceDetection
		{
			get
			{
				return voiceDetection;
			}
			set
			{
				voiceDetection = value;
				if (VoiceDetector != null)
				{
					VoiceDetector.On = value;
				}
			}
		}

		public float VoiceDetectionThreshold
		{
			get
			{
				return voiceDetectionThreshold;
			}
			set
			{
				if (voiceDetectionThreshold.Equals(value))
				{
					return;
				}
				if (value < 0f || value > 1f)
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Value out of range: VAD Threshold needs to be between [0..1], requested value: {0}", value);
					}
				}
				else
				{
					voiceDetectionThreshold = value;
					if (VoiceDetector != null)
					{
						VoiceDetector.Threshold = value;
					}
				}
			}
		}

		public int VoiceDetectionDelayMs
		{
			get
			{
				return voiceDetectionDelayMs;
			}
			set
			{
				if (voiceDetectionDelayMs != value)
				{
					voiceDetectionDelayMs = value;
					if (VoiceDetector != null)
					{
						VoiceDetector.ActivityDelayMs = value;
					}
				}
			}
		}

		public object UserData
		{
			get
			{
				return userData;
			}
			set
			{
				if (userData != value)
				{
					userData = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "UserData");
					}
				}
			}
		}

		public Func<IAudioDesc> InputFactory
		{
			get
			{
				return inputFactory;
			}
			set
			{
				if (inputFactory != value)
				{
					inputFactory = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "InputFactory");
					}
				}
			}
		}

		public AudioUtil.IVoiceDetector VoiceDetector
		{
			get
			{
				return (voiceAudio == null) ? null : voiceAudio.VoiceDetector;
			}
		}

		public string UnityMicrophoneDevice
		{
			get
			{
				if (!string.IsNullOrEmpty(unityMicrophoneDevice) && !Microphone.devices.Contains(unityMicrophoneDevice))
				{
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("\"{0}\" is not a valid Unity microphone device, switching to default (null)", unityMicrophoneDevice);
					}
					unityMicrophoneDevice = null;
				}
				return unityMicrophoneDevice;
			}
			set
			{
				if (!string.IsNullOrEmpty(value) && !Microphone.devices.Contains(value))
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("\"{0}\" is not a valid Unity microphone device", value);
					}
				}
				else if ((value == null && unityMicrophoneDevice != null) || (value != null && !value.Equals(unityMicrophoneDevice)))
				{
					unityMicrophoneDevice = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "UnityMicrophoneDevice");
					}
				}
			}
		}

		public int PhotonMicrophoneDeviceId
		{
			get
			{
				if (!PhotonMicrophoneEnumerator.IsSupported)
				{
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Photon microphone device IDs are not supported on this platform");
					}
					photonMicrophoneDeviceId = -1;
				}
				else if (!PhotonMicrophoneEnumerator.IDIsValid(photonMicrophoneDeviceId))
				{
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("\"{0}\" is not a valid Photon microphone device, switching to default (-1)", photonMicrophoneDeviceId);
					}
					photonMicrophoneDeviceId = -1;
				}
				return photonMicrophoneDeviceId;
			}
			set
			{
				if (!PhotonMicrophoneEnumerator.IsSupported)
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Setting a Photon microphone device ID is not supported on this platform");
					}
				}
				else if (!PhotonMicrophoneEnumerator.IDIsValid(value))
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("\"{0}\" is not a valid Photon microphone device", value);
					}
				}
				else if (photonMicrophoneDeviceId != value)
				{
					photonMicrophoneDeviceId = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "PhotonMicrophoneDeviceId");
					}
				}
			}
		}

		[Obsolete("Use InterestGroup instead")]
		public byte AudioGroup
		{
			get
			{
				return InterestGroup;
			}
			set
			{
				InterestGroup = value;
			}
		}

		public byte InterestGroup
		{
			get
			{
				return interestGroup;
			}
			set
			{
				if (interestGroup != value)
				{
					interestGroup = value;
					voice.InterestGroup = value;
				}
			}
		}

		public bool IsCurrentlyTransmitting
		{
			get
			{
				return voice.IsCurrentlyTransmitting;
			}
		}

		public AudioUtil.ILevelMeter LevelMeter
		{
			get
			{
				return (voiceAudio == null) ? null : voiceAudio.LevelMeter;
			}
		}

		public bool VoiceDetectorCalibrating
		{
			get
			{
				return voiceAudio.VoiceDetectorCalibrating;
			}
		}

		protected ILocalVoiceAudio voiceAudio
		{
			get
			{
				return voice as ILocalVoiceAudio;
			}
		}

		public InputSourceType SourceType
		{
			get
			{
				return sourceType;
			}
			set
			{
				if (sourceType != value)
				{
					sourceType = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "Source");
					}
				}
			}
		}

		public MicType MicrophoneType
		{
			get
			{
				return microphoneType;
			}
			set
			{
				if (microphoneType != value)
				{
					microphoneType = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "MicrophoneType");
					}
				}
			}
		}

		public SampleTypeConv TypeConvert
		{
			get
			{
				return typeConvert;
			}
			set
			{
				if (typeConvert != value)
				{
					typeConvert = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "TypeConvert");
					}
				}
			}
		}

		public AudioClip AudioClip
		{
			get
			{
				return audioClip;
			}
			set
			{
				if (audioClip != value)
				{
					audioClip = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "AudioClip");
					}
				}
			}
		}

		public bool LoopAudioClip
		{
			get
			{
				return loopAudioClip;
			}
			set
			{
				if (loopAudioClip == value)
				{
					return;
				}
				loopAudioClip = value;
				AudioClipWrapper audioClipWrapper = inputSource as AudioClipWrapper;
				if (audioClipWrapper != null)
				{
					audioClipWrapper.Loop = value;
					return;
				}
				RequiresInit = true;
				if (IsInitialized && base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "LoopAudioClip");
				}
			}
		}

		public SamplingRate SamplingRate
		{
			get
			{
				return samplingRate;
			}
			set
			{
				if (samplingRate != value)
				{
					samplingRate = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "SamplingRate");
					}
				}
			}
		}

		public OpusCodec.FrameDuration FrameDuration
		{
			get
			{
				return frameDuration;
			}
			set
			{
				if (frameDuration != value)
				{
					frameDuration = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "FrameDuration");
					}
				}
			}
		}

		public int Bitrate
		{
			get
			{
				return bitrate;
			}
			set
			{
				if (bitrate != value)
				{
					bitrate = value;
					RequiresInit = true;
					if (IsInitialized && base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires reinitialization for this to take effect.", "Bitrate");
					}
				}
			}
		}

		public void Init(VoiceClient voiceClient, object customObj = null)
		{
			if (IsInitialized && !RequiresInit)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Recorder already initialized.");
				}
				return;
			}
			if (IsInitialized)
			{
				RemoveVoice(true);
			}
			client = voiceClient;
			userData = customObj;
			SampleTypeConv sampleTypeConv = TypeConvert;
			if (sampleTypeConv == SampleTypeConv.Short)
			{
				forceShort = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Type Conversion set to Short. Audio samples will be converted if source samples type differs.");
				}
			}
			Setup();
		}

		public void ReInit()
		{
			if (!IsInitialized)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Recorder is not initialized. Call Recorder.Init(VoiceClient, Object) instead.");
				}
			}
			else if (!RequiresInit)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Recorder does not require reinitialization.");
				}
			}
			else
			{
				RemoveVoice(true);
				Setup();
			}
		}

		public void VoiceDetectorCalibrate(int durationMs)
		{
			if (voiceAudio != null)
			{
				voiceAudio.VoiceDetectorCalibrate(durationMs);
			}
		}

		private void Setup()
		{
			voice = CreateLocalVoiceAudioAndSource();
			if (VoiceDetector != null)
			{
				VoiceDetector.Threshold = VoiceDetectionThreshold;
				VoiceDetector.ActivityDelayMs = VoiceDetectionDelayMs;
				VoiceDetector.On = VoiceDetection;
			}
			voice.InterestGroup = InterestGroup;
			voice.DebugEchoMode = DebugEchoMode;
			voice.Encrypt = Encrypt;
			voice.Reliable = ReliableMode;
			RequiresInit = false;
			IsInitialized = true;
			SendPhotonVoiceCreatedMessage();
			voice.TransmitEnabled = TransmitEnabled;
		}

		private LocalVoice CreateLocalVoiceAudioAndSource()
		{
			switch (SourceType)
			{
			case InputSourceType.Microphone:
			{
				if (MicrophoneType == MicType.Photon)
				{
					int num = PhotonMicrophoneDeviceId;
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Setting recorder's source to Photon microphone device [{0}] \"{1}\"", num, PhotonMicrophoneEnumerator.NameAtIndex(num));
					}
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Setting recorder's source to WindowsAudioInPusher");
					}
					inputSource = new WindowsAudioInPusher(num, base.Logger);
					if (inputSource.Error == null)
					{
						break;
					}
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Photon microphone input source creation failure: {0}. Falling back to Unity microphone", inputSource.Error);
					}
				}
				if (Microphone.devices.Length < 1)
				{
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("No Microphone");
					}
					return LocalVoiceAudioDummy.Dummy;
				}
				string text = UnityMicrophoneDevice;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Setting recorder's source to Unity microphone device {0}", text);
				}
				inputSource = new MicWrapper(text, (int)SamplingRate, base.Logger);
				if (inputSource.Error != null && base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Unity microphone input source creation failure: {0}.", inputSource.Error);
				}
				break;
			}
			case InputSourceType.AudioClip:
				if (AudioClip == null)
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("AudioClip property must be set for AudioClip audio source");
					}
					return LocalVoiceAudioDummy.Dummy;
				}
				inputSource = new AudioClipWrapper(AudioClip);
				if (LoopAudioClip)
				{
					((AudioClipWrapper)inputSource).Loop = true;
				}
				break;
			case InputSourceType.Factory:
				if (InputFactory == null)
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Recorder.InputFactory must be specified if Recorder.Source set to Factory");
					}
					return LocalVoiceAudioDummy.Dummy;
				}
				inputSource = InputFactory();
				if (inputSource.Error != null && base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("InputFactory creation failure: {0}.", inputSource.Error);
				}
				break;
			default:
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("unknown Source value {0}", SourceType);
				}
				return LocalVoiceAudioDummy.Dummy;
			}
			VoiceInfo voiceInfo = VoiceInfo.CreateAudioOpus(SamplingRate, inputSource.SamplingRate, inputSource.Channels, FrameDuration, Bitrate, UserData);
			return client.CreateLocalVoiceAudioFromSource(voiceInfo, inputSource, forceShort);
		}

		protected virtual void SendPhotonVoiceCreatedMessage()
		{
			base.gameObject.SendMessage("PhotonVoiceCreated", new PhotonVoiceCreatedParams
			{
				Voice = voice,
				AudioDesc = inputSource
			}, SendMessageOptions.DontRequireReceiver);
		}

		private void OnDestroy()
		{
			RemoveVoice(false);
		}

		private void RemoveVoice(bool sendUnityMsg)
		{
			if (VoiceDetector != null)
			{
				voiceDetectionThreshold = VoiceDetector.Threshold;
			}
			if (voice != LocalVoiceAudioDummy.Dummy)
			{
				voice.RemoveSelf();
			}
			if (inputSource != null)
			{
				inputSource.Dispose();
				inputSource = null;
			}
			if (sendUnityMsg)
			{
				base.gameObject.SendMessage("PhotonVoiceRemoved", SendMessageOptions.DontRequireReceiver);
			}
			IsInitialized = false;
		}
	}
}
