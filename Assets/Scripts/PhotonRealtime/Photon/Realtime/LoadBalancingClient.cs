using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime
{
	public class LoadBalancingClient : IPhotonPeerListener
	{
		private bool didAuthenticate;

		public AuthModeOption AuthMode;

		public EncryptionMode EncryptionMode;

		public ConnectionProtocol ExpectedProtocol;

		private string tokenCache;

		public string NameServerHost = "ns.exitgames.com";

		public string NameServerHttp = "http://ns.exitgames.com:80/photon/n";

		private static readonly Dictionary<ConnectionProtocol, int> ProtocolToNameServerPort = new Dictionary<ConnectionProtocol, int>
		{
			{
				ConnectionProtocol.Udp,
				5058
			},
			{
				ConnectionProtocol.Tcp,
				4533
			},
			{
				ConnectionProtocol.WebSocket,
				9093
			},
			{
				ConnectionProtocol.WebSocketSecure,
				19093
			}
		};

		private ClientState state;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<ClientState, ClientState> StateChanged__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<EventData> EventReceived__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<OperationResponse> OpResponseReceived__BackingField;

		public ConnectionCallbacksContainer ConnectionCallbackTargets = new ConnectionCallbacksContainer();

		public MatchMakingCallbacksContainer MatchMakingCallbackTargets = new MatchMakingCallbacksContainer();

		internal InRoomCallbacksContainer InRoomCallbackTargets = new InRoomCallbacksContainer();

		internal LobbyCallbacksContainer LobbyCallbackTargets = new LobbyCallbacksContainer();

		internal WebRpcCallbacksContainer WebRpcCallbackTargets = new WebRpcCallbacksContainer();

		public bool EnableLobbyStatistics;

		private readonly List<TypedLobbyInfo> lobbyStatistics = new List<TypedLobbyInfo>();

		private JoinType lastJoinType;

		private EnterRoomParams enterRoomParamsCache;

		private OperationResponse failedRoomEntryOperation;

		private const int FriendRequestListMax = 512;

		private string[] friendListRequested;

		public RegionHandler RegionHandler;

		public LoadBalancingPeer LoadBalancingPeer { get; private set; }

		public string AppVersion { get; set; }

		public string AppId { get; set; }

		public AuthenticationValues AuthValues { get; set; }

		private string TokenForInit
		{
			get
			{
				if (AuthMode == AuthModeOption.Auth)
				{
					return null;
				}
				return (AuthValues == null) ? null : AuthValues.Token;
			}
		}

		public bool IsUsingNameServer { get; set; }

		public string NameServerAddress
		{
			get
			{
				return GetNameServerAddress();
			}
		}

		public bool UseAlternativeUdpPorts { get; set; }

		public string CurrentServerAddress
		{
			get
			{
				return LoadBalancingPeer.ServerAddress;
			}
		}

		public string MasterServerAddress { get; set; }

		public string GameServerAddress { get; protected internal set; }

		public ServerConnection Server { get; private set; }

		public ClientState State
		{
			get
			{
				return state;
			}
			set
			{
				if (state != value)
				{
					ClientState arg = state;
					state = value;
					if (StateChanged__BackingField != null)
					{
						StateChanged__BackingField(arg, state);
					}
				}
			}
		}

		public bool IsConnected
		{
			get
			{
				return LoadBalancingPeer != null && State != ClientState.PeerCreated && State != ClientState.Disconnected;
			}
		}

		public bool IsConnectedAndReady
		{
			get
			{
				if (LoadBalancingPeer == null)
				{
					return false;
				}
				switch (State)
				{
				case ClientState.PeerCreated:
				case ClientState.Authenticating:
				case ClientState.ConnectingToGameserver:
				case ClientState.Joining:
				case ClientState.Leaving:
				case ClientState.ConnectingToMasterserver:
				case ClientState.Disconnecting:
				case ClientState.Disconnected:
				case ClientState.ConnectingToNameServer:
					return false;
				default:
					return true;
				}
			}
		}

		public DisconnectCause DisconnectedCause { get; protected set; }

		public bool InLobby { get; private set; }

		public TypedLobby CurrentLobby { get; internal set; }

		public Player LocalPlayer { get; internal set; }

		public string NickName
		{
			get
			{
				return LocalPlayer.NickName;
			}
			set
			{
				if (LocalPlayer != null)
				{
					LocalPlayer.NickName = value;
				}
			}
		}

		public string UserId
		{
			get
			{
				if (AuthValues != null)
				{
					return AuthValues.UserId;
				}
				return null;
			}
			set
			{
				if (AuthValues == null)
				{
					AuthValues = new AuthenticationValues();
				}
				AuthValues.UserId = value;
			}
		}

		public Room CurrentRoom { get; private set; }

		public bool InRoom
		{
			get
			{
				return state == ClientState.Joined;
			}
		}

		public int PlayersOnMasterCount { get; internal set; }

		public int PlayersInRoomsCount { get; internal set; }

		public int RoomsCount { get; internal set; }

		public bool IsFetchingFriendList
		{
			get
			{
				return friendListRequested != null;
			}
		}

		public string CloudRegion { get; private set; }

		public event Action<ClientState, ClientState> StateChanged
		{
			add
			{
				Action<ClientState, ClientState> action = StateChanged__BackingField;
				Action<ClientState, ClientState> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref StateChanged__BackingField, (Action<ClientState, ClientState>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<ClientState, ClientState> action = StateChanged__BackingField;
				Action<ClientState, ClientState> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref StateChanged__BackingField, (Action<ClientState, ClientState>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public event Action<EventData> EventReceived
		{
			add
			{
				Action<EventData> action = EventReceived__BackingField;
				Action<EventData> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref EventReceived__BackingField, (Action<EventData>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<EventData> action = EventReceived__BackingField;
				Action<EventData> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref EventReceived__BackingField, (Action<EventData>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public event Action<OperationResponse> OpResponseReceived
		{
			add
			{
				Action<OperationResponse> action = OpResponseReceived__BackingField;
				Action<OperationResponse> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OpResponseReceived__BackingField, (Action<OperationResponse>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<OperationResponse> action = OpResponseReceived__BackingField;
				Action<OperationResponse> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OpResponseReceived__BackingField, (Action<OperationResponse>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public LoadBalancingClient(ConnectionProtocol protocol = ConnectionProtocol.Udp)
		{
			LoadBalancingPeer = new LoadBalancingPeer(this, protocol);
			LoadBalancingPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV18;
			LocalPlayer = CreatePlayer(string.Empty, -1, true, null);
			State = ClientState.PeerCreated;
		}

		public LoadBalancingClient(string masterAddress, string appId, string gameVersion, ConnectionProtocol protocol = ConnectionProtocol.Udp)
			: this(protocol)
		{
			MasterServerAddress = masterAddress;
			AppId = appId;
			AppVersion = gameVersion;
		}

		private string GetNameServerAddress()
		{
			int value = 0;
			ProtocolToNameServerPort.TryGetValue(LoadBalancingPeer.TransportProtocol, out value);
			if (LoadBalancingPeer.TransportProtocol == ConnectionProtocol.Udp && UseAlternativeUdpPorts)
			{
				value = 27000;
			}
			switch (LoadBalancingPeer.TransportProtocol)
			{
			case ConnectionProtocol.Udp:
			case ConnectionProtocol.Tcp:
				return string.Format("{0}:{1}", NameServerHost, value);
			case ConnectionProtocol.WebSocket:
				return string.Format("ws://{0}:{1}", NameServerHost, value);
			case ConnectionProtocol.WebSocketSecure:
				return string.Format("wss://{0}:{1}", NameServerHost, value);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public virtual bool Connect()
		{
			DisconnectedCause = DisconnectCause.None;
			if (LoadBalancingPeer.Connect(MasterServerAddress, AppId, TokenForInit))
			{
				State = ClientState.ConnectingToMasterserver;
				return true;
			}
			return false;
		}

		public bool ConnectToNameServer()
		{
			IsUsingNameServer = true;
			CloudRegion = null;
			if (AuthMode == AuthModeOption.AuthOnceWss)
			{
				ExpectedProtocol = LoadBalancingPeer.TransportProtocol;
				LoadBalancingPeer.TransportProtocol = ConnectionProtocol.WebSocketSecure;
			}
			if (!LoadBalancingPeer.Connect(NameServerAddress, "NameServer", TokenForInit))
			{
				return false;
			}
			State = ClientState.ConnectingToNameServer;
			return true;
		}

		public bool ConnectToRegionMaster(string region)
		{
			IsUsingNameServer = true;
			if (State == ClientState.ConnectedToNameServer)
			{
				CloudRegion = region;
				return CallAuthenticate();
			}
			LoadBalancingPeer.Disconnect();
			CloudRegion = region;
			if (AuthMode == AuthModeOption.AuthOnceWss)
			{
				ExpectedProtocol = LoadBalancingPeer.TransportProtocol;
				LoadBalancingPeer.TransportProtocol = ConnectionProtocol.WebSocketSecure;
			}
			if (!LoadBalancingPeer.Connect(NameServerAddress, "NameServer", null))
			{
				return false;
			}
			State = ClientState.ConnectingToNameServer;
			return true;
		}

		private bool Connect(string serverAddress, ServerConnection type)
		{
			if (State == ClientState.Disconnecting)
			{
				DebugReturn(DebugLevel.ERROR, "Connect() failed. Can't connect while disconnecting (still). Current state: " + State);
				return false;
			}
			bool flag = LoadBalancingPeer.Connect(serverAddress, string.Empty, TokenForInit);
			if (flag)
			{
				switch (type)
				{
				case ServerConnection.NameServer:
					State = ClientState.ConnectingToNameServer;
					break;
				case ServerConnection.MasterServer:
					State = ClientState.ConnectingToMasterserver;
					break;
				case ServerConnection.GameServer:
					State = ClientState.ConnectingToGameserver;
					break;
				}
			}
			return flag;
		}

		private bool ConnectToGameServer()
		{
			if (LoadBalancingPeer.Connect(GameServerAddress, AppId, TokenForInit))
			{
				State = ClientState.ConnectingToGameserver;
				return true;
			}
			return false;
		}

		public bool ReconnectToMaster()
		{
			if (AuthValues == null)
			{
				DebugReturn(DebugLevel.WARNING, "ReconnectToMaster() with AuthValues == null is not correct!");
				AuthValues = new AuthenticationValues();
			}
			AuthValues.Token = tokenCache;
			return Connect(MasterServerAddress, ServerConnection.MasterServer);
		}

		public bool ReconnectAndRejoin()
		{
			if (string.IsNullOrEmpty(GameServerAddress))
			{
				DebugReturn(DebugLevel.ERROR, "ReconnectAndRejoin() failed. It seems the client wasn't connected to a game server before (no address).");
				return false;
			}
			if (enterRoomParamsCache == null)
			{
				DebugReturn(DebugLevel.ERROR, "ReconnectAndRejoin() failed. It seems the client doesn't have any previous room to re-join.");
				return false;
			}
			if (tokenCache == null)
			{
				DebugReturn(DebugLevel.ERROR, "ReconnectAndRejoin() failed. It seems the client doesn't have any previous authentication token to re-connect.");
				return false;
			}
			if (AuthValues == null)
			{
				AuthValues = new AuthenticationValues();
			}
			AuthValues.Token = tokenCache;
			if (!string.IsNullOrEmpty(GameServerAddress) && enterRoomParamsCache != null)
			{
				lastJoinType = JoinType.JoinRoom;
				enterRoomParamsCache.RejoinOnly = true;
				return Connect(GameServerAddress, ServerConnection.GameServer);
			}
			return false;
		}

		public void Disconnect()
		{
			if (State != ClientState.Disconnected)
			{
				State = ClientState.Disconnecting;
				LoadBalancingPeer.Disconnect();
			}
		}

		private void DisconnectToReconnect()
		{
			switch (Server)
			{
			case ServerConnection.NameServer:
				State = ClientState.DisconnectingFromNameServer;
				break;
			case ServerConnection.MasterServer:
				State = ClientState.DisconnectingFromMasterserver;
				break;
			case ServerConnection.GameServer:
				State = ClientState.DisconnectingFromGameserver;
				break;
			}
			LoadBalancingPeer.Disconnect();
		}

		private bool CallAuthenticate()
		{
			if (AuthMode == AuthModeOption.Auth)
			{
				return LoadBalancingPeer.OpAuthenticate(AppId, AppVersion, AuthValues, CloudRegion, EnableLobbyStatistics && Server == ServerConnection.MasterServer);
			}
			return LoadBalancingPeer.OpAuthenticateOnce(AppId, AppVersion, AuthValues, CloudRegion, EncryptionMode, ExpectedProtocol);
		}

		public void Service()
		{
			if (LoadBalancingPeer != null)
			{
				LoadBalancingPeer.Service();
			}
		}

		private bool OpGetRegions()
		{
			if (Server != ServerConnection.NameServer)
			{
				return false;
			}
			return LoadBalancingPeer.OpGetRegions(AppId);
		}

		public bool OpFindFriends(string[] friendsToFind)
		{
			if (LoadBalancingPeer == null)
			{
				DebugReturn(DebugLevel.WARNING, "OpFindFriends aborted: LoadBalancingPeer is null.");
				return false;
			}
			if (IsFetchingFriendList || Server != ServerConnection.MasterServer)
			{
				DebugReturn(DebugLevel.WARNING, "OpFindFriends skipped: already fetching friends list.");
				return false;
			}
			if (friendsToFind == null || friendsToFind.Length == 0)
			{
				DebugReturn(DebugLevel.ERROR, "OpFindFriends skipped: friendsToFind array is null or empty.");
				return false;
			}
			if (friendsToFind.Length > 512)
			{
				DebugReturn(DebugLevel.ERROR, string.Format("OpFindFriends skipped: friendsToFind array exceeds allowed length of {0}.", 512));
				return false;
			}
			List<string> list = new List<string>(friendsToFind.Length);
			for (int i = 0; i < friendsToFind.Length; i++)
			{
				string text = friendsToFind[i];
				if (string.IsNullOrEmpty(text))
				{
					DebugReturn(DebugLevel.WARNING, string.Format("friendsToFind array contains a null or empty UserId, element at position {0} skipped.", i));
				}
				else if (text.Equals(UserId))
				{
					DebugReturn(DebugLevel.WARNING, string.Format("friendsToFind array contains local player's UserId \"{0}\", element at position {1} skipped.", text, i));
				}
				else if (list.Contains(text))
				{
					DebugReturn(DebugLevel.WARNING, string.Format("friendsToFind array contains duplicate UserId \"{0}\", element at position {1} skipped.", text, i));
				}
				else
				{
					list.Add(text);
				}
			}
			if (list.Count == 0)
			{
				DebugReturn(DebugLevel.ERROR, "OpFindFriends skipped: friends list to find is empty.");
				return false;
			}
			string[] array = list.ToArray();
			bool flag = LoadBalancingPeer.OpFindFriends(array);
			friendListRequested = ((!flag) ? null : array);
			return flag;
		}

		public bool OpJoinLobby(TypedLobby lobby)
		{
			if (!IsConnectedAndReady || Server != ServerConnection.MasterServer)
			{
				DebugReturn(DebugLevel.ERROR, string.Concat("OpJoinLobby is only allowed when connected to a Master Server. Current Server: ", Server, ", current State: ", State));
				return false;
			}
			if (lobby == null)
			{
				lobby = TypedLobby.Default;
			}
			bool flag = LoadBalancingPeer.OpJoinLobby(lobby);
			if (flag)
			{
				CurrentLobby = lobby;
				State = ClientState.JoiningLobby;
			}
			return flag;
		}

		public bool OpLeaveLobby()
		{
			return LoadBalancingPeer.OpLeaveLobby();
		}

		public bool OpJoinRandomRoom(OpJoinRandomRoomParams opJoinRandomRoomParams = null)
		{
			if (opJoinRandomRoomParams == null)
			{
				opJoinRandomRoomParams = new OpJoinRandomRoomParams();
			}
			enterRoomParamsCache = new EnterRoomParams();
			enterRoomParamsCache.Lobby = opJoinRandomRoomParams.TypedLobby;
			enterRoomParamsCache.ExpectedUsers = opJoinRandomRoomParams.ExpectedUsers;
			bool flag = LoadBalancingPeer.OpJoinRandomRoom(opJoinRandomRoomParams);
			if (flag)
			{
				lastJoinType = JoinType.JoinRandomRoom;
				State = ClientState.Joining;
			}
			return flag;
		}

		public bool OpCreateRoom(EnterRoomParams enterRoomParams)
		{
			if (!(enterRoomParams.OnGameServer = Server == ServerConnection.GameServer))
			{
				enterRoomParamsCache = enterRoomParams;
			}
			bool flag = LoadBalancingPeer.OpCreateRoom(enterRoomParams);
			if (flag)
			{
				lastJoinType = JoinType.CreateRoom;
				State = ClientState.Joining;
			}
			return flag;
		}

		public bool OpJoinOrCreateRoom(EnterRoomParams enterRoomParams)
		{
			bool flag = Server == ServerConnection.GameServer;
			enterRoomParams.CreateIfNotExists = true;
			enterRoomParams.OnGameServer = flag;
			if (!flag)
			{
				enterRoomParamsCache = enterRoomParams;
			}
			bool flag2 = LoadBalancingPeer.OpJoinRoom(enterRoomParams);
			if (flag2)
			{
				lastJoinType = JoinType.JoinOrCreateRoom;
				State = ClientState.Joining;
			}
			return flag2;
		}

		public bool OpJoinRoom(EnterRoomParams enterRoomParams)
		{
			if (!(enterRoomParams.OnGameServer = Server == ServerConnection.GameServer))
			{
				enterRoomParamsCache = enterRoomParams;
			}
			bool flag = LoadBalancingPeer.OpJoinRoom(enterRoomParams);
			if (flag)
			{
				lastJoinType = ((!enterRoomParams.CreateIfNotExists) ? JoinType.JoinRoom : JoinType.JoinOrCreateRoom);
				State = ClientState.Joining;
			}
			return flag;
		}

		public bool OpRejoinRoom(string roomName)
		{
			bool onGameServer = Server == ServerConnection.GameServer;
			EnterRoomParams enterRoomParams = (enterRoomParamsCache = new EnterRoomParams());
			enterRoomParams.RoomName = roomName;
			enterRoomParams.OnGameServer = onGameServer;
			enterRoomParams.RejoinOnly = true;
			bool flag = LoadBalancingPeer.OpJoinRoom(enterRoomParams);
			if (flag)
			{
				lastJoinType = JoinType.JoinRoom;
				State = ClientState.Joining;
			}
			return flag;
		}

		public bool OpLeaveRoom(bool becomeInactive, bool sendAuthCookie = false)
		{
			if (CurrentRoom == null || Server != ServerConnection.GameServer || State == ClientState.DisconnectingFromGameserver)
			{
				return false;
			}
			State = ClientState.Leaving;
			return LoadBalancingPeer.OpLeaveRoom(becomeInactive, sendAuthCookie);
		}

		public bool OpGetGameList(TypedLobby typedLobby, string sqlLobbyFilter)
		{
			return LoadBalancingPeer.OpGetGameList(typedLobby, sqlLobbyFilter);
		}

		public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
		{
			if (CurrentRoom == null)
			{
				if (expectedProperties == null && webFlags == null && LocalPlayer != null && LocalPlayer.ActorNumber == actorNr)
				{
					LocalPlayer.SetCustomProperties(propertiesToSet);
					return true;
				}
				if ((int)LoadBalancingPeer.DebugOut >= 1)
				{
					DebugReturn(DebugLevel.ERROR, "OpSetCustomPropertiesOfActor() failed. To use expectedProperties or webForward, you have to be in a room. State: " + State);
				}
				return false;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.MergeStringKeys(propertiesToSet);
			return OpSetPropertiesOfActor(actorNr, hashtable, expectedProperties, webFlags);
		}

		protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, Hashtable expectedProperties = null, WebFlags webFlags = null)
		{
			if (CurrentRoom == null)
			{
				if ((int)LoadBalancingPeer.DebugOut >= 1)
				{
					DebugReturn(DebugLevel.ERROR, "OpSetPropertiesOfActor() failed because this client is not in a room currently. State: " + State);
				}
				return false;
			}
			if (expectedProperties == null || expectedProperties.Count == 0)
			{
				Player player = CurrentRoom.GetPlayer(actorNr);
				if (player != null)
				{
					player.InternalCacheProperties(actorProperties);
				}
			}
			return LoadBalancingPeer.OpSetPropertiesOfActor(actorNr, actorProperties, expectedProperties, webFlags);
		}

		public bool OpSetCustomPropertiesOfRoom(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.MergeStringKeys(propertiesToSet);
			return OpSetPropertiesOfRoom(hashtable, expectedProperties, webFlags);
		}

		protected internal void OpSetPropertyOfRoom(byte propCode, object value)
		{
			Hashtable hashtable = new Hashtable();
			hashtable[propCode] = value;
			OpSetPropertiesOfRoom(hashtable);
		}

		public bool OpSetPropertiesOfRoom(Hashtable gameProperties, Hashtable expectedProperties = null, WebFlags webFlags = null)
		{
			if (CurrentRoom == null)
			{
				if ((int)LoadBalancingPeer.DebugOut >= 1)
				{
					DebugReturn(DebugLevel.ERROR, "OpSetPropertiesOfRoom() failed because this client is not in a room currently. State: " + State);
				}
				return false;
			}
			if (expectedProperties == null || expectedProperties.Count == 0)
			{
				CurrentRoom.InternalCacheProperties(gameProperties);
			}
			return LoadBalancingPeer.OpSetPropertiesOfRoom(gameProperties, expectedProperties, webFlags);
		}

		public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
		{
			if (LoadBalancingPeer == null)
			{
				return false;
			}
			return LoadBalancingPeer.OpRaiseEvent(eventCode, customEventContent, raiseEventOptions, sendOptions);
		}

		public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
		{
			if (LoadBalancingPeer == null)
			{
				return false;
			}
			return LoadBalancingPeer.OpChangeGroups(groupsToRemove, groupsToAdd);
		}

		private void ReadoutProperties(Hashtable gameProperties, Hashtable actorProperties, int targetActorNr)
		{
			if (CurrentRoom != null && gameProperties != null)
			{
				CurrentRoom.InternalCacheProperties(gameProperties);
				InRoomCallbackTargets.OnRoomPropertiesUpdate(gameProperties);
			}
			if (actorProperties == null || actorProperties.Count <= 0)
			{
				return;
			}
			if (targetActorNr > 0)
			{
				Player player = CurrentRoom.GetPlayer(targetActorNr);
				if (player != null)
				{
					Hashtable hashtable = ReadoutPropertiesForActorNr(actorProperties, targetActorNr);
					player.InternalCacheProperties(hashtable);
					InRoomCallbackTargets.OnPlayerPropertiesUpdate(player, hashtable);
				}
				return;
			}
			foreach (object key in actorProperties.Keys)
			{
				int num = (int)key;
				Hashtable hashtable2 = (Hashtable)actorProperties[key];
				string actorName = (string)hashtable2[byte.MaxValue];
				Player player2 = CurrentRoom.GetPlayer(num);
				if (player2 == null)
				{
					player2 = CreatePlayer(actorName, num, false, hashtable2);
					CurrentRoom.StorePlayer(player2);
				}
				player2.InternalCacheProperties(hashtable2);
				InRoomCallbackTargets.OnPlayerPropertiesUpdate(player2, hashtable2);
			}
		}

		private Hashtable ReadoutPropertiesForActorNr(Hashtable actorProperties, int actorNr)
		{
			if (actorProperties.ContainsKey(actorNr))
			{
				return (Hashtable)actorProperties[actorNr];
			}
			return actorProperties;
		}

		public void ChangeLocalID(int newID)
		{
			if (LocalPlayer == null)
			{
				DebugReturn(DebugLevel.WARNING, string.Format("Local actor is null or not in mActors! mLocalActor: {0} mActors==null: {1} newID: {2}", LocalPlayer, CurrentRoom.Players == null, newID));
			}
			if (CurrentRoom == null)
			{
				LocalPlayer.ChangeLocalID(newID);
				LocalPlayer.RoomReference = null;
			}
			else
			{
				CurrentRoom.RemovePlayer(LocalPlayer);
				LocalPlayer.ChangeLocalID(newID);
				CurrentRoom.StorePlayer(LocalPlayer);
			}
		}

		private void GameEnteredOnGameServer(OperationResponse operationResponse)
		{
			CurrentRoom = CreateRoom(enterRoomParamsCache.RoomName, enterRoomParamsCache.RoomOptions);
			CurrentRoom.LoadBalancingClient = this;
			int newID = (int)operationResponse[254];
			ChangeLocalID(newID);
			if (operationResponse.Parameters.ContainsKey(252))
			{
				int[] actorsInGame = (int[])operationResponse.Parameters[252];
				UpdatedActorList(actorsInGame);
			}
			Hashtable actorProperties = (Hashtable)operationResponse[249];
			Hashtable gameProperties = (Hashtable)operationResponse[248];
			ReadoutProperties(gameProperties, actorProperties, 0);
			State = ClientState.Joined;
			switch (operationResponse.OperationCode)
			{
			case 227:
				MatchMakingCallbackTargets.OnCreatedRoom();
				break;
			}
		}

		private void UpdatedActorList(int[] actorsInGame)
		{
			if (actorsInGame == null)
			{
				return;
			}
			foreach (int num in actorsInGame)
			{
				Player player = CurrentRoom.GetPlayer(num);
				if (player == null)
				{
					CurrentRoom.StorePlayer(CreatePlayer(string.Empty, num, false, null));
				}
			}
		}

		protected internal virtual Player CreatePlayer(string actorName, int actorNumber, bool isLocal, Hashtable actorProperties)
		{
			return new Player(actorName, actorNumber, isLocal, actorProperties);
		}

		protected internal virtual Room CreateRoom(string roomName, RoomOptions opt)
		{
			return new Room(roomName, opt);
		}

		public virtual void DebugReturn(DebugLevel level, string message)
		{
			if (LoadBalancingPeer.DebugOut == DebugLevel.ALL || (int)level <= (int)LoadBalancingPeer.DebugOut)
			{
				switch (level)
				{
				case DebugLevel.ERROR:
					UnityEngine.Debug.LogError(message);
					break;
				case DebugLevel.WARNING:
					UnityEngine.Debug.LogWarning(message);
					break;
				case DebugLevel.INFO:
					UnityEngine.Debug.Log(message);
					break;
				case DebugLevel.ALL:
					UnityEngine.Debug.Log(message);
					break;
				}
			}
		}

		private void CallbackRoomEnterFailed(OperationResponse operationResponse)
		{
			if (operationResponse.ReturnCode != 0)
			{
				if (operationResponse.OperationCode == 226)
				{
					MatchMakingCallbackTargets.OnJoinRoomFailed(operationResponse.ReturnCode, operationResponse.DebugMessage);
				}
				else if (operationResponse.OperationCode == 227)
				{
					MatchMakingCallbackTargets.OnCreateRoomFailed(operationResponse.ReturnCode, operationResponse.DebugMessage);
				}
				else if (operationResponse.OperationCode == 225)
				{
					MatchMakingCallbackTargets.OnJoinRandomFailed(operationResponse.ReturnCode, operationResponse.DebugMessage);
				}
			}
		}

		public virtual void OnOperationResponse(OperationResponse operationResponse)
		{
			if (operationResponse.Parameters.ContainsKey(221))
			{
				if (AuthValues == null)
				{
					AuthValues = new AuthenticationValues();
				}
				AuthValues.Token = operationResponse[221] as string;
				tokenCache = AuthValues.Token;
			}
			switch (operationResponse.OperationCode)
			{
			case 230:
			case 231:
			{
				if (operationResponse.ReturnCode != 0)
				{
					DebugReturn(DebugLevel.ERROR, string.Concat(operationResponse.ToStringFull(), " Server: ", Server, " Address: ", LoadBalancingPeer.ServerAddress));
					switch (operationResponse.ReturnCode)
					{
					case short.MaxValue:
						DisconnectedCause = DisconnectCause.InvalidAuthentication;
						ConnectionCallbackTargets.OnDisconnected(DisconnectCause.InvalidAuthentication);
						break;
					case 32755:
						DisconnectedCause = DisconnectCause.CustomAuthenticationFailed;
						ConnectionCallbackTargets.OnCustomAuthenticationFailed(operationResponse.DebugMessage);
						break;
					case 32756:
						DisconnectedCause = DisconnectCause.InvalidRegion;
						ConnectionCallbackTargets.OnDisconnected(DisconnectCause.InvalidRegion);
						break;
					case 32757:
						DisconnectedCause = DisconnectCause.MaxCcuReached;
						ConnectionCallbackTargets.OnDisconnected(DisconnectCause.MaxCcuReached);
						break;
					case -3:
						DisconnectedCause = DisconnectCause.OperationNotAllowedInCurrentState;
						break;
					case 32753:
						DisconnectedCause = DisconnectCause.AuthenticationTicketExpired;
						ConnectionCallbackTargets.OnDisconnected(DisconnectCause.AuthenticationTicketExpired);
						break;
					}
					State = ClientState.Disconnecting;
					Disconnect();
					break;
				}
				if (Server == ServerConnection.NameServer || Server == ServerConnection.MasterServer)
				{
					if (operationResponse.Parameters.ContainsKey(225))
					{
						string text3 = (string)operationResponse.Parameters[225];
						if (!string.IsNullOrEmpty(text3))
						{
							UserId = text3;
							LocalPlayer.UserId = text3;
							DebugReturn(DebugLevel.INFO, string.Format("Received your UserID from server. Updating local value to: {0}", UserId));
						}
					}
					if (operationResponse.Parameters.ContainsKey(202))
					{
						NickName = (string)operationResponse.Parameters[202];
						DebugReturn(DebugLevel.INFO, string.Format("Received your NickName from server. Updating local value to: {0}", NickName));
					}
					if (operationResponse.Parameters.ContainsKey(192))
					{
						SetupEncryption((Dictionary<byte, object>)operationResponse.Parameters[192]);
					}
				}
				if (Server == ServerConnection.NameServer)
				{
					MasterServerAddress = operationResponse[230] as string;
					if (LoadBalancingPeer.TransportProtocol == ConnectionProtocol.Udp && UseAlternativeUdpPorts)
					{
						MasterServerAddress = MasterServerAddress.Replace("5058", "27000").Replace("5055", "27001").Replace("5056", "27002");
					}
					if (AuthMode == AuthModeOption.AuthOnceWss)
					{
						DebugReturn(DebugLevel.INFO, string.Format("Due to AuthOnceWss, switching TransportProtocol to ExpectedProtocol: {0}.", ExpectedProtocol));
						LoadBalancingPeer.TransportProtocol = ExpectedProtocol;
					}
					DisconnectToReconnect();
				}
				else if (Server == ServerConnection.MasterServer)
				{
					State = ClientState.ConnectedToMasterserver;
					if (failedRoomEntryOperation == null)
					{
						ConnectionCallbackTargets.OnConnectedToMaster();
					}
					else
					{
						CallbackRoomEnterFailed(failedRoomEntryOperation);
						failedRoomEntryOperation = null;
					}
					if (AuthMode != AuthModeOption.Auth)
					{
						LoadBalancingPeer.OpSettings(EnableLobbyStatistics);
					}
				}
				else if (Server == ServerConnection.GameServer)
				{
					State = ClientState.Joining;
					if (enterRoomParamsCache.RejoinOnly)
					{
						enterRoomParamsCache.PlayerProperties = null;
					}
					else
					{
						Hashtable hashtable2 = new Hashtable();
						hashtable2.Merge(LocalPlayer.CustomProperties);
						hashtable2[byte.MaxValue] = LocalPlayer.NickName;
						enterRoomParamsCache.PlayerProperties = hashtable2;
					}
					enterRoomParamsCache.OnGameServer = true;
					if (lastJoinType == JoinType.JoinRoom || lastJoinType == JoinType.JoinRandomRoom || lastJoinType == JoinType.JoinOrCreateRoom)
					{
						LoadBalancingPeer.OpJoinRoom(enterRoomParamsCache);
					}
					else if (lastJoinType == JoinType.CreateRoom)
					{
						LoadBalancingPeer.OpCreateRoom(enterRoomParamsCache);
					}
					break;
				}
				Dictionary<string, object> dictionary = (Dictionary<string, object>)operationResponse[245];
				if (dictionary != null)
				{
					ConnectionCallbackTargets.OnCustomAuthenticationResponse(dictionary);
				}
				break;
			}
			case 220:
				if (operationResponse.ReturnCode == short.MaxValue)
				{
					DebugReturn(DebugLevel.ERROR, string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account."));
					ConnectionCallbackTargets.OnCustomAuthenticationFailed("Invalid Authentication");
					State = ClientState.Disconnecting;
					Disconnect();
					break;
				}
				if (operationResponse.ReturnCode != 0)
				{
					DebugReturn(DebugLevel.ERROR, "GetRegions failed. Can't provide regions list. Error: " + operationResponse.ReturnCode + ": " + operationResponse.DebugMessage);
					break;
				}
				if (RegionHandler == null)
				{
					RegionHandler = new RegionHandler();
				}
				if (RegionHandler.IsPinging)
				{
					DebugReturn(DebugLevel.WARNING, "Received an response for OpGetRegions while the RegionHandler is pinging regions already. Skipping this response in favor of completing the current region-pinging.");
					return;
				}
				RegionHandler.SetRegions(operationResponse);
				ConnectionCallbackTargets.OnRegionListReceived(RegionHandler);
				break;
			case 225:
			case 226:
			case 227:
			{
				if (operationResponse.ReturnCode != 0)
				{
					if (Server == ServerConnection.GameServer)
					{
						failedRoomEntryOperation = operationResponse;
						DisconnectToReconnect();
					}
					else
					{
						State = ((!InLobby) ? ClientState.ConnectedToMasterserver : ClientState.JoinedLobby);
						CallbackRoomEnterFailed(operationResponse);
					}
					break;
				}
				if (Server == ServerConnection.GameServer)
				{
					GameEnteredOnGameServer(operationResponse);
					break;
				}
				GameServerAddress = (string)operationResponse[230];
				if (LoadBalancingPeer.TransportProtocol == ConnectionProtocol.Udp && UseAlternativeUdpPorts)
				{
					GameServerAddress = GameServerAddress.Replace("5058", "27000").Replace("5055", "27001").Replace("5056", "27002");
				}
				string text2 = operationResponse[byte.MaxValue] as string;
				if (!string.IsNullOrEmpty(text2))
				{
					enterRoomParamsCache.RoomName = text2;
				}
				DisconnectToReconnect();
				break;
			}
			case 217:
			{
				if (operationResponse.ReturnCode != 0)
				{
					DebugReturn(DebugLevel.ERROR, "GetGameList failed: " + operationResponse.ToStringFull());
					break;
				}
				List<RoomInfo> list2 = new List<RoomInfo>();
				Hashtable hashtable = (Hashtable)operationResponse[222];
				foreach (string key in hashtable.Keys)
				{
					list2.Add(new RoomInfo(key, (Hashtable)hashtable[key]));
				}
				LobbyCallbackTargets.OnRoomListUpdate(list2);
				break;
			}
			case 229:
				State = ClientState.JoinedLobby;
				InLobby = true;
				LobbyCallbackTargets.OnJoinedLobby();
				break;
			case 228:
				State = ClientState.ConnectedToMasterserver;
				InLobby = false;
				LobbyCallbackTargets.OnLeftLobby();
				break;
			case 254:
				DisconnectToReconnect();
				break;
			case 222:
			{
				if (operationResponse.ReturnCode != 0)
				{
					DebugReturn(DebugLevel.ERROR, "OpFindFriends failed: " + operationResponse.ToStringFull());
					friendListRequested = null;
					break;
				}
				bool[] array = operationResponse[1] as bool[];
				string[] array2 = operationResponse[2] as string[];
				List<FriendInfo> list = new List<FriendInfo>(friendListRequested.Length);
				for (int i = 0; i < friendListRequested.Length; i++)
				{
					FriendInfo friendInfo = new FriendInfo();
					friendInfo.UserId = friendListRequested[i];
					friendInfo.Room = array2[i];
					friendInfo.IsOnline = array[i];
					list.Insert(i, friendInfo);
				}
				friendListRequested = null;
				MatchMakingCallbackTargets.OnFriendListUpdate(list);
				break;
			}
			case 219:
				WebRpcCallbackTargets.OnWebRpcResponse(operationResponse);
				break;
			}
			if (OpResponseReceived__BackingField != null)
			{
				OpResponseReceived__BackingField(operationResponse);
			}
		}

		public virtual void OnStatusChanged(StatusCode statusCode)
		{
			switch (statusCode)
			{
			case StatusCode.Connect:
				InLobby = false;
				if (State == ClientState.ConnectingToNameServer)
				{
					if ((int)LoadBalancingPeer.DebugOut >= 5)
					{
						DebugReturn(DebugLevel.ALL, "Connected to nameserver.");
					}
					Server = ServerConnection.NameServer;
					if (AuthValues != null)
					{
						AuthValues.Token = null;
					}
				}
				if (State == ClientState.ConnectingToGameserver)
				{
					if ((int)LoadBalancingPeer.DebugOut >= 5)
					{
						DebugReturn(DebugLevel.ALL, "Connected to gameserver.");
					}
					Server = ServerConnection.GameServer;
				}
				if (State == ClientState.ConnectingToMasterserver)
				{
					if ((int)LoadBalancingPeer.DebugOut >= 5)
					{
						DebugReturn(DebugLevel.ALL, "Connected to masterserver.");
					}
					Server = ServerConnection.MasterServer;
					ConnectionCallbackTargets.OnConnected();
				}
				if (LoadBalancingPeer.TransportProtocol != ConnectionProtocol.WebSocketSecure)
				{
					if (Server == ServerConnection.NameServer || AuthMode == AuthModeOption.Auth)
					{
						LoadBalancingPeer.EstablishEncryption();
					}
					break;
				}
				goto case StatusCode.EncryptionEstablished;
			case StatusCode.EncryptionEstablished:
				if (Server == ServerConnection.NameServer)
				{
					State = ClientState.ConnectedToNameServer;
					if (!didAuthenticate && string.IsNullOrEmpty(CloudRegion))
					{
						OpGetRegions();
					}
				}
				if ((Server == ServerConnection.NameServer || (AuthMode != AuthModeOption.AuthOnce && AuthMode != AuthModeOption.AuthOnceWss)) && !didAuthenticate && (!IsUsingNameServer || !string.IsNullOrEmpty(CloudRegion)))
				{
					didAuthenticate = CallAuthenticate();
					if (didAuthenticate)
					{
						State = ClientState.Authenticating;
					}
					else
					{
						DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected. State: " + State);
					}
				}
				break;
			case StatusCode.Disconnect:
			{
				ChangeLocalID(-1);
				friendListRequested = null;
				bool flag = CurrentRoom != null;
				if (Server == ServerConnection.GameServer || State == ClientState.Disconnecting || State == ClientState.PeerCreated)
				{
					CurrentRoom = null;
				}
				didAuthenticate = false;
				InLobby = false;
				if (Server == ServerConnection.GameServer && flag)
				{
					MatchMakingCallbackTargets.OnLeftRoom();
				}
				switch (State)
				{
				case ClientState.PeerCreated:
				case ClientState.Disconnecting:
					if (AuthValues != null)
					{
						AuthValues.Token = null;
					}
					State = ClientState.Disconnected;
					ConnectionCallbackTargets.OnDisconnected(DisconnectCause.DisconnectByClientLogic);
					break;
				case ClientState.DisconnectingFromGameserver:
				case ClientState.DisconnectingFromNameServer:
					Connect();
					break;
				case ClientState.DisconnectingFromMasterserver:
					ConnectToGameServer();
					break;
				case ClientState.Disconnected:
					break;
				default:
				{
					string empty = string.Empty;
					DebugReturn(DebugLevel.WARNING, string.Concat("Got a unexpected Disconnect in LoadBalancingClient State: ", State, ". Server: ", Server, " Trace: ", empty));
					if (AuthValues != null)
					{
						AuthValues.Token = null;
					}
					State = ClientState.Disconnected;
					ConnectionCallbackTargets.OnDisconnected(DisconnectCause.None);
					break;
				}
				}
				break;
			}
			case StatusCode.DisconnectByServerUserLimit:
				DebugReturn(DebugLevel.ERROR, "The Photon license's CCU Limit was reached. Server rejected this connection. Wait and re-try.");
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.MaxCcuReached;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case StatusCode.SecurityExceptionOnConnect:
			case StatusCode.ExceptionOnConnect:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.ExceptionOnConnect;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case StatusCode.DisconnectByServer:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.ServerTimeout;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case StatusCode.DisconnectByServerLogic:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.DisconnectByServerLogic;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case StatusCode.DisconnectByServerReasonUnknown:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.DisconnectByServerReasonUnknown;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case StatusCode.TimeoutDisconnect:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.ClientTimeout;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			case StatusCode.Exception:
			case StatusCode.ExceptionOnReceive:
				if (AuthValues != null)
				{
					AuthValues.Token = null;
				}
				DisconnectedCause = DisconnectCause.Exception;
				State = ClientState.Disconnected;
				ConnectionCallbackTargets.OnDisconnected(DisconnectedCause);
				break;
			}
		}

		public virtual void OnEvent(EventData photonEvent)
		{
			int sender = photonEvent.Sender;
			Player player = ((CurrentRoom == null) ? null : CurrentRoom.GetPlayer(sender));
			switch (photonEvent.Code)
			{
			case 229:
			case 230:
			{
				List<RoomInfo> list = new List<RoomInfo>();
				Hashtable hashtable2 = (Hashtable)photonEvent[222];
				foreach (string key in hashtable2.Keys)
				{
					list.Add(new RoomInfo(key, (Hashtable)hashtable2[key]));
				}
				LobbyCallbackTargets.OnRoomListUpdate(list);
				break;
			}
			case byte.MaxValue:
			{
				Hashtable hashtable = (Hashtable)photonEvent[249];
				if (player == null)
				{
					player = CreatePlayer(string.Empty, sender, false, hashtable);
					CurrentRoom.StorePlayer(player);
				}
				else
				{
					player.InternalCacheProperties(hashtable);
					player.IsInactive = false;
				}
				if (sender == LocalPlayer.ActorNumber)
				{
					int[] actorsInGame = (int[])photonEvent[252];
					UpdatedActorList(actorsInGame);
					if (lastJoinType == JoinType.JoinOrCreateRoom && LocalPlayer.ActorNumber == 1)
					{
						MatchMakingCallbackTargets.OnCreatedRoom();
					}
					MatchMakingCallbackTargets.OnJoinedRoom();
				}
				else
				{
					InRoomCallbackTargets.OnPlayerEnteredRoom(player);
				}
				break;
			}
			case 254:
			{
				bool flag = false;
				if (photonEvent.Parameters.ContainsKey(233))
				{
					flag = (bool)photonEvent.Parameters[233];
				}
				if (flag)
				{
					player.IsInactive = true;
				}
				else
				{
					CurrentRoom.RemovePlayer(sender);
				}
				if (photonEvent.Parameters.ContainsKey(203))
				{
					int num = (int)photonEvent[203];
					if (num != 0)
					{
						CurrentRoom.masterClientId = num;
						InRoomCallbackTargets.OnMasterClientSwitched(CurrentRoom.GetPlayer(num));
					}
				}
				InRoomCallbackTargets.OnPlayerLeftRoom(player);
				break;
			}
			case 253:
			{
				int num2 = 0;
				if (photonEvent.Parameters.ContainsKey(253))
				{
					num2 = (int)photonEvent[253];
				}
				Hashtable gameProperties = null;
				Hashtable actorProperties = null;
				if (num2 == 0)
				{
					gameProperties = (Hashtable)photonEvent[251];
				}
				else
				{
					actorProperties = (Hashtable)photonEvent[251];
				}
				ReadoutProperties(gameProperties, actorProperties, num2);
				break;
			}
			case 226:
				PlayersInRoomsCount = (int)photonEvent[229];
				RoomsCount = (int)photonEvent[228];
				PlayersOnMasterCount = (int)photonEvent[227];
				break;
			case 224:
			{
				string[] array = photonEvent[213] as string[];
				byte[] array2 = photonEvent[212] as byte[];
				int[] array3 = photonEvent[229] as int[];
				int[] array4 = photonEvent[228] as int[];
				lobbyStatistics.Clear();
				for (int i = 0; i < array.Length; i++)
				{
					TypedLobbyInfo typedLobbyInfo = new TypedLobbyInfo();
					typedLobbyInfo.Name = array[i];
					typedLobbyInfo.Type = (LobbyType)array2[i];
					typedLobbyInfo.PlayerCount = array3[i];
					typedLobbyInfo.RoomCount = array4[i];
					lobbyStatistics.Add(typedLobbyInfo);
				}
				LobbyCallbackTargets.OnLobbyStatisticsUpdate(lobbyStatistics);
				break;
			}
			case 223:
				if (AuthValues == null)
				{
					AuthValues = new AuthenticationValues();
				}
				AuthValues.Token = photonEvent[221] as string;
				tokenCache = AuthValues.Token;
				break;
			}
			if (EventReceived__BackingField != null)
			{
				EventReceived__BackingField(photonEvent);
			}
		}

		public virtual void OnMessage(object message)
		{
			DebugReturn(DebugLevel.ALL, string.Format("got OnMessage {0}", message));
		}

		private void SetupEncryption(Dictionary<byte, object> encryptionData)
		{
			EncryptionMode encryptionMode = (EncryptionMode)(byte)encryptionData[0];
			switch (encryptionMode)
			{
			case EncryptionMode.PayloadEncryption:
			{
				byte[] secret = (byte[])encryptionData[1];
				LoadBalancingPeer.InitPayloadEncryption(secret);
				break;
			}
			case EncryptionMode.DatagramEncryption:
			case EncryptionMode.DatagramEncryptionRandomSequence:
			{
				byte[] encryptionSecret = (byte[])encryptionData[1];
				byte[] hmacSecret = (byte[])encryptionData[2];
				LoadBalancingPeer.InitDatagramEncryption(encryptionSecret, hmacSecret, encryptionMode == EncryptionMode.DatagramEncryptionRandomSequence);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public bool OpWebRpc(string uriPath, object parameters, bool sendAuthCookie = false)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(209, uriPath);
			dictionary.Add(208, parameters);
			if (sendAuthCookie)
			{
				dictionary.Add(234, (byte)2);
			}
			return LoadBalancingPeer.SendOperation(219, dictionary, SendOptions.SendReliable);
		}

		public void AddCallbackTarget(object target)
		{
			IInRoomCallbacks inRoomCallbacks = target as IInRoomCallbacks;
			if (inRoomCallbacks != null)
			{
				InRoomCallbackTargets.AddCallbackTarget(inRoomCallbacks);
			}
			IConnectionCallbacks connectionCallbacks = target as IConnectionCallbacks;
			if (connectionCallbacks != null)
			{
				ConnectionCallbackTargets.AddCallbackTarget(connectionCallbacks);
			}
			IMatchmakingCallbacks matchmakingCallbacks = target as IMatchmakingCallbacks;
			if (matchmakingCallbacks != null)
			{
				MatchMakingCallbackTargets.AddCallbackTarget(matchmakingCallbacks);
			}
			ILobbyCallbacks lobbyCallbacks = target as ILobbyCallbacks;
			if (lobbyCallbacks != null)
			{
				LobbyCallbackTargets.AddCallbackTarget(lobbyCallbacks);
			}
			IOnEventCallback onEventCallback = target as IOnEventCallback;
			if (onEventCallback != null)
			{
				EventReceived += onEventCallback.OnEvent;
			}
			IWebRpcCallback webRpcCallback = target as IWebRpcCallback;
			if (webRpcCallback != null)
			{
				WebRpcCallbackTargets.AddCallbackTarget(webRpcCallback);
			}
		}

		public void RemoveCallbackTarget(object target)
		{
			IInRoomCallbacks inRoomCallbacks = target as IInRoomCallbacks;
			if (inRoomCallbacks != null)
			{
				InRoomCallbackTargets.RemoveCallbackTarget(inRoomCallbacks);
			}
			IConnectionCallbacks connectionCallbacks = target as IConnectionCallbacks;
			if (connectionCallbacks != null)
			{
				ConnectionCallbackTargets.RemoveCallbackTarget(connectionCallbacks);
			}
			IMatchmakingCallbacks matchmakingCallbacks = target as IMatchmakingCallbacks;
			if (matchmakingCallbacks != null)
			{
				MatchMakingCallbackTargets.RemoveCallbackTarget(matchmakingCallbacks);
			}
			ILobbyCallbacks lobbyCallbacks = target as ILobbyCallbacks;
			if (lobbyCallbacks != null)
			{
				LobbyCallbackTargets.RemoveCallbackTarget(lobbyCallbacks);
			}
			IOnEventCallback onEventCallback = target as IOnEventCallback;
			if (onEventCallback != null)
			{
				EventReceived -= onEventCallback.OnEvent;
			}
			IWebRpcCallback webRpcCallback = target as IWebRpcCallback;
			if (webRpcCallback != null)
			{
				WebRpcCallbackTargets.RemoveCallbackTarget(webRpcCallback);
			}
		}
	}
}
