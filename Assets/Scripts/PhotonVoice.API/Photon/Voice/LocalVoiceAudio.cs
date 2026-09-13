using System;

namespace Photon.Voice
{
	public abstract class LocalVoiceAudio<T> : LocalVoiceFramed<T>, ILocalVoiceAudio
	{
		protected AudioUtil.VoiceDetector<T> voiceDetector;

		protected AudioUtil.VoiceDetectorCalibration<T> voiceDetectorCalibration;

		protected AudioUtil.LevelMeter<T> levelMeter;

		protected int channels;

		protected int sourceSamplingRateHz;

		protected bool resampleSource;

		public virtual AudioUtil.IVoiceDetector VoiceDetector
		{
			get
			{
				return voiceDetector;
			}
		}

		public virtual AudioUtil.ILevelMeter LevelMeter
		{
			get
			{
				return levelMeter;
			}
		}

		public bool VoiceDetectorCalibrating
		{
			get
			{
				return voiceDetectorCalibration.IsCalibrating;
			}
		}

		internal LocalVoiceAudio(VoiceClient voiceClient, IEncoderDataFlow<T> encoder, byte id, VoiceInfo voiceInfo, int channelId)
			: base(voiceClient, (IEncoder)encoder, id, voiceInfo, channelId, (voiceInfo.SamplingRate == 0) ? voiceInfo.FrameSize : (voiceInfo.FrameSize * voiceInfo.SourceSamplingRate / voiceInfo.SamplingRate))
		{
			channels = voiceInfo.Channels;
			sourceSamplingRateHz = voiceInfo.SourceSamplingRate;
			if (sourceSamplingRateHz != voiceInfo.SamplingRate)
			{
				resampleSource = true;
				base.voiceClient.transport.LogWarning("[PV] Local voice #" + base.id + " audio source frequency " + sourceSamplingRateHz + " and encoder sampling rate " + voiceInfo.SamplingRate + " do not match. Resampling will occur before encoding.");
			}
		}

		public static LocalVoiceAudio<T> Create(VoiceClient voiceClient, byte voiceId, IEncoder encoder, VoiceInfo voiceInfo, int channelId)
		{
			if (typeof(T) == typeof(float))
			{
				if (encoder == null || encoder is IEncoderDataFlow<float>)
				{
					return new LocalVoiceAudioFloat(voiceClient, encoder as IEncoderDataFlow<float>, voiceId, voiceInfo, channelId) as LocalVoiceAudio<T>;
				}
				throw new Exception("[PV] CreateLocalVoice: encoder for LocalVoiceAudio<float> is not IEncoderDataFlow<float>: " + encoder.GetType());
			}
			if (typeof(T) == typeof(short))
			{
				if (encoder == null || encoder is IEncoderDataFlow<short>)
				{
					return new LocalVoiceAudioShort(voiceClient, encoder as IEncoderDataFlow<short>, voiceId, voiceInfo, channelId) as LocalVoiceAudio<T>;
				}
				throw new Exception("[PV] CreateLocalVoice: encoder for LocalVoiceAudio<short> is not IEncoderDataFlow<short>: " + encoder.GetType());
			}
			throw new UnsupportedSampleTypeException(typeof(T));
		}

		public override IEncoder CreateDefaultEncoder(VoiceInfo info)
		{
			Codec codec = info.Codec;
			if (codec == Codec.AudioOpus)
			{
				return OpusCodec.EncoderFactory.Create<T>(info, base.Logger);
			}
			throw new UnsupportedCodecException(string.Concat("LocalVoiceAudio.CreateDefaultEncoder<", (new T[1])[0].GetType(), ">"), info.Codec, base.Logger);
		}

		public void VoiceDetectorCalibrate(int durationMs)
		{
			voiceDetectorCalibration.Calibrate(durationMs);
		}

		protected void initBuiltinProcessors()
		{
			if (resampleSource)
			{
				AddPostProcessor(new AudioUtil.Resampler<T>(info.FrameSize, channels));
			}
			voiceDetectorCalibration = new AudioUtil.VoiceDetectorCalibration<T>(voiceDetector, levelMeter, info.SamplingRate, channels);
			AddPostProcessor(levelMeter, voiceDetectorCalibration, voiceDetector);
		}
	}
}
