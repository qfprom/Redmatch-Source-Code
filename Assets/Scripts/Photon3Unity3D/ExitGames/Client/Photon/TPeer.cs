using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
	internal class TPeer : PeerBase
	{
		internal const int TCP_HEADER_BYTES = 7;

		internal const int MSG_HEADER_BYTES = 2;

		public const int ALL_HEADER_BYTES = 9;

		private Queue<byte[]> incomingList = new Queue<byte[]>(32);

		internal List<StreamBuffer> outgoingStream;

		private int lastPingResult;

		private byte[] pingRequest = new byte[5] { 240, 0, 0, 0, 0 };

		internal static readonly byte[] tcpFramedMessageHead = new byte[9] { 251, 0, 0, 0, 0, 0, 0, 243, 2 };

		internal static readonly byte[] tcpMsgHead = new byte[2] { 243, 2 };

		internal byte[] messageHeader;

		protected internal bool DoFraming = true;

		internal override int QueuedIncomingCommandsCount
		{
			get
			{
				return incomingList.Count;
			}
		}

		internal override int QueuedOutgoingCommandsCount
		{
			get
			{
				return outgoingCommandsInStream;
			}
		}

		internal TPeer()
		{
			TrafficPackageHeaderSize = 0;
		}

		internal override void InitPeerBase()
		{
			base.InitPeerBase();
			incomingList = new Queue<byte[]>(32);
			timestampOfLastReceive = SupportClass.GetTickCount();
		}

		internal override bool Connect(string serverAddress, string appID, object customData = null)
		{
			if (peerConnectionState != ConnectionStateValue.Disconnected)
			{
				base.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
				return false;
			}
			if ((int)base.debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
			}
			base.ServerAddress = serverAddress;
			InitPeerBase();
			outgoingStream = new List<StreamBuffer>();
			if (usedTransportProtocol == ConnectionProtocol.WebSocket || usedTransportProtocol == ConnectionProtocol.WebSocketSecure)
			{
				serverAddress = PepareWebSocketUrl(serverAddress, appID, customData);
			}
			if (base.SocketImplementation != null)
			{
				PhotonSocket = (IPhotonSocket)Activator.CreateInstance(base.SocketImplementation, this);
			}
			else
			{
				PhotonSocket = new SocketTcp(this);
			}
			if (PhotonSocket == null)
			{
				base.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect(). SocketImplementation: " + base.SocketImplementation);
				return false;
			}
			messageHeader = (DoFraming ? tcpFramedMessageHead : tcpMsgHead);
			if (PhotonSocket.Connect())
			{
				peerConnectionState = ConnectionStateValue.Connecting;
				return true;
			}
			peerConnectionState = ConnectionStateValue.Disconnected;
			return false;
		}

		public override void OnConnect()
		{
			lastPingResult = SupportClass.GetTickCount();
			byte[] data = PrepareConnectData(base.ServerAddress, AppId, CustomInitData);
			EnqueueInit(data);
			SendOutgoingCommands();
		}

		internal override void Disconnect()
		{
			if (peerConnectionState != ConnectionStateValue.Disconnected && peerConnectionState != ConnectionStateValue.Disconnecting)
			{
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "TPeer.Disconnect()");
				}
				StopConnection();
			}
		}

		internal override void StopConnection()
		{
			peerConnectionState = ConnectionStateValue.Disconnecting;
			if (PhotonSocket != null)
			{
				PhotonSocket.Disconnect();
			}
			lock (incomingList)
			{
				incomingList.Clear();
			}
			peerConnectionState = ConnectionStateValue.Disconnected;
			EnqueueStatusCallback(StatusCode.Disconnect);
		}

		internal override void FetchServerTimestamp()
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)base.debugOut >= 3)
				{
					base.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
			}
			else
			{
				SendPing();
				serverTimeOffsetIsAvailable = false;
			}
		}

		private void EnqueueInit(byte[] data)
		{
			if (DoFraming)
			{
				StreamBuffer streamBuffer = new StreamBuffer(data.Length + 32);
				byte[] array = new byte[7] { 251, 0, 0, 0, 0, 0, 1 };
				int targetOffset = 1;
				Protocol.Serialize(data.Length + array.Length, array, ref targetOffset);
				streamBuffer.Write(array, 0, array.Length);
				streamBuffer.Write(data, 0, data.Length);
				if (base.TrafficStatsEnabled)
				{
					base.TrafficStatsOutgoing.TotalPacketCount++;
					base.TrafficStatsOutgoing.TotalCommandsInPackets++;
					base.TrafficStatsOutgoing.CountControlCommand(streamBuffer.Length);
				}
				EnqueueMessageAsPayload(DeliveryMode.Reliable, streamBuffer, 0);
			}
		}

		internal override bool DispatchIncomingCommands()
		{
			if (peerConnectionState == ConnectionStateValue.Connected && SupportClass.GetTickCount() - timestampOfLastReceive > base.DisconnectTimeout)
			{
				EnqueueStatusCallback(StatusCode.TimeoutDisconnect);
				EnqueueActionForDispatch(Disconnect);
			}
			while (true)
			{
				MyAction myAction;
				lock (ActionQueue)
				{
					if (ActionQueue.Count <= 0)
					{
						break;
					}
					myAction = ActionQueue.Dequeue();
					goto IL_008a;
				}
				IL_008a:
				myAction();
			}
			byte[] array;
			lock (incomingList)
			{
				if (incomingList.Count <= 0)
				{
					return false;
				}
				array = incomingList.Dequeue();
			}
			ByteCountCurrentDispatch = array.Length + 3;
			return DeserializeMessageAndCallback(new StreamBuffer(array));
		}

		internal override bool SendOutgoingCommands()
		{
			if (peerConnectionState == ConnectionStateValue.Disconnected)
			{
				return false;
			}
			if (!PhotonSocket.Connected)
			{
				return false;
			}
			timeInt = SupportClass.GetTickCount() - timeBase;
			timeLastSendOutgoing = timeInt;
			if (peerConnectionState == ConnectionStateValue.Connected && Math.Abs(SupportClass.GetTickCount() - lastPingResult) > base.timePingInterval)
			{
				SendPing();
			}
			lock (outgoingStream)
			{
				for (int i = 0; i < outgoingStream.Count; i++)
				{
					StreamBuffer streamBuffer = outgoingStream[i];
					SendData(streamBuffer.GetBuffer(), streamBuffer.Length);
					PeerBase.MessageBufferPoolPut(streamBuffer);
				}
				outgoingStream.Clear();
				outgoingCommandsInStream = 0;
			}
			return false;
		}

		internal override bool SendAcksOnly()
		{
			if (PhotonSocket == null || !PhotonSocket.Connected)
			{
				return false;
			}
			timeInt = SupportClass.GetTickCount() - timeBase;
			if (peerConnectionState == ConnectionStateValue.Connected && SupportClass.GetTickCount() - lastPingResult > base.timePingInterval)
			{
				SendPing();
			}
			return false;
		}

		internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, SendOptions sendParams, EgMessageType messageType)
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)base.debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: " + opCode + "! Not connected. PeerState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			if (sendParams.Channel >= base.ChannelCount)
			{
				if ((int)base.debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + sendParams.Channel + ")>= channelCount (" + base.ChannelCount + ").");
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			StreamBuffer opMessage = SerializeOperationToMessage(opCode, parameters, messageType, sendParams.Encrypt);
			return EnqueueMessageAsPayload(sendParams.DeliveryMode, opMessage, sendParams.Channel);
		}

		internal override bool EnqueueMessage(object msg, SendOptions sendOptions)
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)base.debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send message! Not connected. PeerState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			byte channel = sendOptions.Channel;
			if (channel >= base.ChannelCount)
			{
				if ((int)base.debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + channel + ")>= channelCount (" + base.ChannelCount + ").");
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			StreamBuffer opMessage = SerializeMessageToMessage(msg, sendOptions.Encrypt, messageHeader);
			return EnqueueMessageAsPayload(sendOptions.DeliveryMode, opMessage, channel);
		}

		internal override StreamBuffer SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
		{
			StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
			streamBuffer.SetLength(0L);
			if (!encrypt)
			{
				streamBuffer.Write(messageHeader, 0, messageHeader.Length);
			}
			SerializationProtocol.SerializeOperationRequest(streamBuffer, opCode, parameters, false);
			if (encrypt)
			{
				byte[] array = CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
				streamBuffer.SetLength(0L);
				streamBuffer.Write(messageHeader, 0, messageHeader.Length);
				streamBuffer.Write(array, 0, array.Length);
			}
			byte[] buffer = streamBuffer.GetBuffer();
			if (messageType != EgMessageType.Operation)
			{
				buffer[messageHeader.Length - 1] = (byte)messageType;
			}
			if (encrypt)
			{
				buffer[messageHeader.Length - 1] = (byte)(buffer[messageHeader.Length - 1] | 0x80);
			}
			if (DoFraming)
			{
				int targetOffset = 1;
				Protocol.Serialize(streamBuffer.Length, buffer, ref targetOffset);
			}
			return streamBuffer;
		}

		internal bool EnqueueMessageAsPayload(DeliveryMode deliveryMode, StreamBuffer opMessage, byte channelId)
		{
			if (opMessage == null)
			{
				return false;
			}
			if (DoFraming)
			{
				byte[] buffer = opMessage.GetBuffer();
				buffer[5] = channelId;
				switch (deliveryMode)
				{
				case DeliveryMode.Unreliable:
					buffer[6] = 0;
					break;
				case DeliveryMode.Reliable:
					buffer[6] = 1;
					break;
				case DeliveryMode.UnreliableUnsequenced:
					buffer[6] = 2;
					break;
				case DeliveryMode.ReliableUnsequenced:
					buffer[6] = 3;
					break;
				default:
					throw new ArgumentOutOfRangeException("DeliveryMode", deliveryMode, null);
				}
			}
			lock (outgoingStream)
			{
				outgoingStream.Add(opMessage);
				outgoingCommandsInStream++;
			}
			int num = (ByteCountLastOperation = opMessage.Length);
			if (base.TrafficStatsEnabled)
			{
				switch (deliveryMode)
				{
				case DeliveryMode.Unreliable:
					base.TrafficStatsOutgoing.CountUnreliableOpCommand(num);
					break;
				case DeliveryMode.Reliable:
					base.TrafficStatsOutgoing.CountReliableOpCommand(num);
					break;
				default:
					throw new ArgumentOutOfRangeException("deliveryMode", deliveryMode, null);
				}
				base.TrafficStatsGameLevel.CountOperation(num);
			}
			return true;
		}

		internal void SendPing()
		{
			lastPingResult = SupportClass.GetTickCount();
			if (!DoFraming)
			{
				int tickCount = SupportClass.GetTickCount();
				SendOptions sendParams = new SendOptions
				{
					DeliveryMode = DeliveryMode.Reliable
				};
				EnqueueOperation(new Dictionary<byte, object> { { 1, tickCount } }, PhotonCodes.Ping, sendParams, EgMessageType.InternalOperationRequest);
				return;
			}
			int targetOffset = 1;
			Protocol.Serialize(SupportClass.GetTickCount(), pingRequest, ref targetOffset);
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.CountControlCommand(pingRequest.Length);
			}
			SendData(pingRequest, pingRequest.Length);
		}

		internal void SendData(byte[] data, int length)
		{
			try
			{
				bytesOut += length;
				if (base.TrafficStatsEnabled)
				{
					base.TrafficStatsOutgoing.TotalPacketCount++;
					base.TrafficStatsOutgoing.TotalCommandsInPackets += outgoingCommandsInStream;
				}
				if (base.NetworkSimulationSettings.IsSimulationEnabled)
				{
					byte[] array = new byte[length];
					Buffer.BlockCopy(data, 0, array, 0, length);
					SendNetworkSimulated(array);
				}
				else
				{
					PhotonSocket.Send(data, length);
				}
			}
			catch (Exception ex)
			{
				if ((int)base.debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
				}
				SupportClass.WriteStackTrace(ex);
			}
		}

		internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
		{
			if (inbuff == null)
			{
				if ((int)base.debugOut >= 1)
				{
					EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
				}
				return;
			}
			timestampOfLastReceive = SupportClass.GetTickCount();
			timeInt = SupportClass.GetTickCount() - timeBase;
			bytesIn += dataLength + 7;
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.TotalPacketCount++;
				base.TrafficStatsIncoming.TotalCommandsInPackets++;
			}
			if (inbuff[0] == 243 || inbuff[0] == 244)
			{
				byte[] array = new byte[dataLength];
				Buffer.BlockCopy(inbuff, 0, array, 0, dataLength);
				lock (incomingList)
				{
					incomingList.Enqueue(array);
					return;
				}
			}
			if (inbuff[0] == 240)
			{
				base.TrafficStatsIncoming.CountControlCommand(inbuff.Length);
				ReadPingResult(inbuff);
			}
			else if ((int)base.debugOut >= 1)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + inbuff[0]);
			}
		}

		private void ReadPingResult(byte[] inbuff)
		{
			int value = 0;
			int value2 = 0;
			int offset = 1;
			Protocol.Deserialize(out value, inbuff, ref offset);
			Protocol.Deserialize(out value2, inbuff, ref offset);
			lastRoundTripTime = SupportClass.GetTickCount() - value2;
			if (!serverTimeOffsetIsAvailable)
			{
				roundTripTime = lastRoundTripTime;
			}
			UpdateRoundTripTimeAndVariance(lastRoundTripTime);
			if (!serverTimeOffsetIsAvailable)
			{
				serverTimeOffset = value + (lastRoundTripTime >> 1) - SupportClass.GetTickCount();
				serverTimeOffsetIsAvailable = true;
			}
		}

		protected internal void ReadPingResult(OperationResponse operationResponse)
		{
			int num = (int)operationResponse.Parameters[2];
			int num2 = (int)operationResponse.Parameters[1];
			lastRoundTripTime = SupportClass.GetTickCount() - num2;
			if (!serverTimeOffsetIsAvailable)
			{
				roundTripTime = lastRoundTripTime;
			}
			UpdateRoundTripTimeAndVariance(lastRoundTripTime);
			if (!serverTimeOffsetIsAvailable)
			{
				serverTimeOffset = num + (lastRoundTripTime >> 1) - SupportClass.GetTickCount();
				serverTimeOffsetIsAvailable = true;
			}
		}
	}
}
