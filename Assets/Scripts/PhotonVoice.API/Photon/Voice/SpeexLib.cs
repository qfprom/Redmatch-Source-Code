using System;
using System.Runtime.InteropServices;

namespace Photon.Voice
{
	public class SpeexLib
	{
		private const string lib_name = "libspeexdsp";

		public const int SPEEX_PREPROCESS_SET_DENOISE = 0;

		public const int SPEEX_PREPROCESS_GET_DENOISE = 1;

		public const int SPEEX_PREPROCESS_SET_AGC = 2;

		public const int SPEEX_PREPROCESS_GET_AGC = 3;

		public const int SPEEX_PREPROCESS_SET_VAD = 4;

		public const int SPEEX_PREPROCESS_GET_VAD = 5;

		public const int SPEEX_PREPROCESS_SET_AGC_LEVEL = 6;

		public const int SPEEX_PREPROCESS_GET_AGC_LEVEL = 7;

		public const int SPEEX_PREPROCESS_SET_DEREVERB = 8;

		public const int SPEEX_PREPROCESS_GET_DEREVERB = 9;

		public const int SPEEX_PREPROCESS_SET_DEREVERB_LEVEL = 10;

		public const int SPEEX_PREPROCESS_GET_DEREVERB_LEVEL = 11;

		public const int SPEEX_PREPROCESS_SET_DEREVERB_DECAY = 12;

		public const int SPEEX_PREPROCESS_GET_DEREVERB_DECAY = 13;

		public const int SPEEX_PREPROCESS_SET_PROB_START = 14;

		public const int SPEEX_PREPROCESS_GET_PROB_START = 15;

		public const int SPEEX_PREPROCESS_SET_PROB_CONTINUE = 16;

		public const int SPEEX_PREPROCESS_GET_PROB_CONTINUE = 17;

		public const int SPEEX_PREPROCESS_SET_NOISE_SUPPRESS = 18;

		public const int SPEEX_PREPROCESS_GET_NOISE_SUPPRESS = 19;

		public const int SPEEX_PREPROCESS_SET_ECHO_SUPPRESS = 20;

		public const int SPEEX_PREPROCESS_GET_ECHO_SUPPRESS = 21;

		public const int SPEEX_PREPROCESS_SET_ECHO_SUPPRESS_ACTIVE = 22;

		public const int SPEEX_PREPROCESS_GET_ECHO_SUPPRESS_ACTIVE = 23;

		public const int SPEEX_PREPROCESS_SET_ECHO_STATE = 24;

		public const int SPEEX_PREPROCESS_GET_ECHO_STATE = 25;

		public const int SPEEX_PREPROCESS_SET_AGC_INCREMENT = 26;

		public const int SPEEX_PREPROCESS_GET_AGC_INCREMENT = 27;

		public const int SPEEX_PREPROCESS_SET_AGC_DECREMENT = 28;

		public const int SPEEX_PREPROCESS_GET_AGC_DECREMENT = 29;

		public const int SPEEX_PREPROCESS_SET_AGC_MAX_GAIN = 30;

		public const int SPEEX_PREPROCESS_GET_AGC_MAX_GAIN = 31;

		public const int SPEEX_PREPROCESS_GET_AGC_LOUDNESS = 33;

		public const int SPEEX_PREPROCESS_GET_AGC_GAIN = 35;

		public const int SPEEX_PREPROCESS_GET_PSD_SIZE = 37;

		public const int SPEEX_PREPROCESS_GET_PSD = 39;

		public const int SPEEX_PREPROCESS_GET_NOISE_PSD_SIZE = 41;

		public const int SPEEX_PREPROCESS_GET_NOISE_PSD = 43;

		public const int SPEEX_PREPROCESS_GET_PROB = 45;

		public const int SPEEX_PREPROCESS_SET_AGC_TARGET = 46;

		public const int SPEEX_PREPROCESS_GET_AGC_TARGET = 47;

		public const int SPEEX_ECHO_GET_FRAME_SIZE = 3;

		public const int SPEEX_ECHO_SET_SAMPLING_RATE = 24;

		public const int SPEEX_ECHO_GET_SAMPLING_RATE = 25;

		public const int SPEEX_ECHO_GET_IMPULSE_RESPONSE_SIZE = 27;

		public const int SPEEX_ECHO_GET_IMPULSE_RESPONSE = 29;

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr speex_preprocess_state_init(int frame_size, int sampling_rate);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void speex_preprocess_state_destroy(IntPtr st);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int speex_preprocess_run(IntPtr st, short[] x);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int speex_preprocess_ctl(IntPtr st, int request, IntPtr ptr);

		public static int speex_preprocess_ctl(IntPtr st, int request, ref int value)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(4);
			int[] array = new int[1] { value };
			Marshal.Copy(array, 0, intPtr, 1);
			int result = speex_preprocess_ctl(st, request, intPtr);
			Marshal.Copy(intPtr, array, 0, 1);
			value = array[0];
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		public static int speex_preprocess_ctl(IntPtr st, int request, ref float value)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(4);
			float[] array = new float[1] { value };
			Marshal.Copy(array, 0, intPtr, 1);
			int result = speex_preprocess_ctl(st, request, intPtr);
			Marshal.Copy(intPtr, array, 0, 1);
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr speex_echo_state_init(int frame_size, int filter_length);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr speex_echo_state_init_mc(int frame_size, int filter_length, int nb_mic, int nb_speakers);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void speex_echo_state_destroy(IntPtr st);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void speex_echo_cancellation(IntPtr st, short[] rec, short[] play, short[] outBuf);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void speex_echo_capture(IntPtr st, short[] rec, short[] outBuf);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void speex_echo_playback(IntPtr st, short[] play);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void speex_echo_state_reset(IntPtr st);

		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int speex_echo_ctl(IntPtr st, int request, IntPtr ptr);

		public static int speex_echo_ctl(IntPtr st, int request, ref int value)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(4);
			int[] array = new int[1] { value };
			Marshal.Copy(array, 0, intPtr, 1);
			int result = speex_echo_ctl(st, request, intPtr);
			Marshal.Copy(intPtr, array, 0, 1);
			value = array[0];
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}

		public static int speex_echo_ctl(IntPtr st, int request, ref float value)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(4);
			float[] array = new float[1] { value };
			Marshal.Copy(array, 0, intPtr, 1);
			int result = speex_echo_ctl(st, request, intPtr);
			Marshal.Copy(intPtr, array, 0, 1);
			value = array[0];
			Marshal.FreeCoTaskMem(intPtr);
			return result;
		}
	}
}
