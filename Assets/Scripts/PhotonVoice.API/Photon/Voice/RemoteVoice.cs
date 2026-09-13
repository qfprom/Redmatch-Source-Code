using System;
using System.Collections.Generic;
using System.Threading;

namespace Photon.Voice
{
	internal class RemoteVoice : IDisposable
	{
		internal RemoteVoiceOptions options;

		private int channelId;

		private int playerId;

		private byte voiceId;

		private volatile bool disposed;

		private object disposeLock = new object();

		internal byte lastEvNumber;

		private VoiceClient voiceClient;

		private Queue<byte[]> frameQueue = new Queue<byte[]>();

		private AutoResetEvent frameQueueReady = new AutoResetEvent(false);

		internal VoiceInfo Info { get; private set; }

		protected string Name
		{
			get
			{
				return "Remote v#" + voiceId + " ch#" + voiceClient.channelStr(channelId) + " p#" + playerId;
			}
		}

		protected string LogPrefix
		{
			get
			{
				return "[PV] " + Name;
			}
		}

		internal RemoteVoice(VoiceClient client, RemoteVoiceOptions options, int channelId, int playerId, byte voiceId, VoiceInfo info, byte lastEventNumber)
		{
			this.options = options;
			voiceClient = client;
			this.channelId = channelId;
			this.playerId = playerId;
			this.voiceId = voiceId;
			Info = info;
			lastEvNumber = lastEventNumber;
			if (this.options.Decoder == null)
			{
				voiceClient.transport.LogError(LogPrefix + ": decoder is null");
				disposed = true;
				return;
			}
			Thread thread = new Thread((ThreadStart)delegate
			{
				decodeThread(this.options.Decoder);
			});
			thread.Name = LogPrefix + " decode";
			thread.Start();
		}

		private static byte byteDiff(byte latest, byte last)
		{
			return (byte)(latest - (last + 1));
		}

		internal void receiveBytes(byte[] receivedBytes, byte evNumber)
		{
			if (evNumber != lastEvNumber)
			{
				int num = byteDiff(evNumber, lastEvNumber);
				if (num != 0)
				{
					voiceClient.transport.LogDebug(LogPrefix + " evNumer: " + evNumber + " playerVoice.lastEvNumber: " + lastEvNumber + " missing: " + num + " r/b " + receivedBytes.Length);
				}
				lastEvNumber = evNumber;
				receiveNullFrames(num);
				voiceClient.FramesLost += num;
			}
			receiveFrame(receivedBytes);
		}

		private void receiveFrame(byte[] frame)
		{
			lock (disposeLock)
			{
				if (!disposed)
				{
					lock (frameQueue)
					{
						frameQueue.Enqueue(frame);
					}
					frameQueueReady.Set();
				}
			}
		}

		private void receiveNullFrames(int count)
		{
			lock (disposeLock)
			{
				if (disposed)
				{
					return;
				}
				lock (frameQueue)
				{
					for (int i = 0; i < count; i++)
					{
						frameQueue.Enqueue(null);
					}
				}
				frameQueueReady.Set();
			}
		}

		private void decodeThread(IDecoder decoder)
		{
			voiceClient.transport.LogInfo(LogPrefix + ": Starting decode thread");
			try
			{
				decoder.Open(Info);
				while (!disposed)
				{
					frameQueueReady.WaitOne();
					while (!disposed)
					{
						byte[] frame = null;
						bool flag = false;
						lock (frameQueue)
						{
							if (frameQueue.Count > 0)
							{
								flag = true;
								frame = frameQueue.Dequeue();
							}
						}
						if (flag)
						{
							decodeFrame(decoder, frame);
							continue;
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				voiceClient.transport.LogError(LogPrefix + ": Exception in decode thread: " + ex);
				throw ex;
			}
			finally
			{
				lock (disposeLock)
				{
					disposed = true;
				}
				frameQueueReady.Close();
				lock (frameQueue)
				{
					frameQueue.Clear();
				}
				decoder.Dispose();
				voiceClient.transport.LogInfo(LogPrefix + ": Exiting decode thread");
			}
		}

		private void decodeFrame(IDecoder decoder, byte[] frame)
		{
			if (decoder is IDecoderDirect)
			{
				if (options.OnDecodedFrameByteAction != null)
				{
					byte[] obj = decodeFrameToByte(frame);
					options.OnDecodedFrameByteAction(obj);
				}
				if (options.OnDecodedFrameShortAction != null)
				{
					short[] obj2 = decodeFrameToShort(frame);
					options.OnDecodedFrameShortAction(obj2);
				}
				if (options.OnDecodedFrameFloatAction != null)
				{
					float[] obj3 = decodeFrameToFloat(frame);
					options.OnDecodedFrameFloatAction(obj3);
				}
			}
			else
			{
				((IDecoderQueued)decoder).Decode(frame);
			}
		}

		internal byte[] decodeFrameToByte(byte[] buffer)
		{
			byte[] array;
			if (buffer == null)
			{
				array = ((IDecoderDirect)options.Decoder).DecodeToByte(null);
				voiceClient.transport.LogDebug(LogPrefix + " lost packet decoded length: " + array.Length);
			}
			else
			{
				array = ((IDecoderDirect)options.Decoder).DecodeToByte(buffer);
			}
			return array;
		}

		internal short[] decodeFrameToShort(byte[] buffer)
		{
			short[] array;
			if (buffer == null)
			{
				array = ((IDecoderDirect)options.Decoder).DecodeToShort(null);
				voiceClient.transport.LogDebug(LogPrefix + " lost packet decoded length: " + array.Length);
			}
			else
			{
				array = ((IDecoderDirect)options.Decoder).DecodeToShort(buffer);
			}
			return array;
		}

		internal float[] decodeFrameToFloat(byte[] buffer)
		{
			float[] array;
			if (buffer == null)
			{
				array = ((IDecoderDirect)options.Decoder).DecodeToFloat(null);
				voiceClient.transport.LogDebug(LogPrefix + " lost packet decoded length: " + array.Length);
			}
			else
			{
				array = ((IDecoderDirect)options.Decoder).DecodeToFloat(buffer);
			}
			return array;
		}

		internal void removeAndDispose()
		{
			if (options.OnRemoteVoiceRemoveAction != null)
			{
				options.OnRemoteVoiceRemoveAction();
			}
			Dispose();
		}

		public void Dispose()
		{
			lock (disposeLock)
			{
				if (!disposed)
				{
					disposed = true;
					frameQueueReady.Set();
				}
			}
		}
	}
}
