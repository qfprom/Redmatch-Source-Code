using System;
using POpusCodec;
using POpusCodec.Enums;

namespace Photon.Voice
{
	public class OpusCodec
	{
		public enum FrameDuration
		{
			Frame2dot5ms = 2500,
			Frame5ms = 5000,
			Frame10ms = 10000,
			Frame20ms = 20000,
			Frame40ms = 40000,
			Frame60ms = 60000
		}

		public static class EncoderFactory
		{
			public static IEncoder Create<T>(VoiceInfo i, ILogger logger)
			{
				T[] array = new T[1];
				if (array[0].GetType() == typeof(float))
				{
					return new EncoderFloat(i, logger);
				}
				if (array[0].GetType() == typeof(short))
				{
					return new EncoderShort(i, logger);
				}
				throw new UnsupportedCodecException(string.Concat("EncoderFactory.Create<", array[0].GetType(), ">"), i.Codec, logger);
			}
		}

		public abstract class Encoder<T> : IEncoderDataFlowDirect<T>, IEncoderDataFlow<T>, IEncoder, IDisposable
		{
			protected OpusEncoder encoder;

			protected bool disposed;

			public string Error { get; private set; }

			protected Encoder(VoiceInfo i, ILogger logger)
			{
				try
				{
					encoder = new OpusEncoder((SamplingRate)i.SamplingRate, (Channels)i.Channels, i.Bitrate, OpusApplicationType.Voip, (Delay)(i.FrameDurationUs * 2 / 1000));
				}
				catch (Exception ex)
				{
					Error = ex.ToString();
					if (Error == null)
					{
						Error = "Exception in OpusCodec.Encoder constructor";
					}
					logger.LogError("[PV] OpusCodec.Encoder: " + Error);
				}
			}

			public void Dispose()
			{
				lock (this)
				{
					if (encoder != null)
					{
						encoder.Dispose();
					}
					disposed = true;
				}
			}

			public abstract ArraySegment<byte> EncodeAndGetOutput(T[] buf);
		}

		public class EncoderFloat : Encoder<float>
		{
			private static readonly ArraySegment<byte> EmptyBuffer = new ArraySegment<byte>(new byte[0]);

			internal EncoderFloat(VoiceInfo i, ILogger logger)
				: base(i, logger)
			{
			}

			public override ArraySegment<byte> EncodeAndGetOutput(float[] buf)
			{
				lock (this)
				{
					if (disposed || base.Error != null)
					{
						return EmptyBuffer;
					}
					return encoder.Encode(buf);
				}
			}
		}

		public class EncoderShort : Encoder<short>
		{
			private static readonly ArraySegment<byte> EmptyBuffer = new ArraySegment<byte>(new byte[0]);

			internal EncoderShort(VoiceInfo i, ILogger logger)
				: base(i, logger)
			{
			}

			public override ArraySegment<byte> EncodeAndGetOutput(short[] buf)
			{
				lock (this)
				{
					if (disposed || base.Error != null)
					{
						return EmptyBuffer;
					}
					return encoder.Encode(buf);
				}
			}
		}

		public class Decoder : IDecoderDirect, IDecoder, IDisposable
		{
			private OpusDecoder decoder;

			private ILogger logger;

			private static readonly float[] EmptyBufferFloat = new float[0];

			private static readonly short[] EmptyBufferShort = new short[0];

			public string Error { get; private set; }

			public Decoder(ILogger logger)
			{
				this.logger = logger;
			}

			public void Open(VoiceInfo i)
			{
				try
				{
					decoder = new OpusDecoder((SamplingRate)i.SamplingRate, (Channels)i.Channels);
				}
				catch (Exception ex)
				{
					Error = ex.ToString();
					if (Error == null)
					{
						Error = "Exception in OpusCodec.Decoder constructor";
					}
					logger.LogError("[PV] OpusCodec.Decoder: " + Error);
				}
			}

			public byte[] DecodeToByte(byte[] buf)
			{
				throw new NotImplementedException();
			}

			public float[] DecodeToFloat(byte[] buf)
			{
				return (Error != null) ? EmptyBufferFloat : decoder.DecodePacketFloat(buf);
			}

			public short[] DecodeToShort(byte[] buf)
			{
				return (Error != null) ? EmptyBufferShort : decoder.DecodePacketShort(buf);
			}

			public void Dispose()
			{
				if (decoder != null)
				{
					decoder.Dispose();
				}
			}
		}

		public class Util
		{
			internal static int bestEncoderSampleRate(int f)
			{
				int num = int.MaxValue;
				int result = 48000;
				foreach (object value in Enum.GetValues(typeof(SamplingRate)))
				{
					int num2 = Math.Abs((int)value - f);
					if (num2 < num)
					{
						num = num2;
						result = (int)value;
					}
				}
				return result;
			}
		}
	}
}
