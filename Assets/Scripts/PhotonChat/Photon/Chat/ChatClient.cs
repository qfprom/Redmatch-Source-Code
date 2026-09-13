using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Chat
{
	public class ChatClient : IPhotonPeerListener
	{
		private const int FriendRequestListMax = 1024;

		public const int DefaultMaxSubscribers = 100;

		private string chatRegion = "EU";

		public int MessageLimit;

		public readonly Dictionary<string, ChatChannel> PublicChannels;

		public readonly Dictionary<string, ChatChannel> PrivateChannels;

		private readonly HashSet<string> PublicChannelsUnsubscribing;

		private readonly IChatClientListener listener;

		public ChatPeer chatPeer;

		private const string ChatAppName = "chat";

		private bool didAuthenticate;

		private int? statusToSetWhenConnected;

		private object messageToSetWhenConnected;

		private int msDeltaForServiceCalls = 50;

		private int msTimestampOfLastServiceCall;

		public string NameServerAddress { get; private set; }

		public string FrontendAddress { get; private set; }

		public string ChatRegion
		{
			get
			{
				return chatRegion;
			}
			set
			{
				chatRegion = value;
			}
		}

		public ChatState State { get; private set; }

		public ChatDisconnectCause DisconnectedCause { get; private set; }

		public bool CanChat
		{
			get
			{
				return State == ChatState.ConnectedToFrontEnd && HasPeer;
			}
		}

		private bool HasPeer
		{
			get
			{
				return chatPeer != null;
			}
		}

		public string AppVersion { get; private set; }

		public string AppId { get; private set; }

		public AuthenticationValues AuthValues { get; set; }

		public string UserId
		{
			get
			{
				return (AuthValues == null) ? null : AuthValues.UserId;
			}
			private set
			{
				if (AuthValues == null)
				{
					AuthValues = new AuthenticationValues();
				}
				AuthValues.UserId = value;
			}
		}

		public bool UseBackgroundWorkerForSending { get; set; }

		public ConnectionProtocol TransportProtocol
		{
			get
			{
				return chatPeer.TransportProtocol;
			}
			set
			{
				if (chatPeer == null || chatPeer.PeerState != PeerStateValue.Disconnected)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Can't set TransportProtocol. Disconnect first! " + ((chatPeer == null) ? "The chatPeer is null." : ("PeerState: " + chatPeer.PeerState)));
				}
				else
				{
					chatPeer.TransportProtocol = value;
				}
			}
		}

		public Dictionary<ConnectionProtocol, Type> SocketImplementationConfig
		{
			get
			{
				return chatPeer.SocketImplementationConfig;
			}
		}

		public DebugLevel DebugOut
		{
			get
			{
				return chatPeer.DebugOut;
			}
			set
			{
				chatPeer.DebugOut = value;
			}
		}

		public ChatClient(IChatClientListener listener, ConnectionProtocol protocol = ConnectionProtocol.Udp)
		{
			this.listener = listener;
			State = ChatState.Uninitialized;
			chatPeer = new ChatPeer(this, protocol);
			chatPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV18;
			PublicChannels = new Dictionary<string, ChatChannel>();
			PrivateChannels = new Dictionary<string, ChatChannel>();
			PublicChannelsUnsubscribing = new HashSet<string>();
		}

		public bool CanChatInChannel(string channelName)
		{
			return CanChat && PublicChannels.ContainsKey(channelName) && !PublicChannelsUnsubscribing.Contains(channelName);
		}

		public bool Connect(string appId, string appVersion, AuthenticationValues authValues)
		{
			chatPeer.TimePingInterval = 3000;
			DisconnectedCause = ChatDisconnectCause.None;
			AuthValues = authValues;
			AppId = appId;
			AppVersion = appVersion;
			didAuthenticate = false;
			chatPeer.QuickResendAttempts = 2;
			chatPeer.SentCountAllowance = 7;
			PublicChannels.Clear();
			PrivateChannels.Clear();
			PublicChannelsUnsubscribing.Clear();
			NameServerAddress = chatPeer.NameServerAddress;
			bool flag = chatPeer.Connect();
			if (flag)
			{
				State = ChatState.ConnectingToNameServer;
			}
			if (UseBackgroundWorkerForSending)
			{
				SupportClass.StartBackgroundCalls(SendOutgoingInBackground, msDeltaForServiceCalls, "ChatClient Service Thread");
			}
			return flag;
		}

		public bool ConnectAndSetStatus(string appId, string appVersion, AuthenticationValues authValues, int status = 2, object message = null)
		{
			statusToSetWhenConnected = status;
			messageToSetWhenConnected = message;
			return Connect(appId, appVersion, authValues);
		}

		public void Service()
		{
			while (HasPeer && chatPeer.DispatchIncomingCommands())
			{
			}
			if (!UseBackgroundWorkerForSending && (Environment.TickCount - msTimestampOfLastServiceCall > msDeltaForServiceCalls || msTimestampOfLastServiceCall == 0))
			{
				msTimestampOfLastServiceCall = Environment.TickCount;
				while (HasPeer && chatPeer.SendOutgoingCommands())
				{
				}
			}
		}

		private bool SendOutgoingInBackground()
		{
			while (HasPeer && chatPeer.SendOutgoingCommands())
			{
			}
			return State != ChatState.Disconnected;
		}

		[Obsolete("Better use UseBackgroundWorkerForSending and Service().")]
		public void SendAcksOnly()
		{
			if (HasPeer)
			{
				chatPeer.SendAcksOnly();
			}
		}

		public void Disconnect()
		{
			if (HasPeer && chatPeer.PeerState != PeerStateValue.Disconnected)
			{
				chatPeer.Disconnect();
			}
		}

		public void StopThread()
		{
			if (HasPeer)
			{
				chatPeer.StopThread();
			}
		}

		public bool Subscribe(string[] channels)
		{
			return Subscribe(channels, 0);
		}

		public bool Subscribe(string[] channels, int[] lastMsgIds)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Subscribe called while not connected to front end server.");
				}
				return false;
			}
			if (channels == null || channels.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Subscribe can't be called for empty or null channels-list.");
				}
				return false;
			}
			for (int i = 0; i < channels.Length; i++)
			{
				if (string.IsNullOrEmpty(channels[i]))
				{
					if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, string.Format("Subscribe can't be called with a null or empty channel name at index {0}.", i));
					}
					return false;
				}
			}
			if (lastMsgIds == null || lastMsgIds.Length != channels.Length)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Subscribe can't be called when \"lastMsgIds\" array is null or does not have the same length as \"channels\" array.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(0, channels);
			dictionary.Add(9, lastMsgIds);
			dictionary.Add(14, -1);
			Dictionary<byte, object> operationParameters = dictionary;
			return chatPeer.SendOperation(0, operationParameters, SendOptions.SendReliable);
		}

		public bool Subscribe(string[] channels, int messagesFromHistory)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Subscribe called while not connected to front end server.");
				}
				return false;
			}
			if (channels == null || channels.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Subscribe can't be called for empty or null channels-list.");
				}
				return false;
			}
			return SendChannelOperation(channels, 0, messagesFromHistory);
		}

		public bool Unsubscribe(string[] channels)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Unsubscribe called while not connected to front end server.");
				}
				return false;
			}
			if (channels == null || channels.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Unsubscribe can't be called for empty or null channels-list.");
				}
				return false;
			}
			foreach (string item in channels)
			{
				PublicChannelsUnsubscribing.Add(item);
			}
			return SendChannelOperation(channels, 1, 0);
		}

		public bool PublishMessage(string channelName, object message, bool forwardAsWebhook = false)
		{
			return publishMessage(channelName, message, true, forwardAsWebhook);
		}

		internal bool PublishMessageUnreliable(string channelName, object message, bool forwardAsWebhook = false)
		{
			return publishMessage(channelName, message, false, forwardAsWebhook);
		}

		private bool publishMessage(string channelName, object message, bool reliable, bool forwardAsWebhook = false)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "PublishMessage called while not connected to front end server.");
				}
				return false;
			}
			if (string.IsNullOrEmpty(channelName) || message == null)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "PublishMessage parameters must be non-null and not empty.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(1, channelName);
			dictionary.Add(3, message);
			Dictionary<byte, object> dictionary2 = dictionary;
			if (forwardAsWebhook)
			{
				dictionary2.Add(21, (byte)1);
			}
			return chatPeer.SendOperation(2, dictionary2, new SendOptions
			{
				Reliability = reliable
			});
		}

		public bool SendPrivateMessage(string target, object message, bool forwardAsWebhook = false)
		{
			return SendPrivateMessage(target, message, false, forwardAsWebhook);
		}

		public bool SendPrivateMessage(string target, object message, bool encrypt, bool forwardAsWebhook)
		{
			return sendPrivateMessage(target, message, encrypt, true, forwardAsWebhook);
		}

		internal bool SendPrivateMessageUnreliable(string target, object message, bool encrypt, bool forwardAsWebhook = false)
		{
			return sendPrivateMessage(target, message, encrypt, false, forwardAsWebhook);
		}

		private bool sendPrivateMessage(string target, object message, bool encrypt, bool reliable, bool forwardAsWebhook = false)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "SendPrivateMessage called while not connected to front end server.");
				}
				return false;
			}
			if (string.IsNullOrEmpty(target) || message == null)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "SendPrivateMessage parameters must be non-null and not empty.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(225, target);
			dictionary.Add(3, message);
			Dictionary<byte, object> dictionary2 = dictionary;
			if (forwardAsWebhook)
			{
				dictionary2.Add(21, (byte)1);
			}
			return chatPeer.SendOperation(3, dictionary2, new SendOptions
			{
				Reliability = reliable,
				Encrypt = encrypt
			});
		}

		private bool SetOnlineStatus(int status, object message, bool skipMessage)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "SetOnlineStatus called while not connected to front end server.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(10, status);
			Dictionary<byte, object> dictionary2 = dictionary;
			if (skipMessage)
			{
				dictionary2[12] = true;
			}
			else
			{
				dictionary2[3] = message;
			}
			return chatPeer.SendOperation(5, dictionary2, SendOptions.SendReliable);
		}

		public bool SetOnlineStatus(int status)
		{
			return SetOnlineStatus(status, null, true);
		}

		public bool SetOnlineStatus(int status, object message)
		{
			return SetOnlineStatus(status, message, false);
		}

		public bool AddFriends(string[] friends)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "AddFriends called while not connected to front end server.");
				}
				return false;
			}
			if (friends == null || friends.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "AddFriends can't be called for empty or null list.");
				}
				return false;
			}
			if (friends.Length > 1024)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "AddFriends max list size exceeded: " + friends.Length + " > " + 1024);
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(11, friends);
			Dictionary<byte, object> operationParameters = dictionary;
			return chatPeer.SendOperation(6, operationParameters, SendOptions.SendReliable);
		}

		public bool RemoveFriends(string[] friends)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "RemoveFriends called while not connected to front end server.");
				}
				return false;
			}
			if (friends == null || friends.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "RemoveFriends can't be called for empty or null list.");
				}
				return false;
			}
			if (friends.Length > 1024)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "RemoveFriends max list size exceeded: " + friends.Length + " > " + 1024);
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(11, friends);
			Dictionary<byte, object> operationParameters = dictionary;
			return chatPeer.SendOperation(7, operationParameters, SendOptions.SendReliable);
		}

		public string GetPrivateChannelNameByUser(string userName)
		{
			return string.Format("{0}:{1}", UserId, userName);
		}

		public bool TryGetChannel(string channelName, bool isPrivate, out ChatChannel channel)
		{
			if (!isPrivate)
			{
				return PublicChannels.TryGetValue(channelName, out channel);
			}
			return PrivateChannels.TryGetValue(channelName, out channel);
		}

		public bool TryGetChannel(string channelName, out ChatChannel channel)
		{
			bool flag = false;
			if (PublicChannels.TryGetValue(channelName, out channel))
			{
				return true;
			}
			return PrivateChannels.TryGetValue(channelName, out channel);
		}

		void IPhotonPeerListener.DebugReturn(DebugLevel level, string message)
		{
			listener.DebugReturn(level, message);
		}

		void IPhotonPeerListener.OnEvent(EventData eventData)
		{
			switch (eventData.Code)
			{
			case 0:
				HandleChatMessagesEvent(eventData);
				break;
			case 2:
				HandlePrivateMessageEvent(eventData);
				break;
			case 4:
				HandleStatusUpdate(eventData);
				break;
			case 5:
				HandleSubscribeEvent(eventData);
				break;
			case 6:
				HandleUnsubscribeEvent(eventData);
				break;
			case 8:
				HandleUserSubscribedEvent(eventData);
				break;
			case 9:
				HandleUserUnsubscribedEvent(eventData);
				break;
			case 1:
			case 3:
			case 7:
				break;
			}
		}

		void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
		{
			switch (operationResponse.OperationCode)
			{
			case 230:
				HandleAuthResponse(operationResponse);
				return;
			}
			if (operationResponse.ReturnCode != 0 && (int)DebugOut >= 1)
			{
				if (operationResponse.ReturnCode == -2)
				{
					listener.DebugReturn(DebugLevel.ERROR, string.Format("Chat Operation {0} unknown on server. Check your AppId and make sure it's for a Chat application.", operationResponse.OperationCode));
				}
				else
				{
					listener.DebugReturn(DebugLevel.ERROR, string.Format("Chat Operation {0} failed (Code: {1}). Debug Message: {2}", operationResponse.OperationCode, operationResponse.ReturnCode, operationResponse.DebugMessage));
				}
			}
		}

		void IPhotonPeerListener.OnStatusChanged(StatusCode statusCode)
		{
			switch (statusCode)
			{
			case StatusCode.Connect:
				if (!chatPeer.IsProtocolSecure)
				{
					chatPeer.EstablishEncryption();
				}
				else if (!didAuthenticate)
				{
					didAuthenticate = chatPeer.AuthenticateOnNameServer(AppId, AppVersion, chatRegion, AuthValues);
					if (!didAuthenticate && (int)DebugOut >= 1)
					{
						((IPhotonPeerListener)this).DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected. State: " + State);
					}
				}
				if (State == ChatState.ConnectingToNameServer)
				{
					State = ChatState.ConnectedToNameServer;
					listener.OnChatStateChange(State);
				}
				else if (State == ChatState.ConnectingToFrontEnd)
				{
					AuthenticateOnFrontEnd();
				}
				break;
			case StatusCode.EncryptionEstablished:
				if (!didAuthenticate)
				{
					didAuthenticate = chatPeer.AuthenticateOnNameServer(AppId, AppVersion, chatRegion, AuthValues);
					if (!didAuthenticate && (int)DebugOut >= 1)
					{
						((IPhotonPeerListener)this).DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected. State: " + State);
					}
				}
				break;
			case StatusCode.EncryptionFailedToEstablish:
				State = ChatState.Disconnecting;
				chatPeer.Disconnect();
				break;
			case StatusCode.Disconnect:
				if (State == ChatState.Authenticated)
				{
					ConnectToFrontEnd();
					break;
				}
				State = ChatState.Disconnected;
				listener.OnChatStateChange(ChatState.Disconnected);
				listener.OnDisconnected();
				break;
			}
		}

		private bool SendChannelOperation(string[] channels, byte operation, int historyLength)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(0, channels);
			Dictionary<byte, object> dictionary2 = dictionary;
			if (historyLength != 0)
			{
				dictionary2.Add(14, historyLength);
			}
			return chatPeer.SendOperation(operation, dictionary2, SendOptions.SendReliable);
		}

		private void HandlePrivateMessageEvent(EventData eventData)
		{
			object message = eventData.Parameters[3];
			string text = (string)eventData.Parameters[5];
			int msgId = (int)eventData.Parameters[8];
			string privateChannelNameByUser;
			if (UserId != null && UserId.Equals(text))
			{
				string userName = (string)eventData.Parameters[225];
				privateChannelNameByUser = GetPrivateChannelNameByUser(userName);
			}
			else
			{
				privateChannelNameByUser = GetPrivateChannelNameByUser(text);
			}
			ChatChannel value;
			if (!PrivateChannels.TryGetValue(privateChannelNameByUser, out value))
			{
				value = new ChatChannel(privateChannelNameByUser);
				value.IsPrivate = true;
				value.MessageLimit = MessageLimit;
				PrivateChannels.Add(value.Name, value);
			}
			value.Add(text, message, msgId);
			listener.OnPrivateMessage(text, message, privateChannelNameByUser);
		}

		private void HandleChatMessagesEvent(EventData eventData)
		{
			object[] messages = (object[])eventData.Parameters[2];
			string[] senders = (string[])eventData.Parameters[4];
			string text = (string)eventData.Parameters[1];
			int lastMsgId = (int)eventData.Parameters[8];
			ChatChannel value;
			if (!PublicChannels.TryGetValue(text, out value))
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Channel " + text + " for incoming message event not found.");
				}
			}
			else
			{
				value.Add(senders, messages, lastMsgId);
				listener.OnGetMessages(text, senders, messages);
			}
		}

		private void HandleSubscribeEvent(EventData eventData)
		{
			string[] array = (string[])eventData.Parameters[0];
			bool[] array2 = (bool[])eventData.Parameters[15];
			for (int i = 0; i < array.Length; i++)
			{
				if (array2[i])
				{
					string text = array[i];
					ChatChannel chatChannel;
					if (!PublicChannels.ContainsKey(text))
					{
						chatChannel = new ChatChannel(text);
						chatChannel.MessageLimit = MessageLimit;
						PublicChannels.Add(chatChannel.Name, chatChannel);
					}
					else
					{
						chatChannel = PublicChannels[text];
					}
					if (eventData.Parameters.ContainsKey(22))
					{
						Dictionary<object, object> newProperties = eventData.Parameters[22] as Dictionary<object, object>;
						chatChannel.ReadProperties(newProperties);
					}
					if (eventData.Parameters.ContainsKey(23))
					{
						string[] users = eventData.Parameters[23] as string[];
						chatChannel.TryAddSubscriber(UserId);
						chatChannel.AddSubscribers(users);
					}
				}
			}
			listener.OnSubscribed(array, array2);
		}

		private void HandleUnsubscribeEvent(EventData eventData)
		{
			string[] array = (string[])eventData[0];
			foreach (string text in array)
			{
				PublicChannels.Remove(text);
				PublicChannelsUnsubscribing.Remove(text);
			}
			listener.OnUnsubscribed(array);
		}

		private void HandleAuthResponse(OperationResponse operationResponse)
		{
			if ((int)DebugOut >= 3)
			{
				listener.DebugReturn(DebugLevel.INFO, operationResponse.ToStringFull() + " on: " + chatPeer.NameServerAddress);
			}
			if (operationResponse.ReturnCode == 0)
			{
				if (State == ChatState.ConnectedToNameServer)
				{
					State = ChatState.Authenticated;
					listener.OnChatStateChange(State);
					if (operationResponse.Parameters.ContainsKey(221))
					{
						if (AuthValues == null)
						{
							AuthValues = new AuthenticationValues();
						}
						AuthValues.Token = operationResponse[221] as string;
						FrontendAddress = (string)operationResponse[230];
						chatPeer.Disconnect();
					}
					else if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, "No secret in authentication response.");
					}
					if (operationResponse.Parameters.ContainsKey(225))
					{
						string text = operationResponse.Parameters[225] as string;
						if (!string.IsNullOrEmpty(text))
						{
							UserId = text;
							listener.DebugReturn(DebugLevel.INFO, string.Format("Received your UserID from server. Updating local value to: {0}", UserId));
						}
					}
				}
				else if (State == ChatState.ConnectingToFrontEnd)
				{
					State = ChatState.ConnectedToFrontEnd;
					listener.OnChatStateChange(State);
					listener.OnConnected();
					if (statusToSetWhenConnected.HasValue)
					{
						SetOnlineStatus(statusToSetWhenConnected.Value, messageToSetWhenConnected);
						statusToSetWhenConnected = null;
					}
				}
			}
			else
			{
				switch (operationResponse.ReturnCode)
				{
				case short.MaxValue:
					DisconnectedCause = ChatDisconnectCause.InvalidAuthentication;
					break;
				case 32755:
					DisconnectedCause = ChatDisconnectCause.CustomAuthenticationFailed;
					break;
				case 32756:
					DisconnectedCause = ChatDisconnectCause.InvalidRegion;
					break;
				case 32757:
					DisconnectedCause = ChatDisconnectCause.MaxCcuReached;
					break;
				case -3:
					DisconnectedCause = ChatDisconnectCause.OperationNotAllowedInCurrentState;
					break;
				}
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Authentication request error: " + operationResponse.ReturnCode + ". Disconnecting.");
				}
				State = ChatState.Disconnecting;
				chatPeer.Disconnect();
			}
		}

		private void HandleStatusUpdate(EventData eventData)
		{
			string user = (string)eventData.Parameters[5];
			int status = (int)eventData.Parameters[10];
			object message = null;
			bool flag = eventData.Parameters.ContainsKey(3);
			if (flag)
			{
				message = eventData.Parameters[3];
			}
			listener.OnStatusUpdate(user, status, flag, message);
		}

		private void ConnectToFrontEnd()
		{
			State = ChatState.ConnectingToFrontEnd;
			if ((int)DebugOut >= 3)
			{
				listener.DebugReturn(DebugLevel.INFO, "Connecting to frontend " + FrontendAddress);
			}
			chatPeer.Connect(FrontendAddress, "chat");
		}

		private bool AuthenticateOnFrontEnd()
		{
			if (AuthValues != null)
			{
				if (string.IsNullOrEmpty(AuthValues.Token))
				{
					if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, "Can't authenticate on front end server. Secret is not set");
					}
					return false;
				}
				Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
				dictionary.Add(221, AuthValues.Token);
				Dictionary<byte, object> operationParameters = dictionary;
				return chatPeer.SendOperation(230, operationParameters, SendOptions.SendReliable);
			}
			if ((int)DebugOut >= 1)
			{
				listener.DebugReturn(DebugLevel.ERROR, "Can't authenticate on front end server. Authentication Values are not set");
			}
			return false;
		}

		private void HandleUserUnsubscribedEvent(EventData eventData)
		{
			string text = eventData.Parameters[1] as string;
			string text2 = eventData.Parameters[225] as string;
			ChatChannel value;
			if (!PublicChannels.TryGetValue(text, out value))
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, string.Format("Channel {0} for incoming UserUnsubscribed ({1}) event not found.", text, text2));
				}
			}
			else if (!value.Subscribers.Contains(text2))
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, string.Format("Channel {0} does not contain unsubscribed user {1}.", text, text2));
				}
			}
			else
			{
				value.Subscribers.Remove(text2);
				listener.OnUserUnsubscribed(text, text2);
			}
		}

		private void HandleUserSubscribedEvent(EventData eventData)
		{
			string text = eventData.Parameters[1] as string;
			string text2 = eventData.Parameters[225] as string;
			ChatChannel value;
			if (!PublicChannels.TryGetValue(text, out value))
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, string.Format("Channel {0} for incoming UserSubscribed ({1}) event not found.", text, text2));
				}
				value = new ChatChannel(text);
				PublicChannels.Add(text, value);
			}
			if (value.Subscribers.Contains(text2))
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, string.Format("Channel {0} already contains newly subscribed user {1}.", text, text2));
				}
				return;
			}
			value.Subscribers.Add(text2);
			if (value.Subscribers.Count > value.MaxSubscribers)
			{
				listener.DebugReturn(DebugLevel.WARNING, string.Format("Channel {0} max subscribers exceeded? count={1} > MaxSubscribers={2}.", text, value.Subscribers.Count, value.MaxSubscribers));
			}
			listener.OnUserSubscribed(text, text2);
		}

		public bool Subscribe(string channel, int lastMsgId = 0, int messagesFromHistory = -1, ChannelCreationOptions creationOptions = null)
		{
			if (creationOptions == null)
			{
				creationOptions = ChannelCreationOptions.Default;
			}
			int maxSubscribers = creationOptions.MaxSubscribers;
			bool publishSubscribers = creationOptions.PublishSubscribers;
			if (maxSubscribers < 0)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Cannot set MaxSubscribers < 0.");
				}
				return false;
			}
			if (lastMsgId < 0)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "lastMsgId cannot be < 0.");
				}
				return false;
			}
			if (messagesFromHistory < -1)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "messagesFromHistory < -1, setting it to -1");
				}
				messagesFromHistory = -1;
			}
			if (lastMsgId > 0 && messagesFromHistory == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "lastMsgId will be ignored because messagesFromHistory == 0");
				}
				lastMsgId = 0;
			}
			Dictionary<object, object> dictionary = null;
			if (publishSubscribers)
			{
				if (maxSubscribers > 100)
				{
					if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, string.Format("Cannot set MaxSubscribers > {0} when PublishSubscribers == true.", 100));
					}
					return false;
				}
				dictionary = new Dictionary<object, object>();
				dictionary[(byte)254] = true;
			}
			if (maxSubscribers > 0)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<object, object>();
				}
				dictionary[byte.MaxValue] = maxSubscribers;
			}
			Dictionary<byte, object> dictionary2 = new Dictionary<byte, object>();
			dictionary2.Add(0, new string[1] { channel });
			Dictionary<byte, object> dictionary3 = dictionary2;
			if (messagesFromHistory != 0)
			{
				dictionary3.Add(14, messagesFromHistory);
			}
			if (lastMsgId > 0)
			{
				dictionary3.Add(9, new int[1] { lastMsgId });
			}
			if (dictionary != null && dictionary.Count > 0)
			{
				dictionary3.Add(22, dictionary);
			}
			return chatPeer.SendOperation(0, dictionary3, SendOptions.SendReliable);
		}
	}
}
