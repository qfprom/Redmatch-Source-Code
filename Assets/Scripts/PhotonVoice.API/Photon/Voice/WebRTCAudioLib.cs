using System;
using System.Runtime.InteropServices;

namespace Photon.Voice
{
	public class WebRTCAudioLib
	{
		private enum Error
		{
			kNoError = 0,
			kUnspecifiedError = -1,
			kCreationFailedError = -2,
			kUnsupportedComponentError = -3,
			kUnsupportedFunctionError = -4,
			kNullPointerError = -5,
			kBadParameterError = -6,
			kBadSampleRateError = -7,
			kBadDataLengthError = -8,
			kBadNumberChannelsError = -9,
			kFileError = -10,
			kStreamParameterNotSetError = -11,
			kNotEnabledError = -12,
			kBadStreamParameterWarning = -13
		}

		private enum AECMobileRoutingMode
		{
			kQuietEarpieceOrHeadset = 0,
			kEarpiece = 1,
			kLoudEarpiece = 2,
			kSpeakerphone = 3,
			kLoudSpeakerphone = 4
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct ConfigParam
		{
			public const int AEC_DELAY_AGNOSTIC = 12;

			public const int AEC_EXTENDED_FILTER = 13;

			public const int AGC_EXPERIMENTAL = 53;

			public const int AGC_EXPERIMENTAL_STARTUP_MIN_VOLUME = 54;

			public const int AGC_EXPERIMENTAL_CLIP_LEVEL_MIN = 55;
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct Param
		{
			public const int REVERSE_STREAM_DELAY_MS = 1;

			public const int AEC = 10;

			public const int AEC_SUPPRESSION_LEVEL = 11;

			public const int AECM = 20;

			public const int AECM_ROUTING_MODE = 21;

			public const int AECM_COMFORT_NOISE = 22;

			public const int HIGH_PASS_FILTER = 31;

			public const int NS = 41;

			public const int NS_LEVEL = 42;

			public const int AGC = 51;

			public const int AGC_MODE = 52;

			public const int AGC_COMPRESSION_GAIN = 56;

			public const int AGC_LIMITER = 57;

			public const int VAD = 61;

			public const int VAD_FRAME_SIZE_MS = 62;

			public const int VAD_LIKEHOOD = 63;
		}

		private const string lib_name = "webrtc-audio";

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr webrtc_audio_processor_create(int samplingRate, int channels, int frameSize, int revSamplingRate, int revChannels);

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int webrtc_audio_processor_set_config_param(IntPtr proc, int param, int v);

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int webrtc_audio_processor_init(IntPtr proc);

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int webrtc_audio_processor_set_param(IntPtr proc, int param, int v);

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int webrtc_audio_processor_process(IntPtr proc, short[] buffer, int offset, out bool voiceDetected);

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int webrtc_audio_processor_process_reverse(IntPtr proc, short[] buffer, int bufferSize);

		[DllImport("webrtc-audio", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void webrtc_audio_processor_destroy(IntPtr proc);
	}
}
