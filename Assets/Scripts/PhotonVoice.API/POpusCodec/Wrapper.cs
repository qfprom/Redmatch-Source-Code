using System;
using System.Runtime.InteropServices;
using POpusCodec.Enums;

namespace POpusCodec
{
	internal class Wrapper
	{
		private const string lib_name = "opus_egpv";

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_encoder_get_size(Channels channels);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern OpusStatusCode opus_encoder_init(IntPtr st, SamplingRate Fs, Channels channels, OpusApplicationType application);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr opus_get_version_string();

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_encode(IntPtr st, short[] pcm, int frame_size, byte[] data, int max_data_bytes);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_encode_float(IntPtr st, float[] pcm, int frame_size, byte[] data, int max_data_bytes);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_encoder_ctl_set(IntPtr st, OpusCtlSetRequest request, int value);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_encoder_ctl_get(IntPtr st, OpusCtlGetRequest request, ref int value);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_decoder_ctl_set(IntPtr st, OpusCtlSetRequest request, int value);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_decoder_ctl_get(IntPtr st, OpusCtlGetRequest request, ref int value);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_decoder_get_size(Channels channels);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern OpusStatusCode opus_decoder_init(IntPtr st, SamplingRate Fs, Channels channels);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_decode(IntPtr st, byte[] data, int len, short[] pcm, int frame_size, int decode_fec);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern int opus_decode_float(IntPtr st, byte[] data, int len, float[] pcm, int frame_size, int decode_fec);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int opus_packet_get_bandwidth(byte[] data);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int opus_packet_get_nb_channels(byte[] data);

		[DllImport("opus_egpv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern IntPtr opus_strerror(OpusStatusCode error);

		public static IntPtr opus_encoder_create(SamplingRate Fs, Channels channels, OpusApplicationType application)
		{
			int cb = opus_encoder_get_size(channels);
			IntPtr intPtr = Marshal.AllocHGlobal(cb);
			OpusStatusCode statusCode = opus_encoder_init(intPtr, Fs, channels, application);
			try
			{
				HandleStatusCode(statusCode);
				return intPtr;
			}
			catch (Exception ex)
			{
				if (intPtr != IntPtr.Zero)
				{
					opus_encoder_destroy(intPtr);
					intPtr = IntPtr.Zero;
				}
				throw ex;
			}
		}

		public static int opus_encode(IntPtr st, short[] pcm, int frame_size, byte[] data)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusEncoder");
			}
			int num = opus_encode(st, pcm, frame_size, data, data.Length);
			if (num <= 0)
			{
				HandleStatusCode((OpusStatusCode)num);
			}
			return num;
		}

		public static int opus_encode(IntPtr st, float[] pcm, int frame_size, byte[] data)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusEncoder");
			}
			int num = opus_encode_float(st, pcm, frame_size, data, data.Length);
			if (num <= 0)
			{
				HandleStatusCode((OpusStatusCode)num);
			}
			return num;
		}

		public static void opus_encoder_destroy(IntPtr st)
		{
			Marshal.FreeHGlobal(st);
		}

		public static int get_opus_encoder_ctl(IntPtr st, OpusCtlGetRequest request)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusEncoder");
			}
			int value = 0;
			OpusStatusCode statusCode = (OpusStatusCode)opus_encoder_ctl_get(st, request, ref value);
			HandleStatusCode(statusCode);
			return value;
		}

		public static void set_opus_encoder_ctl(IntPtr st, OpusCtlSetRequest request, int value)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusEncoder");
			}
			OpusStatusCode statusCode = (OpusStatusCode)opus_encoder_ctl_set(st, request, value);
			HandleStatusCode(statusCode);
		}

		public static int get_opus_decoder_ctl(IntPtr st, OpusCtlGetRequest request)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusDcoder");
			}
			int value = 0;
			OpusStatusCode statusCode = (OpusStatusCode)opus_decoder_ctl_get(st, request, ref value);
			HandleStatusCode(statusCode);
			return value;
		}

		public static void set_opus_decoder_ctl(IntPtr st, OpusCtlSetRequest request, int value)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusDecoder");
			}
			OpusStatusCode statusCode = (OpusStatusCode)opus_decoder_ctl_set(st, request, value);
			HandleStatusCode(statusCode);
		}

		public static IntPtr opus_decoder_create(SamplingRate Fs, Channels channels)
		{
			int cb = opus_decoder_get_size(channels);
			IntPtr intPtr = Marshal.AllocHGlobal(cb);
			OpusStatusCode statusCode = opus_decoder_init(intPtr, Fs, channels);
			try
			{
				HandleStatusCode(statusCode);
				return intPtr;
			}
			catch (Exception ex)
			{
				if (intPtr != IntPtr.Zero)
				{
					opus_decoder_destroy(intPtr);
					intPtr = IntPtr.Zero;
				}
				throw ex;
			}
		}

		public static void opus_decoder_destroy(IntPtr st)
		{
			Marshal.FreeHGlobal(st);
		}

		public static int opus_decode(IntPtr st, byte[] data, short[] pcm, int decode_fec, int channels)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusDecoder");
			}
			int num = 0;
			num = ((data == null) ? opus_decode(st, null, 0, pcm, pcm.Length / channels, decode_fec) : opus_decode(st, data, data.Length, pcm, pcm.Length / channels, decode_fec));
			if (num == -4)
			{
				return 0;
			}
			if (num <= 0)
			{
				HandleStatusCode((OpusStatusCode)num);
			}
			return num;
		}

		public static int opus_decode(IntPtr st, byte[] data, float[] pcm, int decode_fec, int channels)
		{
			if (st == IntPtr.Zero)
			{
				throw new ObjectDisposedException("OpusDecoder");
			}
			int num = 0;
			num = ((data == null) ? opus_decode_float(st, null, 0, pcm, pcm.Length / channels, decode_fec) : opus_decode_float(st, data, data.Length, pcm, pcm.Length / channels, decode_fec));
			if (num == -4)
			{
				return 0;
			}
			if (num <= 0)
			{
				HandleStatusCode((OpusStatusCode)num);
			}
			return num;
		}

		private static void HandleStatusCode(OpusStatusCode statusCode)
		{
			if (statusCode != OpusStatusCode.OK)
			{
				throw new OpusException(statusCode, Marshal.PtrToStringAnsi(opus_strerror(statusCode)));
			}
		}
	}
}
