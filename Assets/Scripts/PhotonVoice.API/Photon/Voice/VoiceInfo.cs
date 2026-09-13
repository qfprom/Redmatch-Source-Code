using System.Collections.Generic;
using POpusCodec.Enums;

namespace Photon.Voice
{
	public struct VoiceInfo
	{
		public Codec Codec { get; set; }

		public int SamplingRate { get; set; }

		public int SourceSamplingRate { get; set; }

		public int Channels { get; set; }

		public int FrameDurationUs { get; set; }

		public int Bitrate { get; set; }

		public object UserData { get; set; }

		public int FrameDurationSamples
		{
			get
			{
				return (int)((long)SamplingRate * (long)FrameDurationUs / 1000000);
			}
		}

		public int FrameSize
		{
			get
			{
				return FrameDurationSamples * Channels;
			}
		}

		public int Width { get; set; }

		public int Height { get; set; }

		public static VoiceInfo CreateAudioOpus(SamplingRate samplingRate, int sourceSamplingRate, int channels, OpusCodec.FrameDuration frameDurationUs, int bitrate, object userdata = null)
		{
			return new VoiceInfo
			{
				Codec = Codec.AudioOpus,
				SamplingRate = (int)samplingRate,
				SourceSamplingRate = sourceSamplingRate,
				Channels = channels,
				FrameDurationUs = (int)frameDurationUs,
				Bitrate = bitrate,
				UserData = userdata
			};
		}

		public override string ToString()
		{
			return string.Concat("c=", Codec, " f=", SamplingRate, " ch=", Channels, " d=", FrameDurationUs, " s=", FrameSize, " b=", Bitrate, " w=", Width, " h=", Height, " ud=", UserData);
		}

		internal static VoiceInfo CreateFromEventPayload(Dictionary<byte, object> h)
		{
			return new VoiceInfo
			{
				SamplingRate = (int)h[2],
				Channels = (int)h[3],
				FrameDurationUs = (int)h[4],
				Bitrate = (int)h[5],
				UserData = h[10],
				Codec = (Codec)h[12]
			};
		}
	}
}
