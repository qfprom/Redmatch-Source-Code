using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Voice
{
	public class VoiceClient : IDisposable
	{
		public delegate void RemoteVoiceInfoDelegate(int channelId, int playerId, byte voiceId, VoiceInfo voiceInfo, ref RemoteVoiceOptions options);

		internal IVoiceTransport transport;

		private int prevRtt;

		public const int ChannelAuto = -1;

		private byte globalInterestGroup;

		private byte voiceIdCnt;

		private Dictionary<byte, LocalVoice> localVoices = new Dictionary<byte, LocalVoice>();

		private Dictionary<int, List<LocalVoice>> localVoicesPerChannel = new Dictionary<int, List<LocalVoice>>();

		private Dictionary<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> remoteVoices = new Dictionary<int, Dictionary<int, Dictionary<byte, RemoteVoice>>>();

		private Random rnd = new Random();

		public int FramesLost { get; internal set; }

		public int FramesReceived { get; private set; }

		public int FramesSent
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					num += localVoice.Value.FramesSent;
				}
				return num;
			}
		}

		public int FramesSentBytes
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					num += localVoice.Value.FramesSentBytes;
				}
				return num;
			}
		}

		public int RoundTripTime { get; private set; }

		public int RoundTripTimeVariance { get; private set; }

		public bool SuppressInfoDuplicateWarning { get; set; }

		public RemoteVoiceInfoDelegate OnRemoteVoiceInfoAction { get; set; }

		public int DebugLostPercent { get; set; }

		public IEnumerable<LocalVoice> LocalVoices
		{
			get
			{
				LocalVoice[] array = new LocalVoice[localVoices.Count];
				localVoices.Values.CopyTo(array, 0);
				return array;
			}
		}

		public IEnumerable<RemoteVoiceInfo> RemoteVoiceInfos
		{
			get
			{
				foreach (KeyValuePair<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> channelVoices in remoteVoices)
				{
					foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> playerVoices in channelVoices.Value)
					{
						foreach (KeyValuePair<byte, RemoteVoice> voice in playerVoices.Value)
						{
							yield return new RemoteVoiceInfo(channelVoices.Key, playerVoices.Key, voice.Key, voice.Value.Info);
						}
					}
				}
			}
		}

		internal byte GlobalInterestGroup
		{
			get
			{
				return globalInterestGroup;
			}
			set
			{
				globalInterestGroup = value;
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					localVoice.Value.InterestGroup = globalInterestGroup;
				}
			}
		}

		internal VoiceClient(IVoiceTransport transport)
		{
			this.transport = transport;
		}

		public IEnumerable<LocalVoice> LocalVoicesInChannel(int channelId)
		{
			List<LocalVoice> value;
			if (localVoicesPerChannel.TryGetValue(channelId, out value))
			{
				LocalVoice[] array = new LocalVoice[value.Count];
				value.CopyTo(array, 0);
				return array;
			}
			return new LocalVoice[0];
		}

		public void Service()
		{
			foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
			{
				localVoice.Value.service();
			}
		}

		private LocalVoice createLocalVoice(VoiceInfo voiceInfo, int channelId, Func<byte, int, LocalVoice> voiceFactory)
		{
			if (channelId == -1)
			{
				channelId = transport.AssignChannel(voiceInfo);
			}
			byte newVoiceId = getNewVoiceId();
			if (newVoiceId != 0)
			{
				LocalVoice localVoice = voiceFactory(newVoiceId, channelId);
				if (localVoice != null)
				{
					addVoice(newVoiceId, channelId, localVoice);
					transport.LogInfo(localVoice.LogPrefix + " added enc: " + localVoice.info.ToString());
					return localVoice;
				}
			}
			return null;
		}

		public LocalVoice CreateLocalVoice(VoiceInfo voiceInfo, int channelId = -1, IEncoder encoder = null)
		{
			return createLocalVoice(voiceInfo, channelId, (byte vId, int chId) => new LocalVoice(this, encoder, vId, voiceInfo, chId));
		}

		public LocalVoiceFramed<T> CreateLocalVoiceFramed<T>(VoiceInfo voiceInfo, int frameSize, int channelId = -1, IEncoderDataFlow<T> encoder = null)
		{
			return (LocalVoiceFramed<T>)createLocalVoice(voiceInfo, channelId, (byte vId, int chId) => new LocalVoiceFramed<T>(this, encoder, vId, voiceInfo, chId, frameSize));
		}

		public LocalVoiceAudio<T> CreateLocalVoiceAudio<T>(VoiceInfo voiceInfo, int channelId = -1, IEncoder encoder = null)
		{
			return (LocalVoiceAudio<T>)createLocalVoice(voiceInfo, channelId, (byte vId, int chId) => LocalVoiceAudio<T>.Create(this, vId, encoder, voiceInfo, chId));
		}

		public LocalVoice CreateLocalVoiceAudioFromSource(VoiceInfo voiceInfo, IAudioDesc source, bool forceShort = false, int channelId = -1, IEncoder encoder = null)
		{
			if (source is IAudioPusher<float>)
			{
				if (forceShort)
				{
					LocalVoiceAudio<short> localVoice = CreateLocalVoiceAudio<short>(voiceInfo, channelId, encoder);
					FactoryReusableArray<float> bufferFactory = new FactoryReusableArray<float>(0);
					((IAudioPusher<float>)source).SetCallback(delegate(float[] buf)
					{
						short[] array = localVoice.BufferFactory.New(buf.Length);
						AudioUtil.Convert(buf, array, buf.Length);
						localVoice.PushDataAsync(array);
					}, bufferFactory);
					return localVoice;
				}
				LocalVoiceAudio<float> localVoice2 = CreateLocalVoiceAudio<float>(voiceInfo, channelId, encoder);
				((IAudioPusher<float>)source).SetCallback(delegate(float[] buf)
				{
					localVoice2.PushDataAsync(buf);
				}, localVoice2.BufferFactory);
				return localVoice2;
			}
			if (source is IAudioPusher<short>)
			{
				LocalVoiceAudio<short> localVoice3 = CreateLocalVoiceAudio<short>(voiceInfo, channelId, encoder);
				((IAudioPusher<short>)source).SetCallback(delegate(short[] buf)
				{
					localVoice3.PushDataAsync(buf);
				}, localVoice3.BufferFactory);
				return localVoice3;
			}
			if (source is IAudioReader<float>)
			{
				if (forceShort)
				{
					transport.LogInfo("[PV] Creating local voice with source samples type conversion from float to short.");
					LocalVoiceAudio<short> localVoiceAudio = CreateLocalVoiceAudio<short>(voiceInfo, channelId, encoder);
					localVoiceAudio.LocalUserServiceable = new BufferReaderPushAdapterAsyncPoolFloatToShort(localVoiceAudio, source as IAudioReader<float>);
					return localVoiceAudio;
				}
				LocalVoiceAudio<float> localVoiceAudio2 = CreateLocalVoiceAudio<float>(voiceInfo, channelId, encoder);
				localVoiceAudio2.LocalUserServiceable = new BufferReaderPushAdapterAsyncPool<float>(localVoiceAudio2, source as IAudioReader<float>);
				return localVoiceAudio2;
			}
			if (source is IAudioReader<short>)
			{
				LocalVoiceAudio<short> localVoiceAudio3 = CreateLocalVoiceAudio<short>(voiceInfo, channelId, encoder);
				localVoiceAudio3.LocalUserServiceable = new BufferReaderPushAdapterAsyncPool<short>(localVoiceAudio3, source as IAudioReader<short>);
				return localVoiceAudio3;
			}
			transport.LogError("[PV] CreateLocalVoiceAudioFromSource does not support Voice.IAudioDesc of type {0}", source.GetType());
			return LocalVoiceAudioDummy.Dummy;
		}

		private byte getNewVoiceId()
		{
			byte result = 0;
			if (voiceIdCnt == byte.MaxValue)
			{
				bool[] array = new bool[256];
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					array[localVoice.Value.id] = true;
				}
				for (byte b = 1; b != 0; b++)
				{
					if (!array[b])
					{
						result = b;
						break;
					}
				}
			}
			else
			{
				voiceIdCnt++;
				result = voiceIdCnt;
			}
			return result;
		}

		private void addVoice(byte newId, int channelId, LocalVoice v)
		{
			localVoices[newId] = v;
			List<LocalVoice> value;
			if (!localVoicesPerChannel.TryGetValue(channelId, out value))
			{
				value = new List<LocalVoice>();
				localVoicesPerChannel[channelId] = value;
			}
			value.Add(v);
			if (transport.IsChannelJoined(channelId))
			{
				transport.SendVoicesInfo(new List<LocalVoice> { v }, channelId, 0);
			}
			v.InterestGroup = GlobalInterestGroup;
		}

		public void RemoveLocalVoice(LocalVoice voice)
		{
			localVoices.Remove(voice.id);
			localVoicesPerChannel[voice.channelId].Remove(voice);
			if (transport.IsChannelJoined(voice.channelId))
			{
				transport.SendVoiceRemove(voice, voice.channelId, 0);
			}
			voice.Dispose();
			transport.LogInfo(voice.LogPrefix + " removed");
		}

		internal void sendVoicesInfo(int targetPlayerId)
		{
			foreach (int key in localVoicesPerChannel.Keys)
			{
				sendChannelVoicesInfo(key, targetPlayerId);
			}
		}

		internal void sendChannelVoicesInfo(int channelId, int targetPlayerId)
		{
			List<LocalVoice> value;
			if (transport.IsChannelJoined(channelId) && localVoicesPerChannel.TryGetValue(channelId, out value))
			{
				transport.SendVoicesInfo(value, channelId, targetPlayerId);
			}
		}

		internal void onVoiceEvent(object content0, int channelId, int playerId, int localPlayerId)
		{
			object[] array = (object[])content0;
			if ((byte)array[0] == 0)
			{
				switch ((byte)array[1])
				{
				case 1:
					onVoiceInfo(channelId, playerId, array[2]);
					break;
				case 2:
					onVoiceRemove(channelId, playerId, array[2]);
					break;
				default:
					transport.LogError("[PV] Unknown sevent subcode " + array[1]);
					break;
				}
				return;
			}
			byte b = (byte)array[0];
			byte b2 = (byte)array[1];
			byte[] receivedBytes = (byte[])array[2];
			LocalVoice value;
			int value2;
			if (playerId == localPlayerId && localVoices.TryGetValue(b, out value) && value.eventTimestamps.TryGetValue(b2, out value2))
			{
				int num = Environment.TickCount - value2;
				int num2 = num - prevRtt;
				prevRtt = num;
				if (num2 < 0)
				{
					num2 = -num2;
				}
				RoundTripTimeVariance = (num2 + RoundTripTimeVariance * 19) / 20;
				RoundTripTime = (num + RoundTripTime * 19) / 20;
			}
			onFrame(channelId, playerId, b, b2, receivedBytes);
		}

		internal object[] buildVoicesInfo(IEnumerable<LocalVoice> voicesToSend, bool logInfo)
		{
			object[] array = new object[voicesToSend.Count()];
			object[] result = new object[3]
			{
				(byte)0,
				EventSubcode.VoiceInfo,
				array
			};
			int num = 0;
			foreach (LocalVoice item in voicesToSend)
			{
				array[num] = new Dictionary<byte, object>
				{
					{ 1, item.id },
					{
						12,
						item.info.Codec
					},
					{
						2,
						item.info.SamplingRate
					},
					{
						3,
						item.info.Channels
					},
					{
						4,
						item.info.FrameDurationUs
					},
					{
						5,
						item.info.Bitrate
					},
					{
						10,
						item.info.UserData
					},
					{ 11, item.evNumber }
				};
				num++;
				if (logInfo)
				{
					transport.LogInfo(item.LogPrefix + " Sending info: " + item.info.ToString() + " ev=" + item.evNumber);
				}
			}
			return result;
		}

		internal object[] buildVoiceRemoveMessage(LocalVoice v)
		{
			byte[] array = new byte[1] { v.id };
			object[] result = new object[3]
			{
				(byte)0,
				EventSubcode.VoiceRemove,
				array
			};
			transport.LogInfo(v.LogPrefix + " remove sent");
			return result;
		}

		internal void clearRemoteVoices()
		{
			foreach (KeyValuePair<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> remoteVoice in remoteVoices)
			{
				foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> item in remoteVoice.Value)
				{
					foreach (KeyValuePair<byte, RemoteVoice> item2 in item.Value)
					{
						item2.Value.removeAndDispose();
					}
				}
			}
			remoteVoices.Clear();
			transport.LogInfo("[PV] Remote voices cleared");
		}

		internal void clearRemoteVoicesInChannel(int channelId)
		{
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> item in value)
				{
					foreach (KeyValuePair<byte, RemoteVoice> item2 in item.Value)
					{
						item2.Value.removeAndDispose();
					}
				}
				remoteVoices.Remove(channelId);
			}
			transport.LogInfo("[PV] Remote voices for channel " + channelStr(channelId) + " cleared");
		}

		private void onVoiceInfo(int channelId, int playerId, object payload)
		{
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (!remoteVoices.TryGetValue(channelId, out value))
			{
				value = new Dictionary<int, Dictionary<byte, RemoteVoice>>();
				remoteVoices[channelId] = value;
			}
			Dictionary<byte, RemoteVoice> value2 = null;
			if (!value.TryGetValue(playerId, out value2))
			{
				value2 = (value[playerId] = new Dictionary<byte, RemoteVoice>());
			}
			object[] array = (object[])payload;
			foreach (object obj in array)
			{
				Dictionary<byte, object> dictionary2 = (Dictionary<byte, object>)obj;
				byte b = (byte)dictionary2[1];
				if (!value2.ContainsKey(b))
				{
					byte b2 = (byte)dictionary2[11];
					VoiceInfo voiceInfo = VoiceInfo.CreateFromEventPayload(dictionary2);
					transport.LogInfo("[PV] ch#" + channelStr(channelId) + " p#" + playerStr(playerId) + " v#" + b + " Info received: " + voiceInfo.ToString() + " ev=" + b2);
					RemoteVoiceOptions options = new RemoteVoiceOptions
					{
						Decoder = VoiceCodec.CreateDefaultDecoder(channelId, playerId, b, voiceInfo, transport)
					};
					if (OnRemoteVoiceInfoAction != null)
					{
						OnRemoteVoiceInfoAction(channelId, playerId, b, voiceInfo, ref options);
					}
					value2[b] = new RemoteVoice(this, options, channelId, playerId, b, voiceInfo, b2);
				}
				else if (!SuppressInfoDuplicateWarning)
				{
					transport.LogWarning("[PV] Info duplicate for voice #" + b + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId));
				}
			}
		}

		private void onVoiceRemove(int channelId, int playerId, object payload)
		{
			byte[] array = (byte[])payload;
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				Dictionary<byte, RemoteVoice> value2 = null;
				if (value.TryGetValue(playerId, out value2))
				{
					byte[] array2 = array;
					foreach (byte b in array2)
					{
						RemoteVoice value3;
						if (value2.TryGetValue(b, out value3))
						{
							value2.Remove(b);
							transport.LogInfo("[PV] Remote voice #" + b + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " removed");
							value3.removeAndDispose();
						}
						else
						{
							transport.LogWarning("[PV] Remote voice #" + b + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " not found when trying to remove");
						}
					}
				}
				else
				{
					transport.LogWarning("[PV] Remote voice list of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " not found when trying to remove voice(s)");
				}
			}
			else
			{
				transport.LogWarning("[PV] Remote voice list of channel " + channelStr(channelId) + " not found when trying to remove voice(s)");
			}
		}

		private void onFrame(int channelId, int playerId, byte voiceId, byte evNumber, byte[] receivedBytes)
		{
			if (DebugLostPercent > 0 && rnd.Next(100) < DebugLostPercent)
			{
				transport.LogWarning("[PV] Debug Lost Sim: 1 packet dropped");
				return;
			}
			FramesReceived++;
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				Dictionary<byte, RemoteVoice> value2 = null;
				if (value.TryGetValue(playerId, out value2))
				{
					RemoteVoice value3 = null;
					if (value2.TryGetValue(voiceId, out value3))
					{
						value3.receiveBytes(receivedBytes, evNumber);
						return;
					}
					transport.LogWarning("[PV] Frame event for not inited voice #" + voiceId + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId));
				}
				else
				{
					transport.LogWarning("[PV] Frame event for voice #" + voiceId + " of not inited player " + playerStr(playerId) + " at channel " + channelStr(channelId));
				}
			}
			else
			{
				transport.LogWarning("[PV] Frame event for voice #" + voiceId + " of not inited channel " + channelStr(channelId));
			}
		}

		internal bool removePlayerVoices(int playerId)
		{
			foreach (int key in remoteVoices.Keys)
			{
				removePlayerVoices(key, playerId);
			}
			return true;
		}

		internal bool removePlayerVoices(int channelId, int playerId)
		{
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				Dictionary<byte, RemoteVoice> value2 = null;
				if (value.TryGetValue(playerId, out value2))
				{
					value.Remove(playerId);
					foreach (KeyValuePair<byte, RemoteVoice> item in value2)
					{
						item.Value.removeAndDispose();
					}
					return true;
				}
				return false;
			}
			return false;
		}

		internal string channelStr(int channelId)
		{
			string text = transport.ChannelIdStr(channelId);
			if (text != null)
			{
				return channelId + "(" + text + ")";
			}
			return channelId.ToString();
		}

		internal string playerStr(int playerId)
		{
			string text = transport.PlayerIdStr(playerId);
			if (text != null)
			{
				return playerId + "(" + text + ")";
			}
			return playerId.ToString();
		}

		public void Dispose()
		{
			foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
			{
				localVoice.Value.Dispose();
			}
			foreach (KeyValuePair<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> remoteVoice in remoteVoices)
			{
				foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> item in remoteVoice.Value)
				{
					foreach (KeyValuePair<byte, RemoteVoice> item2 in item.Value)
					{
						item2.Value.Dispose();
					}
				}
			}
		}
	}
}
