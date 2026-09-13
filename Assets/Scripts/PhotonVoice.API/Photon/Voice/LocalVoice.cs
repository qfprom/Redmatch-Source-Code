using System;
using System.Collections.Generic;

namespace Photon.Voice
{
	public class LocalVoice : IDisposable
	{
		public const int DATA_POOL_CAPACITY = 50;

		private bool debugEchoMode;

		internal VoiceInfo info;

		protected IEncoder encoder;

		internal byte id;

		internal int channelId;

		internal byte evNumber;

		protected VoiceClient voiceClient;

		protected volatile bool disposed;

		protected object disposeLock = new object();

		private int noTransmitCnt;

		internal Dictionary<byte, int> eventTimestamps = new Dictionary<byte, int>();

		[Obsolete("Use InterestGroup.")]
		public byte Group
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

		public byte InterestGroup { get; set; }

		public VoiceInfo Info
		{
			get
			{
				return info;
			}
		}

		public bool TransmitEnabled { get; set; }

		public bool IsCurrentlyTransmitting { get; protected set; }

		public int FramesSent { get; private set; }

		public int FramesSentBytes { get; private set; }

		public bool Reliable { get; set; }

		public bool Encrypt { get; set; }

		public IServiceable LocalUserServiceable { get; set; }

		public bool DebugEchoMode
		{
			get
			{
				return debugEchoMode;
			}
			set
			{
				if (debugEchoMode != value)
				{
					debugEchoMode = value;
					if (voiceClient != null && voiceClient.transport != null)
					{
						voiceClient.transport.SetDebugEchoMode(this);
					}
				}
			}
		}

		internal ILogger Logger
		{
			get
			{
				return voiceClient.transport;
			}
		}

		internal string Name
		{
			get
			{
				return "Local v#" + id + " ch#" + voiceClient.channelStr(channelId);
			}
		}

		internal string LogPrefix
		{
			get
			{
				return "[PV] " + Name;
			}
		}

		internal LocalVoice()
		{
		}

		internal LocalVoice(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, int channelId)
		{
			TransmitEnabled = true;
			info = voiceInfo;
			this.channelId = channelId;
			this.voiceClient = voiceClient;
			this.id = id;
			this.encoder = ((encoder != null) ? encoder : CreateDefaultEncoder(voiceInfo));
		}

		public virtual IEncoder CreateDefaultEncoder(VoiceInfo info)
		{
			throw new UnsupportedCodecException("LocalVoice.CreateDefaultEncoder", info.Codec, Logger);
		}

		protected void resetNoTransmitCnt()
		{
			noTransmitCnt = 10;
		}

		internal virtual void service()
		{
			if (voiceClient.transport.IsChannelJoined(channelId) && TransmitEnabled && encoder is IEncoderQueued)
			{
				foreach (ArraySegment<byte> item in ((IEncoderQueued)encoder).GetOutput())
				{
					sendFrame(item);
				}
			}
			if (noTransmitCnt == 0)
			{
				IsCurrentlyTransmitting = false;
			}
			else
			{
				IsCurrentlyTransmitting = true;
				noTransmitCnt--;
			}
			if (LocalUserServiceable != null)
			{
				LocalUserServiceable.Service(this);
			}
		}

		internal void sendFrame(ArraySegment<byte> compressed)
		{
			FramesSent++;
			FramesSentBytes += compressed.Count;
			voiceClient.transport.SendFrame(compressed, evNumber, id, channelId, this);
			eventTimestamps[evNumber] = Environment.TickCount;
			evNumber++;
			resetNoTransmitCnt();
		}

		public void RemoveSelf()
		{
			voiceClient.RemoveLocalVoice(this);
		}

		public virtual void Dispose()
		{
			if (!disposed)
			{
				if (encoder != null)
				{
					encoder.Dispose();
				}
				disposed = true;
			}
		}
	}
}
