using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun
{
	public static class PhotonNetwork
	{
		private struct RaiseEventBatch : IEquatable<RaiseEventBatch>
		{
			public byte Group;

			public bool Reliable;

			public override int GetHashCode()
			{
				return (Group << 1) + (Reliable ? 1 : 0);
			}

			public bool Equals(RaiseEventBatch other)
			{
				return Reliable == other.Reliable && Group == other.Group;
			}
		}

		private class SerializeViewBatch : IEquatable<SerializeViewBatch>, IEquatable<RaiseEventBatch>
		{
			public readonly RaiseEventBatch Batch;

			public List<object> ObjectUpdates;

			private int defaultSize = 20;

			private int offset;

			public SerializeViewBatch(RaiseEventBatch batch, int offset)
			{
				Batch = batch;
				ObjectUpdates = new List<object>(defaultSize);
				this.offset = offset;
				for (int i = 0; i < offset; i++)
				{
					ObjectUpdates.Add(null);
				}
			}

			public override int GetHashCode()
			{
				RaiseEventBatch batch = Batch;
				int num = batch.Group << 1;
				RaiseEventBatch batch2 = Batch;
				return num + (batch2.Reliable ? 1 : 0);
			}

			public bool Equals(SerializeViewBatch other)
			{
				return Equals(other.Batch);
			}

			public bool Equals(RaiseEventBatch other)
			{
				RaiseEventBatch batch = Batch;
				int result;
				if (batch.Reliable == other.Reliable)
				{
					RaiseEventBatch batch2 = Batch;
					result = ((batch2.Group == other.Group) ? 1 : 0);
				}
				else
				{
					result = 0;
				}
				return (byte)result != 0;
			}

			public override bool Equals(object obj)
			{
				SerializeViewBatch serializeViewBatch = obj as SerializeViewBatch;
				return serializeViewBatch != null && Batch.Equals(serializeViewBatch.Batch);
			}

			public void Clear()
			{
				ObjectUpdates.Clear();
				for (int i = 0; i < offset; i++)
				{
					ObjectUpdates.Add(null);
				}
			}

			public void Add(List<object> viewData)
			{
				if (ObjectUpdates.Count >= ObjectUpdates.Capacity)
				{
					throw new Exception("Can't add. Size exceeded.");
				}
				ObjectUpdates.Add(viewData);
			}
		}

		public const string PunVersion = "2.7";

		private static string gameVersion;

		private static readonly PhotonHandler photonMono;

		public static LoadBalancingClient NetworkingClient;

		public static readonly int MAX_VIEW_IDS;

		internal const string ServerSettingsFileName = "PhotonServerSettings";

		public static ServerSettings PhotonServerSettings;

		private const string PlayerPrefsKey = "PUNCloudBestRegion";

		public static ConnectMethod ConnectMethod;

		public static PunLogLevel LogLevel;

		public static float PrecisionForVectorSynchronization;

		public static float PrecisionForQuaternionSynchronization;

		public static float PrecisionForFloatSynchronization;

		private static bool offlineMode;

		private static Room offlineModeRoom;

		private static bool automaticallySyncScene;

		private static int sendFrequency;

		private static int serializationFrequency;

		private static bool isMessageQueueRunning;

		private static double frametime;

		private static int frame;

		private static readonly Stopwatch StartupStopwatch;

		private static int lastUsedViewSubId;

		private static int lastUsedViewSubIdStatic;

		private static readonly HashSet<string> PrefabsWithoutMagicCallback;

		private static readonly Hashtable SendInstantiateEvHashtable;

		private static readonly RaiseEventOptions SendInstantiateRaiseEventOptions;

		private static HashSet<byte> allowedReceivingGroups;

		private static HashSet<byte> blockedSendingGroups;

		private static Dictionary<int, PhotonView> photonViewList;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<PhotonView, Player> OnOwnershipRequestEv__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<PhotonView, Player> OnOwnershipTransferedEv__BackingField;

		internal static byte currentLevelPrefix;

		internal static bool loadingLevelAndPausedNetwork;

		internal const string CurrentSceneProperty = "curScn";

		internal const string CurrentScenePropertyLoadAsync = "curScnLa";

		private static IPunPrefabPool prefabPool;

		public static bool UseRpcMonoBehaviourCache;

		private static readonly Dictionary<Type, List<MethodInfo>> monoRPCMethodsCache;

		private static readonly Dictionary<string, int> rpcShortcuts;

		private static AsyncOperation _AsyncLevelLoadingOperation;

		private static float _levelLoadingProgress;

		private static readonly Hashtable removeFilter;

		private static readonly Hashtable ServerCleanDestroyEvent;

		private static readonly RaiseEventOptions ServerCleanOptions;

		private static readonly Hashtable rpcFilterByViewId;

		private static readonly RaiseEventOptions OpCleanRpcBufferOptions;

		private static Hashtable rpcEvent;

		private static RaiseEventOptions RpcOptionsToAll;

		public static int ObjectsInOneUpdate;

		private static readonly PhotonStream serializeStreamOut;

		private static readonly PhotonStream serializeStreamIn;

		private static RaiseEventOptions serializeRaiseEvOptions;

		private static readonly Dictionary<RaiseEventBatch, SerializeViewBatch> serializeViewBatches;

		public const int SyncViewId = 0;

		public const int SyncCompressed = 1;

		public const int SyncNullValues = 2;

		public const int SyncFirstValue = 3;

		private static RegionHandler _cachedRegionHandler;

		public static string GameVersion
		{
			get
			{
				return gameVersion;
			}
			set
			{
				gameVersion = value;
				NetworkingClient.AppVersion = string.Format("{0}_{1}", value, "2.7");
			}
		}

		public static string AppVersion
		{
			get
			{
				return NetworkingClient.AppVersion;
			}
		}

		public static string ServerAddress
		{
			get
			{
				return (NetworkingClient == null) ? "<not connected>" : NetworkingClient.CurrentServerAddress;
			}
		}

		public static string CloudRegion
		{
			get
			{
				return (NetworkingClient == null || !IsConnected || Server == ServerConnection.NameServer) ? null : NetworkingClient.CloudRegion;
			}
		}

		public static string BestRegionSummaryInPreferences
		{
			get
			{
				return PlayerPrefs.GetString("PUNCloudBestRegion", null);
			}
			internal set
			{
				if (string.IsNullOrEmpty(value))
				{
					PlayerPrefs.DeleteKey("PUNCloudBestRegion");
				}
				else
				{
					PlayerPrefs.SetString("PUNCloudBestRegion", value.ToString());
				}
			}
		}

		public static bool IsConnected
		{
			get
			{
				if (OfflineMode)
				{
					return true;
				}
				if (NetworkingClient == null)
				{
					return false;
				}
				return NetworkingClient.IsConnected;
			}
		}

		public static bool IsConnectedAndReady
		{
			get
			{
				if (OfflineMode)
				{
					return true;
				}
				if (NetworkingClient == null)
				{
					return false;
				}
				return NetworkingClient.IsConnectedAndReady;
			}
		}

		public static ClientState NetworkClientState
		{
			get
			{
				if (OfflineMode)
				{
					return (offlineModeRoom == null) ? ClientState.ConnectedToMasterserver : ClientState.Joined;
				}
				if (NetworkingClient == null)
				{
					return ClientState.Disconnected;
				}
				return NetworkingClient.State;
			}
		}

		public static ServerConnection Server
		{
			get
			{
				return (NetworkingClient == null) ? ServerConnection.NameServer : NetworkingClient.Server;
			}
		}

		public static AuthenticationValues AuthValues
		{
			get
			{
				return (NetworkingClient == null) ? null : NetworkingClient.AuthValues;
			}
			set
			{
				if (NetworkingClient != null)
				{
					NetworkingClient.AuthValues = value;
				}
			}
		}

		public static TypedLobby CurrentLobby
		{
			get
			{
				return NetworkingClient.CurrentLobby;
			}
		}

		public static Room CurrentRoom
		{
			get
			{
				if (offlineMode)
				{
					return offlineModeRoom;
				}
				return (NetworkingClient != null) ? NetworkingClient.CurrentRoom : null;
			}
		}

		public static Player LocalPlayer
		{
			get
			{
				if (NetworkingClient == null)
				{
					return null;
				}
				return NetworkingClient.LocalPlayer;
			}
		}

		public static string NickName
		{
			get
			{
				return NetworkingClient.NickName;
			}
			set
			{
				NetworkingClient.NickName = value;
			}
		}

		public static Player[] PlayerList
		{
			get
			{
				Room currentRoom = CurrentRoom;
				if (currentRoom != null)
				{
					return currentRoom.Players.Values.OrderBy((Player x) => x.ActorNumber).ToArray();
				}
				return new Player[0];
			}
		}

		public static Player[] PlayerListOthers
		{
			get
			{
				Room currentRoom = CurrentRoom;
				if (currentRoom != null)
				{
					return (from x in currentRoom.Players.Values
						orderby x.ActorNumber
						where !x.IsLocal
						select x).ToArray();
				}
				return new Player[0];
			}
		}

		public static bool OfflineMode
		{
			get
			{
				return offlineMode;
			}
			set
			{
				if (value == offlineMode)
				{
					return;
				}
				if (value && IsConnected)
				{
					UnityEngine.Debug.LogError("Can't start OFFLINE mode while connected!");
					return;
				}
				if (NetworkingClient.IsConnected)
				{
					NetworkingClient.Disconnect();
				}
				offlineMode = value;
				if (offlineMode)
				{
					NetworkingClient.ChangeLocalID(-1);
					NetworkingClient.ConnectionCallbackTargets.OnConnectedToMaster();
					return;
				}
				if (offlineModeRoom != null)
				{
					LeftRoomCleanup();
				}
				offlineModeRoom = null;
				NetworkingClient.ChangeLocalID(-1);
			}
		}

		public static bool AutomaticallySyncScene
		{
			get
			{
				return automaticallySyncScene;
			}
			set
			{
				automaticallySyncScene = value;
				if (automaticallySyncScene && CurrentRoom != null)
				{
					LoadLevelIfSynced();
				}
			}
		}

		public static bool EnableLobbyStatistics
		{
			get
			{
				return NetworkingClient.EnableLobbyStatistics;
			}
		}

		public static bool InLobby
		{
			get
			{
				return NetworkingClient.InLobby;
			}
		}

		public static int SendRate
		{
			get
			{
				return 1000 / sendFrequency;
			}
			set
			{
				sendFrequency = 1000 / value;
				if (photonMono != null)
				{
					photonMono.UpdateInterval = sendFrequency;
				}
				if (value < SerializationRate)
				{
					SerializationRate = value;
				}
			}
		}

		public static int SerializationRate
		{
			get
			{
				return 1000 / serializationFrequency;
			}
			set
			{
				if (value > SendRate)
				{
					UnityEngine.Debug.LogError("Error: Can not set the OnSerialize rate higher than the overall SendRate.");
					value = SendRate;
				}
				serializationFrequency = 1000 / value;
				if (photonMono != null)
				{
					photonMono.UpdateIntervalOnSerialize = serializationFrequency;
				}
			}
		}

		public static bool IsMessageQueueRunning
		{
			get
			{
				return isMessageQueueRunning;
			}
			set
			{
				NetworkingClient.LoadBalancingPeer.IsSendingOnlyAcks = !value;
				isMessageQueueRunning = value;
			}
		}

		public static double Time
		{
			get
			{
				if (UnityEngine.Time.frameCount == frame)
				{
					return frametime;
				}
				uint serverTimestamp = (uint)ServerTimestamp;
				double num = serverTimestamp;
				frametime = num / 1000.0;
				frame = UnityEngine.Time.frameCount;
				return frametime;
			}
		}

		public static int ServerTimestamp
		{
			get
			{
				if (OfflineMode)
				{
					if (StartupStopwatch != null && StartupStopwatch.IsRunning)
					{
						return (int)StartupStopwatch.ElapsedMilliseconds;
					}
					return Environment.TickCount;
				}
				return NetworkingClient.LoadBalancingPeer.ServerTimeInMilliSeconds;
			}
		}

		public static float KeepAliveInBackground
		{
			get
			{
				return (!(PhotonHandler.Instance != null)) ? 60f : Mathf.Round((float)PhotonHandler.Instance.KeepAliveInBackground / 1000f);
			}
			set
			{
				if (PhotonHandler.Instance != null)
				{
					PhotonHandler.Instance.KeepAliveInBackground = (int)Mathf.Round(value * 1000f);
				}
			}
		}

		[Obsolete("Use KeepAliveInBackground instead.")]
		public static float BackgroundTimeout
		{
			get
			{
				return KeepAliveInBackground;
			}
			set
			{
				KeepAliveInBackground = value;
			}
		}

		public static bool IsMasterClient
		{
			get
			{
				if (OfflineMode)
				{
					return true;
				}
				return NetworkingClient.CurrentRoom != null && NetworkingClient.CurrentRoom.MasterClientId == LocalPlayer.ActorNumber;
			}
		}

		public static Player MasterClient
		{
			get
			{
				if (OfflineMode)
				{
					return LocalPlayer;
				}
				if (NetworkingClient == null || NetworkingClient.CurrentRoom == null)
				{
					return null;
				}
				return NetworkingClient.CurrentRoom.GetPlayer(NetworkingClient.CurrentRoom.MasterClientId);
			}
		}

		public static bool InRoom
		{
			get
			{
				return NetworkClientState == ClientState.Joined;
			}
		}

		public static int CountOfPlayersOnMaster
		{
			get
			{
				return NetworkingClient.PlayersOnMasterCount;
			}
		}

		public static int CountOfPlayersInRooms
		{
			get
			{
				return NetworkingClient.PlayersInRoomsCount;
			}
		}

		public static int CountOfPlayers
		{
			get
			{
				return NetworkingClient.PlayersInRoomsCount + NetworkingClient.PlayersOnMasterCount;
			}
		}

		public static int CountOfRooms
		{
			get
			{
				return NetworkingClient.RoomsCount;
			}
		}

		public static bool NetworkStatisticsEnabled
		{
			get
			{
				return NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled;
			}
			set
			{
				NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = value;
			}
		}

		public static int ResentReliableCommands
		{
			get
			{
				return NetworkingClient.LoadBalancingPeer.ResentReliableCommands;
			}
		}

		public static bool CrcCheckEnabled
		{
			get
			{
				return NetworkingClient.LoadBalancingPeer.CrcEnabled;
			}
			set
			{
				if (!IsConnected)
				{
					NetworkingClient.LoadBalancingPeer.CrcEnabled = value;
				}
				else
				{
					UnityEngine.Debug.Log("Can't change CrcCheckEnabled while being connected. CrcCheckEnabled stays " + NetworkingClient.LoadBalancingPeer.CrcEnabled);
				}
			}
		}

		public static int PacketLossByCrcCheck
		{
			get
			{
				return NetworkingClient.LoadBalancingPeer.PacketLossByCrc;
			}
		}

		public static int MaxResendsBeforeDisconnect
		{
			get
			{
				return NetworkingClient.LoadBalancingPeer.SentCountAllowance;
			}
			set
			{
				if (value < 3)
				{
					value = 3;
				}
				if (value > 10)
				{
					value = 10;
				}
				NetworkingClient.LoadBalancingPeer.SentCountAllowance = value;
			}
		}

		public static int QuickResends
		{
			get
			{
				return NetworkingClient.LoadBalancingPeer.QuickResendAttempts;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > 3)
				{
					value = 3;
				}
				NetworkingClient.LoadBalancingPeer.QuickResendAttempts = (byte)value;
			}
		}

		public static bool UseAlternativeUdpPorts
		{
			get
			{
				return NetworkingClient != null && NetworkingClient.UseAlternativeUdpPorts;
			}
			set
			{
				if (NetworkingClient != null)
				{
					NetworkingClient.UseAlternativeUdpPorts = value;
				}
			}
		}

		public static PhotonView[] PhotonViews
		{
			get
			{
				return photonViewList.Values.ToArray();
			}
		}

		public static IPunPrefabPool PrefabPool
		{
			get
			{
				return prefabPool;
			}
			set
			{
				if (value == null)
				{
					UnityEngine.Debug.LogWarning("PhotonNetwork.PrefabPool cannot be set to null. It will default back to using the 'DefaultPool' Pool");
					prefabPool = new DefaultPool();
				}
				else
				{
					prefabPool = value;
				}
			}
		}

		public static float LevelLoadingProgress
		{
			get
			{
				if (_AsyncLevelLoadingOperation != null)
				{
					_levelLoadingProgress = _AsyncLevelLoadingOperation.progress;
				}
				else if (_levelLoadingProgress > 0f)
				{
					_levelLoadingProgress = 1f;
				}
				return _levelLoadingProgress;
			}
		}

		private static event Action<PhotonView, Player> OnOwnershipRequestEv
		{
			add
			{
				Action<PhotonView, Player> action = OnOwnershipRequestEv__BackingField;
				Action<PhotonView, Player> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnOwnershipRequestEv__BackingField, (Action<PhotonView, Player>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<PhotonView, Player> action = OnOwnershipRequestEv__BackingField;
				Action<PhotonView, Player> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnOwnershipRequestEv__BackingField, (Action<PhotonView, Player>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		private static event Action<PhotonView, Player> OnOwnershipTransferedEv
		{
			add
			{
				Action<PhotonView, Player> action = OnOwnershipTransferedEv__BackingField;
				Action<PhotonView, Player> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnOwnershipTransferedEv__BackingField, (Action<PhotonView, Player>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<PhotonView, Player> action = OnOwnershipTransferedEv__BackingField;
				Action<PhotonView, Player> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnOwnershipTransferedEv__BackingField, (Action<PhotonView, Player>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		static PhotonNetwork()
		{
			MAX_VIEW_IDS = 1000;
			PhotonServerSettings = (ServerSettings)Resources.Load("PhotonServerSettings", typeof(ServerSettings));
			ConnectMethod = ConnectMethod.NotCalled;
			LogLevel = PunLogLevel.ErrorsOnly;
			PrecisionForVectorSynchronization = 9.9E-05f;
			PrecisionForQuaternionSynchronization = 1f;
			PrecisionForFloatSynchronization = 0.01f;
			offlineMode = false;
			offlineModeRoom = null;
			automaticallySyncScene = false;
			sendFrequency = 50;
			serializationFrequency = 100;
			isMessageQueueRunning = true;
			lastUsedViewSubId = 0;
			lastUsedViewSubIdStatic = 0;
			PrefabsWithoutMagicCallback = new HashSet<string>();
			SendInstantiateEvHashtable = new Hashtable();
			SendInstantiateRaiseEventOptions = new RaiseEventOptions();
			allowedReceivingGroups = new HashSet<byte>();
			blockedSendingGroups = new HashSet<byte>();
			photonViewList = new Dictionary<int, PhotonView>();
			currentLevelPrefix = 0;
			loadingLevelAndPausedNetwork = false;
			monoRPCMethodsCache = new Dictionary<Type, List<MethodInfo>>();
			_levelLoadingProgress = 0f;
			removeFilter = new Hashtable();
			ServerCleanDestroyEvent = new Hashtable();
			ServerCleanOptions = new RaiseEventOptions
			{
				CachingOption = EventCaching.RemoveFromRoomCache
			};
			rpcFilterByViewId = new Hashtable();
			OpCleanRpcBufferOptions = new RaiseEventOptions
			{
				CachingOption = EventCaching.RemoveFromRoomCache
			};
			rpcEvent = new Hashtable();
			RpcOptionsToAll = new RaiseEventOptions();
			ObjectsInOneUpdate = 10;
			serializeStreamOut = new PhotonStream(true, null);
			serializeStreamIn = new PhotonStream(false, null);
			serializeRaiseEvOptions = new RaiseEventOptions();
			serializeViewBatches = new Dictionary<RaiseEventBatch, SerializeViewBatch>();
			if (PhotonServerSettings != null)
			{
				Application.runInBackground = PhotonServerSettings.RunInBackground;
			}
			PrefabPool = new DefaultPool();
			ConnectionProtocol protocol = PhotonServerSettings.AppSettings.Protocol;
			NetworkingClient = new LoadBalancingClient(protocol);
			NetworkingClient.LoadBalancingPeer.QuickResendAttempts = 2;
			NetworkingClient.LoadBalancingPeer.SentCountAllowance = 7;
			NetworkingClient.EventReceived += OnEvent;
			NetworkingClient.OpResponseReceived += OnOperation;
			NetworkingClient.StateChanged += delegate(ClientState previousState, ClientState state)
			{
				if ((previousState == ClientState.Joined && state == ClientState.Disconnected) || (Server == ServerConnection.GameServer && (state == ClientState.Disconnecting || state == ClientState.DisconnectingFromGameserver)))
				{
					LeftRoomCleanup();
				}
				if (state == ClientState.ConnectedToMasterserver && _cachedRegionHandler != null)
				{
					BestRegionSummaryInPreferences = _cachedRegionHandler.SummaryToCache;
					_cachedRegionHandler = null;
				}
			};
			rpcShortcuts = new Dictionary<string, int>(PhotonServerSettings.RpcList.Count);
			for (int num = 0; num < PhotonServerSettings.RpcList.Count; num++)
			{
				string key = PhotonServerSettings.RpcList[num];
				rpcShortcuts[key] = num;
			}
			StartupStopwatch = new Stopwatch();
			StartupStopwatch.Start();
			NetworkingClient.LoadBalancingPeer.LocalMsTimestampDelegate = () => (int)StartupStopwatch.ElapsedMilliseconds;
			CustomTypes.Register();
			GameObject gameObject = new GameObject();
			photonMono = gameObject.AddComponent<PhotonHandler>();
			gameObject.name = "PhotonMono";
			gameObject.hideFlags = HideFlags.HideInHierarchy;
		}

		public static bool ConnectUsingSettings()
		{
			if (NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
			{
				UnityEngine.Debug.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			if (PhotonServerSettings == null)
			{
				UnityEngine.Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: PhotonServerSettings");
				return false;
			}
			SetupLogging();
			NetworkingClient.LoadBalancingPeer.TransportProtocol = PhotonServerSettings.AppSettings.Protocol;
			IsMessageQueueRunning = true;
			NetworkingClient.AppId = PhotonServerSettings.AppSettings.AppIdRealtime;
			GameVersion = PhotonServerSettings.AppSettings.AppVersion;
			if (PhotonServerSettings.StartInOfflineMode)
			{
				OfflineMode = true;
				return true;
			}
			if (OfflineMode)
			{
				OfflineMode = false;
				UnityEngine.Debug.LogWarning("ConnectUsingSettings() disabled the offline mode. No longer offline.");
			}
			NetworkingClient.EnableLobbyStatistics = PhotonServerSettings.AppSettings.EnableLobbyStatistics;
			if (PhotonServerSettings.AppSettings.IsMasterServerAddress)
			{
				NetworkingClient.LoadBalancingPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV16;
				return ConnectToMaster(PhotonServerSettings.AppSettings.Server, PhotonServerSettings.AppSettings.Port, PhotonServerSettings.AppSettings.AppIdRealtime);
			}
			if (!PhotonServerSettings.AppSettings.IsDefaultNameServer)
			{
				NetworkingClient.NameServerHost = PhotonServerSettings.AppSettings.Server;
			}
			if (PhotonServerSettings.AppSettings.IsBestRegion)
			{
				return ConnectToBestCloudServer();
			}
			return ConnectToRegion(PhotonServerSettings.AppSettings.FixedRegion);
		}

		public static bool ConnectToMaster(string masterServerAddress, int port, string appID)
		{
			if (NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
			{
				UnityEngine.Debug.LogWarning("ConnectToMaster() failed. Can only connect while in state 'Disconnected'. Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			if (OfflineMode)
			{
				OfflineMode = false;
				UnityEngine.Debug.LogWarning("ConnectToMaster() disabled the offline mode. No longer offline.");
			}
			if (!IsMessageQueueRunning)
			{
				IsMessageQueueRunning = true;
				UnityEngine.Debug.LogWarning("ConnectToMaster() enabled IsMessageQueueRunning. Needs to be able to dispatch incoming messages.");
			}
			SetupLogging();
			ConnectMethod = ConnectMethod.ConnectToMaster;
			NetworkingClient.IsUsingNameServer = false;
			NetworkingClient.MasterServerAddress = ((port != 0) ? (masterServerAddress + ":" + port) : masterServerAddress);
			NetworkingClient.AppId = appID;
			return NetworkingClient.Connect();
		}

		public static bool ConnectToBestCloudServer()
		{
			if (NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
			{
				UnityEngine.Debug.LogWarning("ConnectToBestCloudServer() failed. Can only connect while in state 'Disconnected'. Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			SetupLogging();
			ConnectMethod = ConnectMethod.ConnectToBest;
			return NetworkingClient.ConnectToNameServer();
		}

		public static bool ConnectToRegion(string region)
		{
			if (NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected && NetworkingClient.Server != ServerConnection.NameServer)
			{
				UnityEngine.Debug.LogWarning("ConnectToRegion() failed. Can only connect while in state 'Disconnected'. Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			SetupLogging();
			ConnectMethod = ConnectMethod.ConnectToRegion;
			if (!string.IsNullOrEmpty(region))
			{
				return NetworkingClient.ConnectToRegionMaster(region);
			}
			return false;
		}

		public static void Disconnect()
		{
			if (OfflineMode)
			{
				OfflineMode = false;
				offlineModeRoom = null;
				NetworkingClient.State = ClientState.Disconnecting;
				NetworkingClient.OnStatusChanged(StatusCode.Disconnect);
			}
			else if (NetworkingClient != null)
			{
				NetworkingClient.Disconnect();
			}
		}

		public static bool Reconnect()
		{
			if (string.IsNullOrEmpty(NetworkingClient.MasterServerAddress))
			{
				UnityEngine.Debug.LogWarning("Reconnect() failed. It seems the client wasn't connected before?! Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			if (NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
			{
				UnityEngine.Debug.LogWarning("Reconnect() failed. Can only connect while in state 'Disconnected'. Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			if (OfflineMode)
			{
				OfflineMode = false;
				UnityEngine.Debug.LogWarning("Reconnect() disabled the offline mode. No longer offline.");
			}
			if (!IsMessageQueueRunning)
			{
				IsMessageQueueRunning = true;
				UnityEngine.Debug.LogWarning("Reconnect() enabled IsMessageQueueRunning. Needs to be able to dispatch incoming messages.");
			}
			NetworkingClient.IsUsingNameServer = false;
			return NetworkingClient.ReconnectToMaster();
		}

		public static void NetworkStatisticsReset()
		{
			NetworkingClient.LoadBalancingPeer.TrafficStatsReset();
		}

		public static string NetworkStatisticsToString()
		{
			if (NetworkingClient == null || OfflineMode)
			{
				return "Offline or in OfflineMode. No VitalStats available.";
			}
			return NetworkingClient.LoadBalancingPeer.VitalStatsToString(false);
		}

		private static bool VerifyCanUseNetwork()
		{
			if (IsConnected)
			{
				return true;
			}
			UnityEngine.Debug.LogError("Cannot send messages when not connected. Either connect to Photon OR use offline mode!");
			return false;
		}

		public static int GetPing()
		{
			return NetworkingClient.LoadBalancingPeer.RoundTripTime;
		}

		public static void FetchServerTimestamp()
		{
			if (NetworkingClient != null)
			{
				NetworkingClient.LoadBalancingPeer.FetchServerTimestamp();
			}
		}

		public static void SendAllOutgoingCommands()
		{
			if (VerifyCanUseNetwork())
			{
				while (NetworkingClient.LoadBalancingPeer.SendOutgoingCommands())
				{
				}
			}
		}

		public static bool CloseConnection(Player kickPlayer)
		{
			if (!VerifyCanUseNetwork())
			{
				return false;
			}
			if (!LocalPlayer.IsMasterClient)
			{
				UnityEngine.Debug.LogError("CloseConnection: Only the masterclient can kick another player.");
				return false;
			}
			if (kickPlayer == null)
			{
				UnityEngine.Debug.LogError("CloseConnection: No such player connected!");
				return false;
			}
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[1] { kickPlayer.ActorNumber };
			RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
			return NetworkingClient.OpRaiseEvent(203, null, raiseEventOptions2, SendOptions.SendReliable);
		}

		public static bool SetMasterClient(Player masterClientPlayer)
		{
			if (!InRoom || !VerifyCanUseNetwork() || OfflineMode)
			{
				if (LogLevel == PunLogLevel.Informational)
				{
					UnityEngine.Debug.Log("Can not SetMasterClient(). Not in room or in OfflineMode.");
				}
				return false;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add((byte)248, masterClientPlayer.ActorNumber);
			Hashtable gameProperties = hashtable;
			hashtable = new Hashtable();
			hashtable.Add((byte)248, NetworkingClient.CurrentRoom.MasterClientId);
			Hashtable expectedProperties = hashtable;
			return NetworkingClient.OpSetPropertiesOfRoom(gameProperties, expectedProperties);
		}

		public static bool JoinRandomRoom()
		{
			return JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, null, null);
		}

		public static bool JoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers)
		{
			return JoinRandomRoom(expectedCustomRoomProperties, expectedMaxPlayers, MatchmakingMode.FillRoom, null, null);
		}

		public static bool JoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, MatchmakingMode matchingType, TypedLobby typedLobby, string sqlLobbyFilter, string[] expectedUsers = null)
		{
			if (OfflineMode)
			{
				if (offlineModeRoom != null)
				{
					UnityEngine.Debug.LogError("JoinRandomRoom failed. In offline mode you still have to leave a room to enter another.");
					return false;
				}
				EnterOfflineRoom("offline room", null, true);
				return true;
			}
			if (NetworkingClient.Server != ServerConnection.MasterServer || !IsConnectedAndReady)
			{
				UnityEngine.Debug.LogError(string.Concat("JoinRandomRoom failed. Client is on ", NetworkingClient.Server, " (must be Master Server for matchmaking)", (!IsConnectedAndReady) ? string.Concat(" but not ready for operations (State: ", NetworkingClient.State, ")") : " and ready", ". Wait for callback: OnJoinedLobby or OnConnectedToMaster."));
				return false;
			}
			typedLobby = typedLobby ?? ((!NetworkingClient.InLobby) ? null : NetworkingClient.CurrentLobby);
			OpJoinRandomRoomParams opJoinRandomRoomParams = new OpJoinRandomRoomParams();
			opJoinRandomRoomParams.ExpectedCustomRoomProperties = expectedCustomRoomProperties;
			opJoinRandomRoomParams.ExpectedMaxPlayers = expectedMaxPlayers;
			opJoinRandomRoomParams.MatchingType = matchingType;
			opJoinRandomRoomParams.TypedLobby = typedLobby;
			opJoinRandomRoomParams.SqlLobbyFilter = sqlLobbyFilter;
			opJoinRandomRoomParams.ExpectedUsers = expectedUsers;
			return NetworkingClient.OpJoinRandomRoom(opJoinRandomRoomParams);
		}

		public static bool CreateRoom(string roomName, RoomOptions roomOptions = null, TypedLobby typedLobby = null, string[] expectedUsers = null)
		{
			if (OfflineMode)
			{
				if (offlineModeRoom != null)
				{
					UnityEngine.Debug.LogError("CreateRoom failed. In offline mode you still have to leave a room to enter another.");
					return false;
				}
				EnterOfflineRoom(roomName, roomOptions, true);
				return true;
			}
			if (NetworkingClient.Server != ServerConnection.MasterServer || !IsConnectedAndReady)
			{
				UnityEngine.Debug.LogError(string.Concat("CreateRoom failed. Client is on ", NetworkingClient.Server, " (must be Master Server for matchmaking)", (!IsConnectedAndReady) ? string.Concat("but not ready for operations (State: ", NetworkingClient.State, ")") : " and ready", ". Wait for callback: OnJoinedLobby or OnConnectedToMaster."));
				return false;
			}
			typedLobby = typedLobby ?? ((!NetworkingClient.InLobby) ? null : NetworkingClient.CurrentLobby);
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.RoomOptions = roomOptions;
			enterRoomParams.Lobby = typedLobby;
			enterRoomParams.ExpectedUsers = expectedUsers;
			return NetworkingClient.OpCreateRoom(enterRoomParams);
		}

		public static bool JoinOrCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby, string[] expectedUsers = null)
		{
			if (OfflineMode)
			{
				if (offlineModeRoom != null)
				{
					UnityEngine.Debug.LogError("JoinOrCreateRoom failed. In offline mode you still have to leave a room to enter another.");
					return false;
				}
				EnterOfflineRoom(roomName, roomOptions, true);
				return true;
			}
			if (NetworkingClient.Server != ServerConnection.MasterServer || !IsConnectedAndReady)
			{
				UnityEngine.Debug.LogError(string.Concat("JoinOrCreateRoom failed. Client is on ", NetworkingClient.Server, " (must be Master Server for matchmaking)", (!IsConnectedAndReady) ? string.Concat("but not ready for operations (State: ", NetworkingClient.State, ")") : " and ready", ". Wait for callback: OnJoinedLobby or OnConnectedToMaster."));
				return false;
			}
			if (string.IsNullOrEmpty(roomName))
			{
				UnityEngine.Debug.LogError("JoinOrCreateRoom failed. A roomname is required. If you don't know one, how will you join?");
				return false;
			}
			typedLobby = typedLobby ?? ((!NetworkingClient.InLobby) ? null : NetworkingClient.CurrentLobby);
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.RoomOptions = roomOptions;
			enterRoomParams.Lobby = typedLobby;
			enterRoomParams.CreateIfNotExists = true;
			enterRoomParams.PlayerProperties = LocalPlayer.CustomProperties;
			enterRoomParams.ExpectedUsers = expectedUsers;
			return NetworkingClient.OpJoinRoom(enterRoomParams);
		}

		public static bool JoinRoom(string roomName, string[] expectedUsers = null)
		{
			if (OfflineMode)
			{
				if (offlineModeRoom != null)
				{
					UnityEngine.Debug.LogError("JoinRoom failed. In offline mode you still have to leave a room to enter another.");
					return false;
				}
				EnterOfflineRoom(roomName, null, true);
				return true;
			}
			if (NetworkingClient.Server != ServerConnection.MasterServer || !IsConnectedAndReady)
			{
				UnityEngine.Debug.LogError(string.Concat("JoinRoom failed. Client is on ", NetworkingClient.Server, " (must be Master Server for matchmaking)", (!IsConnectedAndReady) ? string.Concat("but not ready for operations (State: ", NetworkingClient.State, ")") : " and ready", ". Wait for callback: OnJoinedLobby or OnConnectedToMaster."));
				return false;
			}
			if (string.IsNullOrEmpty(roomName))
			{
				UnityEngine.Debug.LogError("JoinRoom failed. A roomname is required. If you don't know one, how will you join?");
				return false;
			}
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.ExpectedUsers = expectedUsers;
			return NetworkingClient.OpJoinRoom(enterRoomParams);
		}

		public static bool RejoinRoom(string roomName)
		{
			if (OfflineMode)
			{
				UnityEngine.Debug.LogError("RejoinRoom failed due to offline mode.");
				return false;
			}
			if (NetworkingClient.Server != ServerConnection.MasterServer || !IsConnectedAndReady)
			{
				UnityEngine.Debug.LogError(string.Concat("RejoinRoom failed. Client is on ", NetworkingClient.Server, " (must be Master Server for matchmaking)", (!IsConnectedAndReady) ? string.Concat("but not ready for operations (State: ", NetworkingClient.State, ")") : " and ready", ". Wait for callback: OnJoinedLobby or OnConnectedToMaster."));
				return false;
			}
			if (string.IsNullOrEmpty(roomName))
			{
				UnityEngine.Debug.LogError("RejoinRoom failed. A roomname is required. If you don't know one, how will you join?");
				return false;
			}
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.RejoinOnly = true;
			enterRoomParams.PlayerProperties = LocalPlayer.CustomProperties;
			return NetworkingClient.OpJoinRoom(enterRoomParams);
		}

		public static bool ReconnectAndRejoin()
		{
			if (NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
			{
				UnityEngine.Debug.LogWarning("ReconnectAndRejoin() failed. Can only connect while in state 'Disconnected'. Current state: " + NetworkingClient.LoadBalancingPeer.PeerState);
				return false;
			}
			if (OfflineMode)
			{
				OfflineMode = false;
				UnityEngine.Debug.LogWarning("ReconnectAndRejoin() disabled the offline mode. No longer offline.");
			}
			if (!IsMessageQueueRunning)
			{
				IsMessageQueueRunning = true;
				UnityEngine.Debug.LogWarning("ReconnectAndRejoin() enabled IsMessageQueueRunning. Needs to be able to dispatch incoming messages.");
			}
			NetworkingClient.IsUsingNameServer = false;
			return NetworkingClient.ReconnectAndRejoin();
		}

		public static bool LeaveRoom(bool becomeInactive = true)
		{
			if (OfflineMode)
			{
				offlineModeRoom = null;
				NetworkingClient.MatchMakingCallbackTargets.OnLeftRoom();
				return true;
			}
			if (CurrentRoom == null)
			{
				UnityEngine.Debug.LogWarning("PhotonNetwork.CurrentRoom is null. You don't have to call LeaveRoom() when you're not in one. State: " + NetworkClientState);
			}
			else
			{
				becomeInactive = becomeInactive && CurrentRoom.PlayerTtl != 0;
			}
			return NetworkingClient.OpLeaveRoom(becomeInactive);
		}

		private static void EnterOfflineRoom(string roomName, RoomOptions roomOptions, bool createdRoom)
		{
			offlineModeRoom = new Room(roomName, roomOptions, true);
			NetworkingClient.ChangeLocalID(1);
			offlineModeRoom.masterClientId = 1;
			offlineModeRoom.AddPlayer(LocalPlayer);
			offlineModeRoom.LoadBalancingClient = NetworkingClient;
			if (createdRoom)
			{
				NetworkingClient.MatchMakingCallbackTargets.OnCreatedRoom();
			}
			NetworkingClient.MatchMakingCallbackTargets.OnJoinedRoom();
		}

		public static bool JoinLobby()
		{
			return JoinLobby(null);
		}

		public static bool JoinLobby(TypedLobby typedLobby)
		{
			if (IsConnected && Server == ServerConnection.MasterServer)
			{
				return NetworkingClient.OpJoinLobby(typedLobby);
			}
			return false;
		}

		public static bool LeaveLobby()
		{
			if (IsConnected && Server == ServerConnection.MasterServer)
			{
				return NetworkingClient.OpLeaveLobby();
			}
			return false;
		}

		public static bool FindFriends(string[] friendsToFind)
		{
			if (NetworkingClient == null || offlineMode)
			{
				return false;
			}
			return NetworkingClient.OpFindFriends(friendsToFind);
		}

		public static bool GetCustomRoomList(TypedLobby typedLobby, string sqlLobbyFilter)
		{
			return NetworkingClient.OpGetGameList(typedLobby, sqlLobbyFilter);
		}

		public static void SetPlayerCustomProperties(Hashtable customProperties)
		{
			if (customProperties == null)
			{
				customProperties = new Hashtable();
				foreach (object key in LocalPlayer.CustomProperties.Keys)
				{
					customProperties[(string)key] = null;
				}
			}
			if (CurrentRoom != null)
			{
				LocalPlayer.SetCustomProperties(customProperties);
			}
			else
			{
				LocalPlayer.InternalCacheProperties(customProperties);
			}
		}

		public static void RemovePlayerCustomProperties(string[] customPropertiesToDelete)
		{
			if (customPropertiesToDelete == null || customPropertiesToDelete.Length == 0 || LocalPlayer.CustomProperties == null)
			{
				LocalPlayer.CustomProperties = new Hashtable();
				return;
			}
			foreach (string key in customPropertiesToDelete)
			{
				if (LocalPlayer.CustomProperties.ContainsKey(key))
				{
					LocalPlayer.CustomProperties.Remove(key);
				}
			}
		}

		public static bool RaiseEvent(byte eventCode, object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
		{
			if (offlineMode)
			{
				if (raiseEventOptions.Receivers == ReceiverGroup.Others)
				{
					return true;
				}
				EventData eventData = new EventData();
				eventData.Code = eventCode;
				eventData.Parameters = new Dictionary<byte, object> { { 245, eventContent } };
				EventData photonEvent = eventData;
				NetworkingClient.OnEvent(photonEvent);
				return true;
			}
			if (!InRoom || eventCode >= 200)
			{
				UnityEngine.Debug.LogWarning("RaiseEvent(" + eventCode + ") failed. Your event is not being sent! Check if your are in a Room and the eventCode must be less than 200 (0..199).");
				return false;
			}
			return NetworkingClient.OpRaiseEvent(eventCode, eventContent, raiseEventOptions, sendOptions);
		}

		private static bool RaiseEventInternal(byte eventCode, object eventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
		{
			if (offlineMode)
			{
				return false;
			}
			if (!InRoom)
			{
				UnityEngine.Debug.LogWarning("RaiseEvent(" + eventCode + ") failed. Your event is not being sent! Check if your are in a Room");
				return false;
			}
			return NetworkingClient.OpRaiseEvent(eventCode, eventContent, raiseEventOptions, sendOptions);
		}

		public static bool AllocateViewID(PhotonView view)
		{
			if (view.ViewID != 0)
			{
				UnityEngine.Debug.LogError("AllocateViewID() can't be used for PhotonViews that already have a viewID. This view is: " + view.ToString());
				return false;
			}
			int viewID = AllocateViewID(LocalPlayer.ActorNumber);
			view.ViewID = viewID;
			return true;
		}

		public static bool AllocateSceneViewID(PhotonView view)
		{
			if (!IsMasterClient)
			{
				UnityEngine.Debug.LogError("Only the Master Client can AllocateSceneViewID(). Check PhotonNetwork.IsMasterClient!");
				return false;
			}
			if (view.ViewID != 0)
			{
				UnityEngine.Debug.LogError("AllocateSceneViewID() can't be used for PhotonViews that already have a viewID. This view is: " + view.ToString());
				return false;
			}
			int viewID = AllocateViewID(0);
			view.ViewID = viewID;
			return true;
		}

		private static int AllocateViewID(int ownerId)
		{
			if (ownerId == 0)
			{
				int num = lastUsedViewSubIdStatic;
				int num2 = ownerId * MAX_VIEW_IDS;
				for (int i = 1; i < MAX_VIEW_IDS; i++)
				{
					num = (num + 1) % MAX_VIEW_IDS;
					if (num != 0)
					{
						int num3 = num + num2;
						if (!photonViewList.ContainsKey(num3))
						{
							lastUsedViewSubIdStatic = num;
							return num3;
						}
					}
				}
				throw new Exception(string.Format("AllocateViewID() failed. The room (user {0}) is out of 'scene' viewIDs. It seems all available are in use.", ownerId));
			}
			int num4 = lastUsedViewSubId;
			int num5 = ownerId * MAX_VIEW_IDS;
			for (int j = 1; j <= MAX_VIEW_IDS; j++)
			{
				num4 = (num4 + 1) % MAX_VIEW_IDS;
				if (num4 != 0)
				{
					int num6 = num4 + num5;
					if (!photonViewList.ContainsKey(num6))
					{
						lastUsedViewSubId = num4;
						return num6;
					}
				}
			}
			throw new Exception(string.Format("AllocateViewID() failed. User {0} is out of viewIDs. It seems all available are in use.", ownerId));
		}

		public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
		{
			InstantiateParameters parameters = new InstantiateParameters(prefabName, position, rotation, group, data, currentLevelPrefix, null, LocalPlayer, ServerTimestamp);
			return NetworkInstantiate(parameters);
		}

		public static GameObject InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
		{
			if (LocalPlayer.IsMasterClient)
			{
				InstantiateParameters parameters = new InstantiateParameters(prefabName, position, rotation, group, data, currentLevelPrefix, null, LocalPlayer, ServerTimestamp);
				return NetworkInstantiate(parameters, true);
			}
			return null;
		}

		private static GameObject NetworkInstantiate(Hashtable networkEvent, Player creator)
		{
			string prefabName = (string)networkEvent[(byte)0];
			int timestamp = (int)networkEvent[(byte)6];
			int num = (int)networkEvent[(byte)7];
			Vector3 position = ((!networkEvent.ContainsKey((byte)1)) ? Vector3.zero : ((Vector3)networkEvent[(byte)1]));
			Quaternion rotation = Quaternion.identity;
			if (networkEvent.ContainsKey((byte)2))
			{
				rotation = (Quaternion)networkEvent[(byte)2];
			}
			byte b = 0;
			if (networkEvent.ContainsKey((byte)3))
			{
				b = (byte)networkEvent[(byte)3];
			}
			byte objLevelPrefix = 0;
			if (networkEvent.ContainsKey((byte)8))
			{
				objLevelPrefix = (byte)networkEvent[(byte)8];
			}
			int[] viewIDs = ((!networkEvent.ContainsKey((byte)4)) ? new int[1] { num } : ((int[])networkEvent[(byte)4]));
			object[] data = ((!networkEvent.ContainsKey((byte)5)) ? null : ((object[])networkEvent[(byte)5]));
			if (b != 0 && !allowedReceivingGroups.Contains(b))
			{
				return null;
			}
			InstantiateParameters parameters = new InstantiateParameters(prefabName, position, rotation, b, data, objLevelPrefix, viewIDs, creator, timestamp);
			return NetworkInstantiate(parameters, false, true);
		}

		private static GameObject NetworkInstantiate(InstantiateParameters parameters, bool sceneObject = false, bool instantiateEvent = false)
		{
			GameObject gameObject = null;
			gameObject = prefabPool.Instantiate(parameters.prefabName, parameters.position, parameters.rotation);
			if (gameObject == null)
			{
				UnityEngine.Debug.LogError("Failed to network-Instantiate: " + parameters.prefabName);
				return null;
			}
			if (gameObject.activeSelf)
			{
				UnityEngine.Debug.LogWarning("PrefabPool.Instantiate() should return an inactive GameObject. " + prefabPool.GetType().Name + " returned an active object. PrefabId: " + parameters.prefabName);
			}
			PhotonView[] photonViewsInChildren = gameObject.GetPhotonViewsInChildren();
			if (photonViewsInChildren.Length == 0)
			{
				UnityEngine.Debug.LogError("PhotonNetwork.Instantiate() can only instantiate objects with a PhotonView component. This prefab does not have one: " + parameters.prefabName);
				return null;
			}
			bool flag = !instantiateEvent && LocalPlayer.Equals(parameters.creator);
			if (flag)
			{
				parameters.viewIDs = new int[photonViewsInChildren.Length];
			}
			for (int i = 0; i < photonViewsInChildren.Length; i++)
			{
				if (flag)
				{
					parameters.viewIDs[i] = ((!sceneObject) ? AllocateViewID(parameters.creator.ActorNumber) : AllocateViewID(0));
				}
				photonViewsInChildren[i].didAwake = false;
				photonViewsInChildren[i].ViewID = 0;
				photonViewsInChildren[i].Prefix = parameters.objLevelPrefix;
				photonViewsInChildren[i].InstantiationId = parameters.viewIDs[0];
				photonViewsInChildren[i].isRuntimeInstantiated = true;
				photonViewsInChildren[i].InstantiationData = parameters.data;
				photonViewsInChildren[i].didAwake = true;
				photonViewsInChildren[i].ViewID = parameters.viewIDs[i];
			}
			if (flag)
			{
				SendInstantiate(parameters, sceneObject);
			}
			gameObject.SetActive(true);
			if (!PrefabsWithoutMagicCallback.Contains(parameters.prefabName))
			{
				IPunInstantiateMagicCallback[] components = gameObject.GetComponents<IPunInstantiateMagicCallback>();
				if (components.Length > 0)
				{
					PhotonMessageInfo info = new PhotonMessageInfo(parameters.creator, parameters.timestamp, photonViewsInChildren[0]);
					IPunInstantiateMagicCallback[] array = components;
					foreach (IPunInstantiateMagicCallback punInstantiateMagicCallback in array)
					{
						punInstantiateMagicCallback.OnPhotonInstantiate(info);
					}
				}
				else
				{
					PrefabsWithoutMagicCallback.Add(parameters.prefabName);
				}
			}
			return gameObject;
		}

		internal static bool SendInstantiate(InstantiateParameters parameters, bool sceneObject = false)
		{
			int num = parameters.viewIDs[0];
			SendInstantiateEvHashtable.Clear();
			SendInstantiateEvHashtable[(byte)0] = parameters.prefabName;
			if (parameters.position != Vector3.zero)
			{
				SendInstantiateEvHashtable[(byte)1] = parameters.position;
			}
			if (parameters.rotation != Quaternion.identity)
			{
				SendInstantiateEvHashtable[(byte)2] = parameters.rotation;
			}
			if (parameters.group != 0)
			{
				SendInstantiateEvHashtable[(byte)3] = parameters.group;
			}
			if (parameters.viewIDs.Length > 1)
			{
				SendInstantiateEvHashtable[(byte)4] = parameters.viewIDs;
			}
			if (parameters.data != null)
			{
				SendInstantiateEvHashtable[(byte)5] = parameters.data;
			}
			if (currentLevelPrefix > 0)
			{
				SendInstantiateEvHashtable[(byte)8] = currentLevelPrefix;
			}
			SendInstantiateEvHashtable[(byte)6] = ServerTimestamp;
			SendInstantiateEvHashtable[(byte)7] = num;
			SendInstantiateRaiseEventOptions.CachingOption = ((!sceneObject) ? EventCaching.AddToRoomCache : EventCaching.AddToRoomCacheGlobal);
			return RaiseEventInternal(202, SendInstantiateEvHashtable, SendInstantiateRaiseEventOptions, SendOptions.SendReliable);
		}

		public static void Destroy(PhotonView targetView)
		{
			if (targetView != null)
			{
				RemoveInstantiatedGO(targetView.gameObject, !InRoom);
			}
			else
			{
				UnityEngine.Debug.LogError("Destroy(targetPhotonView) failed, cause targetPhotonView is null.");
			}
		}

		public static void Destroy(GameObject targetGo)
		{
			RemoveInstantiatedGO(targetGo, !InRoom);
		}

		public static void DestroyPlayerObjects(Player targetPlayer)
		{
			if (LocalPlayer == null)
			{
				UnityEngine.Debug.LogError("DestroyPlayerObjects() failed, cause parameter 'targetPlayer' was null.");
			}
			DestroyPlayerObjects(targetPlayer.ActorNumber);
		}

		public static void DestroyPlayerObjects(int targetPlayerId)
		{
			if (VerifyCanUseNetwork())
			{
				if (LocalPlayer.IsMasterClient || targetPlayerId == LocalPlayer.ActorNumber)
				{
					DestroyPlayerObjects(targetPlayerId, false);
				}
				else
				{
					UnityEngine.Debug.LogError("DestroyPlayerObjects() failed, cause players can only destroy their own GameObjects. A Master Client can destroy anyone's. This is master: " + IsMasterClient);
				}
			}
		}

		public static void DestroyAll()
		{
			if (IsMasterClient)
			{
				DestroyAll(false);
			}
			else
			{
				UnityEngine.Debug.LogError("Couldn't call DestroyAll() as only the master client is allowed to call this.");
			}
		}

		public static void RemoveRPCs(Player targetPlayer)
		{
			if (VerifyCanUseNetwork())
			{
				if (!targetPlayer.IsLocal && !IsMasterClient)
				{
					UnityEngine.Debug.LogError("Error; Only the MasterClient can call RemoveRPCs for other players.");
				}
				else
				{
					OpCleanActorRpcBuffer(targetPlayer.ActorNumber);
				}
			}
		}

		public static void RemoveRPCs(PhotonView targetPhotonView)
		{
			if (VerifyCanUseNetwork())
			{
				CleanRpcBufferIfMine(targetPhotonView);
			}
		}

		internal static void RPC(PhotonView view, string methodName, RpcTarget target, bool encrypt, params object[] parameters)
		{
			if (VerifyCanUseNetwork())
			{
				if (CurrentRoom == null)
				{
					UnityEngine.Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
				}
				else if (NetworkingClient != null)
				{
					RPC(view, methodName, target, null, encrypt, parameters);
				}
				else
				{
					UnityEngine.Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
				}
			}
		}

		internal static void RPC(PhotonView view, string methodName, Player targetPlayer, bool encrpyt, params object[] parameters)
		{
			if (!VerifyCanUseNetwork())
			{
				return;
			}
			if (CurrentRoom == null)
			{
				UnityEngine.Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
				return;
			}
			if (LocalPlayer == null)
			{
				UnityEngine.Debug.LogError("RPC can't be sent to target Player being null! Did not send \"" + methodName + "\" call.");
			}
			if (NetworkingClient != null)
			{
				RPC(view, methodName, RpcTarget.Others, targetPlayer, encrpyt, parameters);
			}
			else
			{
				UnityEngine.Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
			}
		}

		public static HashSet<GameObject> FindGameObjectsWithComponent(Type type)
		{
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			Component[] array = (Component[])UnityEngine.Object.FindObjectsOfType(type);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					hashSet.Add(array[i].gameObject);
				}
			}
			return hashSet;
		}

		public static void SetInterestGroups(byte group, bool enabled)
		{
			if (VerifyCanUseNetwork())
			{
				if (enabled)
				{
					byte[] enableGroups = new byte[1] { group };
					SetInterestGroups(null, enableGroups);
				}
				else
				{
					byte[] disableGroups = new byte[1] { group };
					SetInterestGroups(disableGroups, null);
				}
			}
		}

		public static void LoadLevel(int levelNumber)
		{
			if (AutomaticallySyncScene)
			{
				SetLevelInPropsIfSynced(levelNumber);
			}
			IsMessageQueueRunning = false;
			loadingLevelAndPausedNetwork = true;
			_AsyncLevelLoadingOperation = SceneManager.LoadSceneAsync(levelNumber, LoadSceneMode.Single);
		}

		public static void LoadLevel(string levelName)
		{
			if (AutomaticallySyncScene)
			{
				SetLevelInPropsIfSynced(levelName);
			}
			IsMessageQueueRunning = false;
			loadingLevelAndPausedNetwork = true;
			_AsyncLevelLoadingOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
		}

		public static bool WebRpc(string name, object parameters, bool sendAuthCookie = false)
		{
			return NetworkingClient.OpWebRpc(name, parameters, sendAuthCookie);
		}

		private static void SetupLogging()
		{
			if (LogLevel == PunLogLevel.ErrorsOnly)
			{
				LogLevel = PhotonServerSettings.PunLogging;
			}
			if (NetworkingClient.LoadBalancingPeer.DebugOut == DebugLevel.ERROR)
			{
				NetworkingClient.LoadBalancingPeer.DebugOut = PhotonServerSettings.AppSettings.NetworkLogging;
			}
		}

		public static void AddCallbackTarget(object target)
		{
			if (!(target is PhotonView))
			{
				IPunOwnershipCallbacks punOwnershipCallbacks = target as IPunOwnershipCallbacks;
				if (punOwnershipCallbacks != null)
				{
					OnOwnershipRequestEv += punOwnershipCallbacks.OnOwnershipRequest;
					OnOwnershipTransferedEv += punOwnershipCallbacks.OnOwnershipTransfered;
				}
				NetworkingClient.AddCallbackTarget(target);
			}
		}

		public static void RemoveCallbackTarget(object target)
		{
			if (!(target is PhotonView) && NetworkingClient != null)
			{
				IPunOwnershipCallbacks punOwnershipCallbacks = target as IPunOwnershipCallbacks;
				if (punOwnershipCallbacks != null)
				{
					OnOwnershipRequestEv -= punOwnershipCallbacks.OnOwnershipRequest;
					OnOwnershipTransferedEv -= punOwnershipCallbacks.OnOwnershipTransfered;
				}
				NetworkingClient.RemoveCallbackTarget(target);
			}
		}

		internal static string CallbacksToString()
		{
			string[] value = NetworkingClient.ConnectionCallbackTargets.Select((IConnectionCallbacks m) => m.ToString()).ToArray();
			return string.Join(", ", value);
		}

		private static void LeftRoomCleanup()
		{
			if (_AsyncLevelLoadingOperation != null)
			{
				_AsyncLevelLoadingOperation.allowSceneActivation = false;
				_AsyncLevelLoadingOperation = null;
			}
			bool flag = NetworkingClient.CurrentRoom != null && CurrentRoom.AutoCleanUp;
			allowedReceivingGroups = new HashSet<byte>();
			blockedSendingGroups = new HashSet<byte>();
			if (flag || offlineModeRoom != null)
			{
				LocalCleanupAnythingInstantiated(true);
			}
		}

		internal static void LocalCleanupAnythingInstantiated(bool destroyInstantiatedGameObjects)
		{
			if (destroyInstantiatedGameObjects)
			{
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				foreach (PhotonView value in photonViewList.Values)
				{
					if (value.isRuntimeInstantiated)
					{
						hashSet.Add(value.gameObject);
					}
				}
				foreach (GameObject item in hashSet)
				{
					RemoveInstantiatedGO(item, true);
				}
			}
			lastUsedViewSubId = 0;
			lastUsedViewSubIdStatic = 0;
		}

		private static void ResetPhotonViewsOnSerialize()
		{
			foreach (PhotonView value in photonViewList.Values)
			{
				value.lastOnSerializeDataSent = null;
			}
		}

		internal static void ExecuteRpc(Hashtable rpcData, Player sender)
		{
			if (rpcData == null || !rpcData.ContainsKey((byte)0))
			{
				UnityEngine.Debug.LogError("Malformed RPC; this should never occur. Content: " + SupportClass.DictionaryToString(rpcData));
				return;
			}
			int num = (int)rpcData[(byte)0];
			int num2 = 0;
			if (rpcData.ContainsKey((byte)1))
			{
				num2 = (short)rpcData[(byte)1];
			}
			string text;
			if (rpcData.ContainsKey((byte)5))
			{
				int num3 = (byte)rpcData[(byte)5];
				if (num3 > PhotonServerSettings.RpcList.Count - 1)
				{
					UnityEngine.Debug.LogError("Could not find RPC with index: " + num3 + ". Going to ignore! Check PhotonServerSettings.RpcList");
					return;
				}
				text = PhotonServerSettings.RpcList[num3];
			}
			else
			{
				text = (string)rpcData[(byte)3];
			}
			object[] array = null;
			if (rpcData.ContainsKey((byte)4))
			{
				array = (object[])rpcData[(byte)4];
			}
			PhotonView photonView = GetPhotonView(num);
			if (photonView == null)
			{
				int num4 = num / MAX_VIEW_IDS;
				bool flag = num4 == NetworkingClient.LocalPlayer.ActorNumber;
				bool flag2 = num4 == sender.ActorNumber;
				if (flag)
				{
					UnityEngine.Debug.LogWarning("Received RPC \"" + text + "\" for viewID " + num + " but this PhotonView does not exist! View was/is ours." + ((!flag2) ? " Remote called." : " Owner called.") + " By: " + sender.ActorNumber);
				}
				else
				{
					UnityEngine.Debug.LogWarning("Received RPC \"" + text + "\" for viewID " + num + " but this PhotonView does not exist! Was remote PV." + ((!flag2) ? " Remote called." : " Owner called.") + " By: " + sender.ActorNumber + " Maybe GO was destroyed but RPC not cleaned up.");
				}
				return;
			}
			if (photonView.Prefix != num2)
			{
				UnityEngine.Debug.LogError("Received RPC \"" + text + "\" on viewID " + num + " with a prefix of " + num2 + ", our prefix is " + photonView.Prefix + ". The RPC has been ignored.");
				return;
			}
			if (string.IsNullOrEmpty(text))
			{
				UnityEngine.Debug.LogError("Malformed RPC; this should never occur. Content: " + SupportClass.DictionaryToString(rpcData));
				return;
			}
			if (LogLevel >= PunLogLevel.Full)
			{
				UnityEngine.Debug.Log("Received RPC: " + text);
			}
			if (photonView.Group != 0 && !allowedReceivingGroups.Contains(photonView.Group))
			{
				return;
			}
			Type[] array2 = null;
			if (array != null && array.Length > 0)
			{
				array2 = new Type[array.Length];
				int num5 = 0;
				foreach (object obj in array)
				{
					if (obj == null)
					{
						array2[num5] = null;
					}
					else
					{
						array2[num5] = obj.GetType();
					}
					num5++;
				}
			}
			int num6 = 0;
			int num7 = 0;
			if (!UseRpcMonoBehaviourCache || photonView.RpcMonoBehaviours == null || photonView.RpcMonoBehaviours.Length == 0)
			{
				photonView.RefreshRpcMonoBehaviourCache();
			}
			for (int j = 0; j < photonView.RpcMonoBehaviours.Length; j++)
			{
				MonoBehaviour monoBehaviour = photonView.RpcMonoBehaviours[j];
				if (monoBehaviour == null)
				{
					UnityEngine.Debug.LogError("ERROR You have missing MonoBehaviours on your gameobjects!");
					continue;
				}
				Type type = monoBehaviour.GetType();
				List<MethodInfo> value = null;
				if (!monoRPCMethodsCache.TryGetValue(type, out value))
				{
					List<MethodInfo> methods = SupportClass.GetMethods(type, typeof(PunRPC));
					monoRPCMethodsCache[type] = methods;
					value = methods;
				}
				if (value == null)
				{
					continue;
				}
				for (int k = 0; k < value.Count; k++)
				{
					MethodInfo methodInfo = value[k];
					if (!methodInfo.Name.Equals(text))
					{
						continue;
					}
					ParameterInfo[] cachedParemeters = methodInfo.GetCachedParemeters();
					num7++;
					if (array == null)
					{
						if (cachedParemeters.Length == 0)
						{
							num6++;
							methodInfo.Invoke(monoBehaviour, null);
						}
						else if (cachedParemeters.Length == 1 && cachedParemeters[0].ParameterType == typeof(PhotonMessageInfo))
						{
							int timestamp = (int)rpcData[(byte)2];
							num6++;
							methodInfo.Invoke(monoBehaviour, new object[1]
							{
								new PhotonMessageInfo(sender, timestamp, photonView)
							});
						}
					}
					else if (cachedParemeters.Length == array.Length)
					{
						if (CheckTypeMatch(cachedParemeters, array2))
						{
							num6++;
							methodInfo.Invoke(monoBehaviour, array);
						}
					}
					else if (cachedParemeters.Length == array.Length + 1)
					{
						if (cachedParemeters[cachedParemeters.Length - 1].ParameterType == typeof(PhotonMessageInfo) && CheckTypeMatch(cachedParemeters, array2))
						{
							int timestamp2 = (int)rpcData[(byte)2];
							object[] array3 = new object[array.Length + 1];
							array.CopyTo(array3, 0);
							array3[array3.Length - 1] = new PhotonMessageInfo(sender, timestamp2, photonView);
							num6++;
							methodInfo.Invoke(monoBehaviour, array3);
						}
					}
					else if (cachedParemeters.Length == 1 && cachedParemeters[0].ParameterType.IsArray)
					{
						num6++;
						methodInfo.Invoke(monoBehaviour, new object[1] { array });
					}
				}
			}
			if (num6 == 1)
			{
				return;
			}
			string text2 = string.Empty;
			foreach (Type type2 in array2)
			{
				if (text2 != string.Empty)
				{
					text2 += ", ";
				}
				text2 = ((type2 != null) ? (text2 + type2.Name) : (text2 + "null"));
			}
			if (num6 == 0)
			{
				if (num7 == 0)
				{
					UnityEngine.Debug.LogError("PhotonView with ID " + num + " has no method \"" + text + "\" marked with the [PunRPC](C#) or @PunRPC(JS) property! Args: " + text2);
				}
				else
				{
					UnityEngine.Debug.LogError("PhotonView with ID " + num + " has no method \"" + text + "\" that takes " + array2.Length + " argument(s): " + text2);
				}
			}
			else
			{
				UnityEngine.Debug.LogError("PhotonView with ID " + num + " has " + num6 + " methods \"" + text + "\" that takes " + array2.Length + " argument(s): " + text2 + ". Should be just one?");
			}
		}

		private static bool CheckTypeMatch(ParameterInfo[] methodParameters, Type[] callParameterTypes)
		{
			if (methodParameters.Length < callParameterTypes.Length)
			{
				return false;
			}
			for (int i = 0; i < callParameterTypes.Length; i++)
			{
				Type parameterType = methodParameters[i].ParameterType;
				if (callParameterTypes[i] != null && !parameterType.IsAssignableFrom(callParameterTypes[i]) && (!parameterType.IsEnum || !Enum.GetUnderlyingType(parameterType).IsAssignableFrom(callParameterTypes[i])))
				{
					return false;
				}
			}
			return true;
		}

		public static void DestroyPlayerObjects(int playerId, bool localOnly)
		{
			if (playerId <= 0)
			{
				UnityEngine.Debug.LogError("Failed to Destroy objects of playerId: " + playerId);
				return;
			}
			if (!localOnly)
			{
				OpRemoveFromServerInstantiationsOfPlayer(playerId);
				OpCleanActorRpcBuffer(playerId);
				SendDestroyOfPlayer(playerId);
			}
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			foreach (PhotonView value in photonViewList.Values)
			{
				if (value != null && value.CreatorActorNr == playerId)
				{
					hashSet.Add(value.gameObject);
				}
			}
			foreach (GameObject item in hashSet)
			{
				RemoveInstantiatedGO(item, true);
			}
			foreach (PhotonView value2 in photonViewList.Values)
			{
				if (value2.OwnerActorNr == playerId)
				{
					value2.OwnerActorNr = value2.CreatorActorNr;
				}
			}
		}

		public static void DestroyAll(bool localOnly)
		{
			if (!localOnly)
			{
				OpRemoveCompleteCache();
				SendDestroyOfAll();
			}
			LocalCleanupAnythingInstantiated(true);
		}

		internal static void RemoveInstantiatedGO(GameObject go, bool localOnly)
		{
			if (go == null)
			{
				UnityEngine.Debug.LogError("Failed to 'network-remove' GameObject because it's null.");
				return;
			}
			PhotonView[] componentsInChildren = go.GetComponentsInChildren<PhotonView>(true);
			if (componentsInChildren == null || componentsInChildren.Length <= 0)
			{
				UnityEngine.Debug.LogError("Failed to 'network-remove' GameObject because has no PhotonView components: " + go);
				return;
			}
			PhotonView photonView = componentsInChildren[0];
			int creatorActorNr = photonView.CreatorActorNr;
			int instantiationId = photonView.InstantiationId;
			if (!localOnly)
			{
				if (!photonView.IsMine)
				{
					UnityEngine.Debug.LogError("Failed to 'network-remove' GameObject. Client is neither owner nor MasterClient taking over for owner who left: " + photonView);
					return;
				}
				if (instantiationId < 1)
				{
					UnityEngine.Debug.LogError(string.Concat("Failed to 'network-remove' GameObject because it is missing a valid InstantiationId on view: ", photonView, ". Not Destroying GameObject or PhotonViews!"));
					return;
				}
			}
			if (!localOnly)
			{
				ServerCleanInstantiateAndDestroy(instantiationId, creatorActorNr, photonView.isRuntimeInstantiated);
			}
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				PhotonView photonView2 = componentsInChildren[num];
				if (!(photonView2 == null))
				{
					if (photonView2.InstantiationId >= 1)
					{
						LocalCleanPhotonView(photonView2);
					}
					if (!localOnly)
					{
						OpCleanRpcBuffer(photonView2);
					}
				}
			}
			if (LogLevel >= PunLogLevel.Full)
			{
				UnityEngine.Debug.Log("Network destroy Instantiated GO: " + go.name);
			}
			go.SetActive(false);
			prefabPool.Destroy(go);
		}

		private static void ServerCleanInstantiateAndDestroy(int instantiateId, int creatorId, bool isRuntimeInstantiated)
		{
			removeFilter[(byte)7] = instantiateId;
			ServerCleanOptions.CachingOption = EventCaching.RemoveFromRoomCache;
			RaiseEventInternal(202, removeFilter, ServerCleanOptions, SendOptions.SendReliable);
			ServerCleanDestroyEvent[(byte)0] = instantiateId;
			ServerCleanOptions.CachingOption = ((!isRuntimeInstantiated) ? EventCaching.AddToRoomCacheGlobal : EventCaching.DoNotCache);
			RaiseEventInternal(204, ServerCleanDestroyEvent, ServerCleanOptions, SendOptions.SendReliable);
		}

		private static void SendDestroyOfPlayer(int actorNr)
		{
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)0] = actorNr;
			RaiseEventInternal(207, hashtable, null, SendOptions.SendReliable);
		}

		private static void SendDestroyOfAll()
		{
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)0] = -1;
			RaiseEventInternal(207, hashtable, null, SendOptions.SendReliable);
		}

		private static void OpRemoveFromServerInstantiationsOfPlayer(int actorNr)
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
			raiseEventOptions.TargetActors = new int[1] { actorNr };
			RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
			RaiseEventInternal(202, null, raiseEventOptions2, SendOptions.SendReliable);
		}

		internal static void RequestOwnership(int viewID, int fromOwner)
		{
			UnityEngine.Debug.Log("RequestOwnership(): " + viewID + " from: " + fromOwner + " Time: " + Environment.TickCount % 1000);
			RaiseEventInternal(209, new int[2] { viewID, fromOwner }, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, SendOptions.SendReliable);
		}

		internal static void TransferOwnership(int viewID, int playerID)
		{
			UnityEngine.Debug.Log("TransferOwnership() view " + viewID + " to: " + playerID + " Time: " + Environment.TickCount % 1000);
			RaiseEventInternal(210, new int[2] { viewID, playerID }, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, SendOptions.SendReliable);
		}

		public static bool LocalCleanPhotonView(PhotonView view)
		{
			view.removedFromLocalViewList = true;
			return photonViewList.Remove(view.ViewID);
		}

		public static PhotonView GetPhotonView(int viewID)
		{
			PhotonView value = null;
			photonViewList.TryGetValue(viewID, out value);
			if (value == null)
			{
				PhotonView[] array = UnityEngine.Object.FindObjectsOfType(typeof(PhotonView)) as PhotonView[];
				foreach (PhotonView photonView in array)
				{
					if (photonView.ViewID == viewID)
					{
						if (photonView.didAwake)
						{
							UnityEngine.Debug.LogWarning("Had to lookup view that wasn't in photonViewList: " + photonView);
						}
						return photonView;
					}
				}
			}
			return value;
		}

		public static void RegisterPhotonView(PhotonView netView)
		{
			if (!Application.isPlaying)
			{
				photonViewList = new Dictionary<int, PhotonView>();
				return;
			}
			if (netView.ViewID == 0)
			{
				UnityEngine.Debug.Log("PhotonView register is ignored, because viewID is 0. No id assigned yet to: " + netView);
				return;
			}
			PhotonView value = null;
			if (photonViewList.TryGetValue(netView.ViewID, out value))
			{
				if (!(netView != value))
				{
					return;
				}
				UnityEngine.Debug.LogError(string.Format("PhotonView ID duplicate found: {0}. New: {1} old: {2}. Maybe one wasn't destroyed on scene load?! Check for 'DontDestroyOnLoad'. Destroying old entry, adding new.", netView.ViewID, netView, value));
				RemoveInstantiatedGO(value.gameObject, true);
			}
			photonViewList.Add(netView.ViewID, netView);
			if (LogLevel >= PunLogLevel.Full)
			{
				UnityEngine.Debug.Log("Registered PhotonView: " + netView.ViewID);
			}
		}

		public static void OpCleanActorRpcBuffer(int actorNumber)
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
			raiseEventOptions.TargetActors = new int[1] { actorNumber };
			RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
			RaiseEventInternal(200, null, raiseEventOptions2, SendOptions.SendReliable);
		}

		public static void OpRemoveCompleteCacheOfPlayer(int actorNumber)
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
			raiseEventOptions.TargetActors = new int[1] { actorNumber };
			RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
			RaiseEventInternal(0, null, raiseEventOptions2, SendOptions.SendReliable);
		}

		public static void OpRemoveCompleteCache()
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.RemoveFromRoomCache;
			raiseEventOptions.Receivers = ReceiverGroup.MasterClient;
			RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
			RaiseEventInternal(0, null, raiseEventOptions2, SendOptions.SendReliable);
		}

		private static void RemoveCacheOfLeftPlayers()
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary[244] = (byte)0;
			dictionary[247] = (byte)7;
			NetworkingClient.LoadBalancingPeer.SendOperation(253, dictionary, SendOptions.SendReliable);
		}

		public static void CleanRpcBufferIfMine(PhotonView view)
		{
			if (view.OwnerActorNr != NetworkingClient.LocalPlayer.ActorNumber && !NetworkingClient.LocalPlayer.IsMasterClient)
			{
				UnityEngine.Debug.LogError(string.Concat("Cannot remove cached RPCs on a PhotonView thats not ours! ", view.Owner, " scene: ", view.IsSceneView));
			}
			else
			{
				OpCleanRpcBuffer(view);
			}
		}

		public static void OpCleanRpcBuffer(PhotonView view)
		{
			rpcFilterByViewId[(byte)0] = view.ViewID;
			RaiseEventInternal(200, rpcFilterByViewId, OpCleanRpcBufferOptions, SendOptions.SendReliable);
		}

		public static void RemoveRPCsInGroup(int group)
		{
			foreach (PhotonView value in photonViewList.Values)
			{
				if (value.Group == group)
				{
					CleanRpcBufferIfMine(value);
				}
			}
		}

		public static void SetLevelPrefix(byte prefix)
		{
			currentLevelPrefix = prefix;
		}

		internal static void RPC(PhotonView view, string methodName, RpcTarget target, Player player, bool encrypt, params object[] parameters)
		{
			if (blockedSendingGroups.Contains(view.Group))
			{
				return;
			}
			if (view.ViewID < 1)
			{
				UnityEngine.Debug.LogError("Illegal view ID:" + view.ViewID + " method: " + methodName + " GO:" + view.gameObject.name);
			}
			if (LogLevel >= PunLogLevel.Full)
			{
				UnityEngine.Debug.Log(string.Concat("Sending RPC \"", methodName, "\" to target: ", target, " or player:", player, "."));
			}
			rpcEvent.Clear();
			rpcEvent[(byte)0] = view.ViewID;
			if (view.Prefix > 0)
			{
				rpcEvent[(byte)1] = (short)view.Prefix;
			}
			rpcEvent[(byte)2] = ServerTimestamp;
			int value = 0;
			if (rpcShortcuts.TryGetValue(methodName, out value))
			{
				rpcEvent[(byte)5] = (byte)value;
			}
			else
			{
				rpcEvent[(byte)3] = methodName;
			}
			if (parameters != null && parameters.Length > 0)
			{
				rpcEvent[(byte)4] = parameters;
			}
			SendOptions sendOptions = new SendOptions
			{
				Reliability = true,
				Encrypt = encrypt
			};
			if (player != null)
			{
				if (NetworkingClient.LocalPlayer.ActorNumber == player.ActorNumber)
				{
					ExecuteRpc(rpcEvent, player);
					return;
				}
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.TargetActors = new int[1] { player.ActorNumber };
				RaiseEventOptions raiseEventOptions2 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions2, sendOptions);
				return;
			}
			switch (target)
			{
			case RpcTarget.All:
				RpcOptionsToAll.InterestGroup = view.Group;
				RaiseEventInternal(200, rpcEvent, RpcOptionsToAll, sendOptions);
				ExecuteRpc(rpcEvent, NetworkingClient.LocalPlayer);
				break;
			case RpcTarget.Others:
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.InterestGroup = view.Group;
				RaiseEventOptions raiseEventOptions8 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions8, sendOptions);
				break;
			}
			case RpcTarget.AllBuffered:
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.CachingOption = EventCaching.AddToRoomCache;
				RaiseEventOptions raiseEventOptions6 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions6, sendOptions);
				ExecuteRpc(rpcEvent, NetworkingClient.LocalPlayer);
				break;
			}
			case RpcTarget.OthersBuffered:
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.CachingOption = EventCaching.AddToRoomCache;
				RaiseEventOptions raiseEventOptions4 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions4, sendOptions);
				break;
			}
			case RpcTarget.MasterClient:
			{
				if (NetworkingClient.LocalPlayer.IsMasterClient)
				{
					ExecuteRpc(rpcEvent, NetworkingClient.LocalPlayer);
					break;
				}
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.Receivers = ReceiverGroup.MasterClient;
				RaiseEventOptions raiseEventOptions7 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions7, sendOptions);
				break;
			}
			case RpcTarget.AllViaServer:
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.InterestGroup = view.Group;
				raiseEventOptions.Receivers = ReceiverGroup.All;
				RaiseEventOptions raiseEventOptions5 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions5, sendOptions);
				if (OfflineMode)
				{
					ExecuteRpc(rpcEvent, NetworkingClient.LocalPlayer);
				}
				break;
			}
			case RpcTarget.AllBufferedViaServer:
			{
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				raiseEventOptions.InterestGroup = view.Group;
				raiseEventOptions.Receivers = ReceiverGroup.All;
				raiseEventOptions.CachingOption = EventCaching.AddToRoomCache;
				RaiseEventOptions raiseEventOptions3 = raiseEventOptions;
				RaiseEventInternal(200, rpcEvent, raiseEventOptions3, sendOptions);
				if (OfflineMode)
				{
					ExecuteRpc(rpcEvent, NetworkingClient.LocalPlayer);
				}
				break;
			}
			default:
				UnityEngine.Debug.LogError("Unsupported target enum: " + target);
				break;
			}
		}

		public static void SetInterestGroups(byte[] disableGroups, byte[] enableGroups)
		{
			if (disableGroups != null)
			{
				if (disableGroups.Length == 0)
				{
					allowedReceivingGroups.Clear();
				}
				else
				{
					foreach (byte b in disableGroups)
					{
						if (b <= 0)
						{
							UnityEngine.Debug.LogError("Error: PhotonNetwork.SetInterestGroups was called with an illegal group number: " + b + ". The Group number should be at least 1.");
						}
						else if (allowedReceivingGroups.Contains(b))
						{
							allowedReceivingGroups.Remove(b);
						}
					}
				}
			}
			if (enableGroups != null)
			{
				if (enableGroups.Length == 0)
				{
					for (byte b2 = 0; b2 < byte.MaxValue; b2++)
					{
						allowedReceivingGroups.Add(b2);
					}
					allowedReceivingGroups.Add(byte.MaxValue);
				}
				else
				{
					foreach (byte b3 in enableGroups)
					{
						if (b3 <= 0)
						{
							UnityEngine.Debug.LogError("Error: PhotonNetwork.SetInterestGroups was called with an illegal group number: " + b3 + ". The Group number should be at least 1.");
						}
						else
						{
							allowedReceivingGroups.Add(b3);
						}
					}
				}
			}
			if (!offlineMode)
			{
				NetworkingClient.OpChangeGroups(disableGroups, enableGroups);
			}
		}

		public static void SetSendingEnabled(byte group, bool enabled)
		{
			if (!enabled)
			{
				blockedSendingGroups.Add(group);
			}
			else
			{
				blockedSendingGroups.Remove(group);
			}
		}

		public static void SetSendingEnabled(byte[] disableGroups, byte[] enableGroups)
		{
			if (disableGroups != null)
			{
				foreach (byte item in disableGroups)
				{
					blockedSendingGroups.Add(item);
				}
			}
			if (enableGroups != null)
			{
				foreach (byte item2 in enableGroups)
				{
					blockedSendingGroups.Remove(item2);
				}
			}
		}

		internal static void NewSceneLoaded()
		{
			if (loadingLevelAndPausedNetwork)
			{
				if (_AsyncLevelLoadingOperation != null)
				{
					_AsyncLevelLoadingOperation = null;
				}
				loadingLevelAndPausedNetwork = false;
				IsMessageQueueRunning = true;
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, PhotonView> photonView in photonViewList)
			{
				PhotonView value = photonView.Value;
				if (value == null)
				{
					list.Add(photonView.Key);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				int key = list[i];
				photonViewList.Remove(key);
			}
			if (list.Count > 0 && LogLevel >= PunLogLevel.Informational)
			{
				UnityEngine.Debug.Log("New level loaded. Removed " + list.Count + " scene view IDs from last level.");
			}
		}

		internal static void RunViewUpdate()
		{
			if (OfflineMode || CurrentRoom == null || CurrentRoom.Players == null || CurrentRoom.Players.Count <= 1)
			{
				return;
			}
			Dictionary<int, PhotonView>.Enumerator enumerator = photonViewList.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PhotonView value = enumerator.Current.Value;
				if (value.Synchronization == ViewSynchronization.Off || !value.IsMine || !value.isActiveAndEnabled || blockedSendingGroups.Contains(value.Group))
				{
					continue;
				}
				List<object> list = OnSerializeWrite(value);
				if (list != null)
				{
					RaiseEventBatch raiseEventBatch = new RaiseEventBatch
					{
						Reliable = (value.Synchronization == ViewSynchronization.ReliableDeltaCompressed || value.mixedModeIsReliable),
						Group = value.Group
					};
					SerializeViewBatch value2 = null;
					if (!serializeViewBatches.TryGetValue(raiseEventBatch, out value2))
					{
						value2 = new SerializeViewBatch(raiseEventBatch, 2);
						serializeViewBatches.Add(raiseEventBatch, value2);
					}
					value2.Add(list);
					if (value2.ObjectUpdates.Count == value2.ObjectUpdates.Capacity)
					{
						SendSerializeViewBatch(value2);
					}
				}
			}
			Dictionary<RaiseEventBatch, SerializeViewBatch>.Enumerator enumerator2 = serializeViewBatches.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				SendSerializeViewBatch(enumerator2.Current.Value);
			}
		}

		private static void SendSerializeViewBatch(SerializeViewBatch batch)
		{
			if (batch != null && batch.ObjectUpdates.Count > 2)
			{
				RaiseEventOptions raiseEventOptions = serializeRaiseEvOptions;
				RaiseEventBatch batch2 = batch.Batch;
				raiseEventOptions.InterestGroup = batch2.Group;
				batch.ObjectUpdates[0] = ServerTimestamp;
				batch.ObjectUpdates[1] = ((currentLevelPrefix == 0) ? null : ((object)currentLevelPrefix));
				RaiseEventBatch batch3 = batch.Batch;
				byte eventCode = (byte)((!batch3.Reliable) ? 201 : 206);
				List<object> objectUpdates = batch.ObjectUpdates;
				RaiseEventOptions raiseEventOptions2 = serializeRaiseEvOptions;
				RaiseEventBatch batch4 = batch.Batch;
				RaiseEventInternal(eventCode, objectUpdates, raiseEventOptions2, (!batch4.Reliable) ? SendOptions.SendUnreliable : SendOptions.SendReliable);
				batch.Clear();
			}
		}

		private static List<object> OnSerializeWrite(PhotonView view)
		{
			if (view.Synchronization == ViewSynchronization.Off)
			{
				return null;
			}
			PhotonMessageInfo info = new PhotonMessageInfo(NetworkingClient.LocalPlayer, ServerTimestamp, view);
			if (view.syncValues == null)
			{
				view.syncValues = new List<object>();
			}
			view.syncValues.Clear();
			serializeStreamOut.SetWriteStream(view.syncValues, 0);
			serializeStreamOut.SendNext(null);
			serializeStreamOut.SendNext(null);
			serializeStreamOut.SendNext(null);
			view.SerializeView(serializeStreamOut, info);
			if (serializeStreamOut.Count <= 3)
			{
				return null;
			}
			List<object> writeStream = serializeStreamOut.GetWriteStream();
			writeStream[0] = view.ViewID;
			writeStream[1] = false;
			writeStream[2] = null;
			if (view.Synchronization == ViewSynchronization.Unreliable)
			{
				return writeStream;
			}
			if (view.Synchronization == ViewSynchronization.UnreliableOnChange)
			{
				if (AlmostEquals(writeStream, view.lastOnSerializeDataSent))
				{
					if (view.mixedModeIsReliable)
					{
						return null;
					}
					view.mixedModeIsReliable = true;
					List<object> lastOnSerializeDataSent = view.lastOnSerializeDataSent;
					view.lastOnSerializeDataSent = writeStream;
					view.syncValues = lastOnSerializeDataSent;
				}
				else
				{
					view.mixedModeIsReliable = false;
					List<object> lastOnSerializeDataSent2 = view.lastOnSerializeDataSent;
					view.lastOnSerializeDataSent = writeStream;
					view.syncValues = lastOnSerializeDataSent2;
				}
				return writeStream;
			}
			if (view.Synchronization == ViewSynchronization.ReliableDeltaCompressed)
			{
				List<object> result = DeltaCompressionWrite(view.lastOnSerializeDataSent, writeStream);
				List<object> lastOnSerializeDataSent3 = view.lastOnSerializeDataSent;
				view.lastOnSerializeDataSent = writeStream;
				view.syncValues = lastOnSerializeDataSent3;
				return result;
			}
			return null;
		}

		private static void OnSerializeRead(object[] data, Player sender, int networkTime, short correctPrefix)
		{
			int num = (int)data[0];
			PhotonView photonView = GetPhotonView(num);
			if (photonView == null)
			{
				UnityEngine.Debug.LogWarning("Received OnSerialization for view ID " + num + ". We have no such PhotonView! Ignored this if you're leaving a room. State: " + NetworkingClient.State);
			}
			else if (photonView.Prefix > 0 && correctPrefix != photonView.Prefix)
			{
				UnityEngine.Debug.LogError("Received OnSerialization for view ID " + num + " with prefix " + correctPrefix + ". Our prefix is " + photonView.Prefix);
			}
			else
			{
				if (photonView.Group != 0 && !allowedReceivingGroups.Contains(photonView.Group))
				{
					return;
				}
				if (photonView.Synchronization == ViewSynchronization.ReliableDeltaCompressed)
				{
					object[] array = DeltaCompressionRead(photonView.lastOnSerializeDataReceived, data);
					if (array == null)
					{
						if (LogLevel >= PunLogLevel.Informational)
						{
							UnityEngine.Debug.Log("Skipping packet for " + photonView.name + " [" + photonView.ViewID + "] as we haven't received a full packet for delta compression yet. This is OK if it happens for the first few frames after joining a game.");
						}
						return;
					}
					photonView.lastOnSerializeDataReceived = array;
					data = array;
				}
				serializeStreamIn.SetReadStream(data, 3);
				photonView.DeserializeView(info: new PhotonMessageInfo(sender, networkTime, photonView), stream: serializeStreamIn);
			}
		}

		private static List<object> DeltaCompressionWrite(List<object> previousContent, List<object> currentContent)
		{
			if (currentContent == null || previousContent == null || previousContent.Count != currentContent.Count)
			{
				return currentContent;
			}
			if (currentContent.Count <= 3)
			{
				return null;
			}
			previousContent[1] = false;
			int num = 0;
			Queue<int> queue = null;
			for (int i = 3; i < currentContent.Count; i++)
			{
				object obj = currentContent[i];
				object two = previousContent[i];
				if (AlmostEquals(obj, two))
				{
					num++;
					previousContent[i] = null;
					continue;
				}
				previousContent[i] = obj;
				if (obj == null)
				{
					if (queue == null)
					{
						queue = new Queue<int>(currentContent.Count);
					}
					queue.Enqueue(i);
				}
			}
			if (num > 0)
			{
				if (num == currentContent.Count - 3)
				{
					return null;
				}
				previousContent[1] = true;
				if (queue != null)
				{
					previousContent[2] = queue.ToArray();
				}
			}
			previousContent[0] = currentContent[0];
			return previousContent;
		}

		private static object[] DeltaCompressionRead(object[] lastOnSerializeDataReceived, object[] incomingData)
		{
			if (!(bool)incomingData[1])
			{
				return incomingData;
			}
			if (lastOnSerializeDataReceived == null)
			{
				return null;
			}
			int[] array = incomingData[2] as int[];
			for (int i = 3; i < incomingData.Length; i++)
			{
				if ((array == null || !array.Contains(i)) && incomingData[i] == null)
				{
					object obj = lastOnSerializeDataReceived[i];
					incomingData[i] = obj;
				}
			}
			return incomingData;
		}

		private static bool AlmostEquals(IList<object> lastData, IList<object> currentContent)
		{
			if (lastData == null && currentContent == null)
			{
				return true;
			}
			if (lastData == null || currentContent == null || lastData.Count != currentContent.Count)
			{
				return false;
			}
			for (int i = 0; i < currentContent.Count; i++)
			{
				object one = currentContent[i];
				object two = lastData[i];
				if (!AlmostEquals(one, two))
				{
					return false;
				}
			}
			return true;
		}

		private static bool AlmostEquals(object one, object two)
		{
			if (one == null || two == null)
			{
				return one == null && two == null;
			}
			if (!one.Equals(two))
			{
				if (one is Vector3)
				{
					Vector3 target = (Vector3)one;
					Vector3 second = (Vector3)two;
					if (target.AlmostEquals(second, PrecisionForVectorSynchronization))
					{
						return true;
					}
				}
				else if (one is Vector2)
				{
					Vector2 target2 = (Vector2)one;
					Vector2 second2 = (Vector2)two;
					if (target2.AlmostEquals(second2, PrecisionForVectorSynchronization))
					{
						return true;
					}
				}
				else if (one is Quaternion)
				{
					Quaternion target3 = (Quaternion)one;
					Quaternion second3 = (Quaternion)two;
					if (target3.AlmostEquals(second3, PrecisionForQuaternionSynchronization))
					{
						return true;
					}
				}
				else if (one is float)
				{
					float target4 = (float)one;
					float second4 = (float)two;
					if (target4.AlmostEquals(second4, PrecisionForFloatSynchronization))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		internal static bool GetMethod(MonoBehaviour monob, string methodType, out MethodInfo mi)
		{
			mi = null;
			if (monob == null || string.IsNullOrEmpty(methodType))
			{
				return false;
			}
			List<MethodInfo> methods = SupportClass.GetMethods(monob.GetType(), null);
			for (int i = 0; i < methods.Count; i++)
			{
				MethodInfo methodInfo = methods[i];
				if (methodInfo.Name.Equals(methodType))
				{
					mi = methodInfo;
					return true;
				}
			}
			return false;
		}

		internal static void LoadLevelIfSynced()
		{
			if (!AutomaticallySyncScene || IsMasterClient || CurrentRoom == null || !CurrentRoom.CustomProperties.ContainsKey("curScn"))
			{
				return;
			}
			object obj = CurrentRoom.CustomProperties["curScn"];
			if (obj is int)
			{
				if (SceneManagerHelper.ActiveSceneBuildIndex != (int)obj)
				{
					LoadLevel((int)obj);
				}
			}
			else if (obj is string && SceneManagerHelper.ActiveSceneName != (string)obj)
			{
				LoadLevel((string)obj);
			}
		}

		internal static void SetLevelInPropsIfSynced(object levelId)
		{
			if (!AutomaticallySyncScene || !IsMasterClient || CurrentRoom == null)
			{
				return;
			}
			if (levelId == null)
			{
				UnityEngine.Debug.LogError("Parameter levelId can't be null!");
				return;
			}
			if (_AsyncLevelLoadingOperation != null)
			{
				_AsyncLevelLoadingOperation.allowSceneActivation = false;
				_AsyncLevelLoadingOperation = null;
			}
			if (CurrentRoom.CustomProperties.ContainsKey("curScn"))
			{
				object obj = CurrentRoom.CustomProperties["curScn"];
				if ((obj is int && SceneManagerHelper.ActiveSceneBuildIndex == (int)obj) || (obj is string && SceneManagerHelper.ActiveSceneName != null && SceneManagerHelper.ActiveSceneName.Equals((string)obj)))
				{
					return;
				}
				if (_AsyncLevelLoadingOperation != null)
				{
					bool flag = false;
					if ((obj is int && levelId is int && (int)levelId != (int)obj) || (obj is string && levelId is string && (string)levelId != (string)obj))
					{
						_AsyncLevelLoadingOperation.allowSceneActivation = false;
						_AsyncLevelLoadingOperation = null;
					}
				}
			}
			Hashtable hashtable = new Hashtable();
			if (levelId is int)
			{
				hashtable["curScn"] = (int)levelId;
			}
			else if (levelId is string)
			{
				hashtable["curScn"] = (string)levelId;
			}
			else
			{
				UnityEngine.Debug.LogError("Parameter levelId must be int or string!");
			}
			CurrentRoom.SetCustomProperties(hashtable);
			SendAllOutgoingCommands();
		}

		private static void OnEvent(EventData photonEvent)
		{
			int num = 0;
			Player player = null;
			if (photonEvent.Parameters.ContainsKey(254))
			{
				num = (int)photonEvent[254];
				if (NetworkingClient.CurrentRoom != null)
				{
					player = NetworkingClient.CurrentRoom.GetPlayer(num);
				}
			}
			switch (photonEvent.Code)
			{
			case byte.MaxValue:
				ResetPhotonViewsOnSerialize();
				break;
			case 200:
				ExecuteRpc(photonEvent[245] as Hashtable, player);
				break;
			case 201:
			case 206:
			{
				object[] array3 = (object[])photonEvent[245];
				int networkTime = (int)array3[0];
				short correctPrefix = (short)((array3[1] != null) ? ((short)array3[1]) : 0);
				object[] array4 = null;
				for (int i = 2; i < array3.Length; i++)
				{
					array4 = array3[i] as object[];
					if (array4 == null)
					{
						break;
					}
					OnSerializeRead(array4, player, networkTime, correctPrefix);
				}
				break;
			}
			case 202:
				NetworkInstantiate((Hashtable)photonEvent[245], player);
				break;
			case 203:
				if (player == null || !player.IsMasterClient)
				{
					UnityEngine.Debug.LogError(string.Concat("Error: Someone else(", player, ") then the masterserver requests a disconnect!"));
				}
				else
				{
					LeaveRoom(false);
				}
				break;
			case 207:
			{
				Hashtable hashtable = (Hashtable)photonEvent[245];
				int num4 = (int)hashtable[(byte)0];
				if (num4 >= 0)
				{
					DestroyPlayerObjects(num4, true);
				}
				else
				{
					DestroyAll(true);
				}
				break;
			}
			case 254:
				if (CurrentRoom != null && CurrentRoom.AutoCleanUp && CurrentRoom.GetPlayer(num) == null)
				{
					DestroyPlayerObjects(num, true);
				}
				break;
			case 204:
			{
				Hashtable hashtable = (Hashtable)photonEvent[245];
				int num5 = (int)hashtable[(byte)0];
				PhotonView value = null;
				if (photonViewList.TryGetValue(num5, out value))
				{
					RemoveInstantiatedGO(value.gameObject, true);
					break;
				}
				UnityEngine.Debug.LogError("Ev Destroy Failed. Could not find PhotonView with instantiationId " + num5 + ". Sent by actorNr: " + num);
				break;
			}
			case 209:
			{
				int[] array2 = (int[])photonEvent.Parameters[245];
				int num6 = array2[0];
				int num7 = array2[1];
				PhotonView photonView2 = PhotonView.Find(num6);
				if (photonView2 == null)
				{
					UnityEngine.Debug.LogWarning("Can't find PhotonView of incoming OwnershipRequest. ViewId not found: " + num6);
					break;
				}
				if (LogLevel == PunLogLevel.Informational)
				{
					UnityEngine.Debug.Log(string.Format("OwnershipRequest. actorNr {0} requests view {1} from {2}. current pv owner: {3} is {4}. isMine: {6} master client: {5}", num, num6, num7, photonView2.OwnerActorNr, (!photonView2.IsOwnerActive) ? "inactive" : "active", MasterClient.ActorNumber, photonView2.IsMine));
				}
				switch (photonView2.OwnershipTransfer)
				{
				case OwnershipOption.Takeover:
				{
					int ownerActorNr2 = photonView2.OwnerActorNr;
					if (num7 == ownerActorNr2 || (num7 == 0 && ownerActorNr2 == MasterClient.ActorNumber) || ownerActorNr2 == 0)
					{
						Player player3 = CurrentRoom.GetPlayer(ownerActorNr2);
						photonView2.OwnerActorNr = num;
						photonView2.OwnershipWasTransfered = true;
						if (OnOwnershipTransferedEv__BackingField != null)
						{
							OnOwnershipTransferedEv__BackingField(photonView2, player3);
						}
					}
					else
					{
						UnityEngine.Debug.LogWarning("requestedView.OwnershipTransfer was ignored! ");
					}
					break;
				}
				case OwnershipOption.Request:
					if (OnOwnershipRequestEv__BackingField != null)
					{
						OnOwnershipRequestEv__BackingField(photonView2, player);
					}
					break;
				default:
					UnityEngine.Debug.LogWarning(string.Concat("Ownership mode == ", photonView2.OwnershipTransfer, ". Ignoring request."));
					break;
				}
				break;
			}
			case 210:
			{
				int[] array = (int[])photonEvent.Parameters[245];
				int num2 = array[0];
				int num3 = array[1];
				if (LogLevel >= PunLogLevel.Informational)
				{
					UnityEngine.Debug.Log("Ev OwnershipTransfer. ViewID " + num2 + " to: " + num3 + " Time: " + Environment.TickCount % 1000);
				}
				PhotonView photonView = PhotonView.Find(num2);
				if (photonView != null)
				{
					int ownerActorNr = photonView.OwnerActorNr;
					photonView.OwnershipWasTransfered = true;
					photonView.OwnerActorNr = num3;
					Player player2 = CurrentRoom.GetPlayer(ownerActorNr);
					if (OnOwnershipTransferedEv__BackingField != null)
					{
						OnOwnershipTransferedEv__BackingField(photonView, player2);
					}
				}
				break;
			}
			}
		}

		private static void OnOperation(OperationResponse opResponse)
		{
			byte operationCode = opResponse.OperationCode;
			if (operationCode == 220 && ConnectMethod == ConnectMethod.ConnectToBest)
			{
				string bestRegionSummaryInPreferences = BestRegionSummaryInPreferences;
				if (LogLevel >= PunLogLevel.Informational)
				{
					UnityEngine.Debug.Log("PUN got region list. Going to ping minimum regions, based on this previous result summary: " + bestRegionSummaryInPreferences);
				}
				NetworkingClient.RegionHandler.PingMinimumOfRegions(OnRegionsPinged, bestRegionSummaryInPreferences);
			}
		}

		private static void OnRegionsPinged(RegionHandler regionHandler)
		{
			if (LogLevel >= PunLogLevel.Informational)
			{
				foreach (Region enabledRegion in regionHandler.EnabledRegions)
				{
					UnityEngine.Debug.Log(enabledRegion.ToString());
				}
			}
			_cachedRegionHandler = regionHandler;
			NetworkingClient.ConnectToRegionMaster(regionHandler.BestRegion.Code);
		}
	}
}
