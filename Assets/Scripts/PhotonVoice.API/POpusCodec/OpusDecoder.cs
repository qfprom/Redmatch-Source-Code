using System;
using System.Runtime.InteropServices;
using POpusCodec.Enums;

namespace POpusCodec
{
	public class OpusDecoder : IDisposable
	{
		private IntPtr _handle = IntPtr.Zero;

		private string _version = string.Empty;

		private const int MaxFrameSize = 5760;

		private bool _previousPacketInvalid = true;

		private int _channelCount;

		private static readonly float[] EmptyBufferFloat = new float[0];

		private static readonly short[] EmptyBufferShort = new short[0];

		private Bandwidth? _previousPacketBandwidth;

		private float[] bufferFloat;

		private short[] bufferShort;

		public string Version
		{
			get
			{
				return _version;
			}
		}

		public Bandwidth? PreviousPacketBandwidth
		{
			get
			{
				return _previousPacketBandwidth;
			}
		}

		public OpusDecoder(SamplingRate outputSamplingRateHz, Channels numChannels)
		{
			if (outputSamplingRateHz != SamplingRate.Sampling08000 && outputSamplingRateHz != SamplingRate.Sampling12000 && outputSamplingRateHz != SamplingRate.Sampling16000 && outputSamplingRateHz != SamplingRate.Sampling24000 && outputSamplingRateHz != SamplingRate.Sampling48000)
			{
				throw new ArgumentOutOfRangeException("outputSamplingRateHz", string.Concat("Must use one of the pre-defined sampling rates (", outputSamplingRateHz, ")"));
			}
			if (numChannels != Channels.Mono && numChannels != Channels.Stereo)
			{
				throw new ArgumentOutOfRangeException("numChannels", "Must be Mono or Stereo");
			}
			_channelCount = (int)numChannels;
			_handle = Wrapper.opus_decoder_create(outputSamplingRateHz, numChannels);
			_version = Marshal.PtrToStringAnsi(Wrapper.opus_get_version_string());
			if (_handle == IntPtr.Zero)
			{
				throw new OpusException(OpusStatusCode.AllocFail, "Memory was not allocated for the encoder");
			}
		}

		public float[] DecodePacketFloat(byte[] packetData)
		{
			if (bufferFloat == null && packetData == null)
			{
				return EmptyBufferFloat;
			}
			int num = 0;
			float[] array = ((bufferFloat != null) ? bufferFloat : new float[5760 * _channelCount]);
			num = Wrapper.opus_decode(_handle, packetData, array, 0, _channelCount);
			if (packetData == null)
			{
				_previousPacketInvalid = false;
			}
			else
			{
				int num2 = Wrapper.opus_packet_get_bandwidth(packetData);
				_previousPacketInvalid = num2 == -4;
			}
			if (num == 0)
			{
				return EmptyBufferFloat;
			}
			if (bufferFloat == null)
			{
				bufferFloat = new float[num * _channelCount];
				Buffer.BlockCopy(array, 0, bufferFloat, 0, num * 4);
			}
			return bufferFloat;
		}

		public short[] DecodePacketShort(byte[] packetData)
		{
			if (bufferShort == null && packetData == null)
			{
				return EmptyBufferShort;
			}
			int num = 0;
			short[] array = ((bufferShort != null) ? bufferShort : new short[5760 * _channelCount]);
			num = Wrapper.opus_decode(_handle, packetData, array, 0, _channelCount);
			if (packetData == null)
			{
				_previousPacketInvalid = false;
			}
			else
			{
				int num2 = Wrapper.opus_packet_get_bandwidth(packetData);
				_previousPacketInvalid = num2 == -4;
			}
			if (num == 0)
			{
				return EmptyBufferShort;
			}
			if (bufferShort == null)
			{
				bufferShort = new short[num * _channelCount];
				Buffer.BlockCopy(array, 0, bufferShort, 0, num * 2);
			}
			return bufferShort;
		}

		public void Dispose()
		{
			if (_handle != IntPtr.Zero)
			{
				Wrapper.opus_decoder_destroy(_handle);
				_handle = IntPtr.Zero;
			}
		}
	}
}
