using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Voice.Unity
{
	[AddComponentMenu("Photon Voice/Voice Connection")]
	public class VoiceConnection : ConnectionHandler, ILoggable
	{
		private VoiceLogger logger;

		[SerializeField]
		private DebugLevel logLevel = DebugLevel.ERROR;

		private const string PlayerPrefsKey = "VoiceCloudBestRegion";

		[SerializeField]
		protected bool usePunSettings = true;

		private LoadBalancingTransport client;

		[SerializeField]
		private bool enableSupportLogger;

		private SupportLogger supportLoggerComponent;

		[SerializeField]
		private int updateInterval = 50;

		private int nextSendTickCount;

		private RegionHandler cachedRegionHandler;

		[SerializeField]
		private bool runInBackground = true;

		[SerializeField]
		private int statsResetInterval = 1000;

		private int nextStatsTickCount;

		private float statsReferenceTime;

		private int referenceFramesLost;

		private int referenceFramesReceived;

		[SerializeField]
		private GameObject speakerPrefab;

		private bool cleanedUp;

		protected List<RemoteVoiceLink> cachedRemoteVoices = new List<RemoteVoiceLink>();

		public AppSettings Settings;

		public Recorder PrimaryRecorder;

		public Func<int, byte, object, Speaker> SpeakerFactory;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<Speaker> SpeakerLinked__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<RemoteVoiceLink> RemoteVoiceAdded__BackingField;

		public VoiceLogger Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new VoiceLogger(this, string.Format("{0}.{1}", base.name, GetType().Name), logLevel);
				}
				return logger;
			}
			protected set
			{
				logger = value;
			}
		}

		public DebugLevel LogLevel
		{
			get
			{
				if (Logger != null)
				{
					logLevel = Logger.LogLevel;
				}
				return logLevel;
			}
			set
			{
				logLevel = value;
				if (Logger != null)
				{
					Logger.LogLevel = logLevel;
				}
			}
		}

		public new LoadBalancingTransport Client
		{
			get
			{
				if (client == null)
				{
					client = new LoadBalancingTransport();
					VoiceClient voiceClient = client.VoiceClient;
					voiceClient.OnRemoteVoiceInfoAction = (VoiceClient.RemoteVoiceInfoDelegate)Delegate.Combine(voiceClient.OnRemoteVoiceInfoAction, new VoiceClient.RemoteVoiceInfoDelegate(OnRemoteVoiceInfo));
					client.OpResponseReceived += OnOperationResponse;
					client.StateChanged += OnVoiceStateChanged;
					base.Client = client;
					StartFallbackSendAckThread();
				}
				return client;
			}
		}

		public VoiceClient VoiceClient
		{
			get
			{
				return Client.VoiceClient;
			}
		}

		public ClientState ClientState
		{
			get
			{
				return Client.State;
			}
		}

		public float FramesReceivedPerSecond { get; private set; }

		public float FramesLostPerSecond { get; private set; }

		public float FramesLostPercent { get; private set; }

		public GameObject SpeakerPrefab
		{
			get
			{
				return speakerPrefab;
			}
			set
			{
				if (!(value != speakerPrefab))
				{
					return;
				}
				if (value != null && value.GetComponentInChildren<Speaker>() == null)
				{
					if (Logger.IsErrorEnabled)
					{
						Logger.LogError("SpeakerPrefab must have a component of type Speaker in its hierarchy.");
					}
				}
				else
				{
					speakerPrefab = value;
				}
			}
		}

		internal string BestRegionSummaryInPreferences
		{
			get
			{
				if (cachedRegionHandler != null)
				{
					BestRegionSummaryInPreferences = cachedRegionHandler.SummaryToCache;
					return cachedRegionHandler.SummaryToCache;
				}
				return PlayerPrefs.GetString("VoiceCloudBestRegion", null);
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					PlayerPrefs.DeleteKey("VoiceCloudBestRegion");
				}
				else
				{
					PlayerPrefs.SetString("VoiceCloudBestRegion", value);
				}
			}
		}

		public event Action<Speaker> SpeakerLinked
		{
			add
			{
				Action<Speaker> action = SpeakerLinked__BackingField;
				Action<Speaker> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref SpeakerLinked__BackingField, (Action<Speaker>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<Speaker> action = SpeakerLinked__BackingField;
				Action<Speaker> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref SpeakerLinked__BackingField, (Action<Speaker>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public event Action<RemoteVoiceLink> RemoteVoiceAdded
		{
			add
			{
				Action<RemoteVoiceLink> action = RemoteVoiceAdded__BackingField;
				Action<RemoteVoiceLink> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref RemoteVoiceAdded__BackingField, (Action<RemoteVoiceLink>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<RemoteVoiceLink> action = RemoteVoiceAdded__BackingField;
				Action<RemoteVoiceLink> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref RemoteVoiceAdded__BackingField, (Action<RemoteVoiceLink>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public bool ConnectUsingSettings(AppSettings overwriteSettings = null)
		{
			if (Client.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
			{
				if (Logger.IsWarningEnabled)
				{
					Logger.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: {0}", Client.LoadBalancingPeer.PeerState);
				}
				return false;
			}
			if (overwriteSettings != null)
			{
				Settings = overwriteSettings;
			}
			else if (usePunSettings)
			{
				Settings = PhotonNetwork.PhotonServerSettings.AppSettings;
			}
			if (Settings == null)
			{
				if (Logger.IsErrorEnabled)
				{
					Logger.LogError("Settings are null");
				}
				return false;
			}
			if (Settings.Protocol == ConnectionProtocol.Tcp)
			{
				if (!Settings.IsMasterServerAddress)
				{
					if (Logger.IsWarningEnabled)
					{
						Logger.LogWarning("Requested protocol not supported on Photon Cloud {0}. Switched to UDP.", Settings.Protocol);
					}
					Client.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Udp;
				}
				else
				{
					Client.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Tcp;
				}
			}
			else if (Settings.Protocol != ConnectionProtocol.Udp)
			{
				if (Logger.IsWarningEnabled)
				{
					Logger.LogWarning("Requested protocol not supported: {0}. Switched to UDP.", Settings.Protocol);
				}
				Client.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Udp;
			}
			Client.EnableLobbyStatistics = Settings.EnableLobbyStatistics;
			Client.LoadBalancingPeer.DebugOut = Settings.NetworkLogging;
			if (Settings.IsMasterServerAddress)
			{
				Client.LoadBalancingPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV16;
				if (string.IsNullOrEmpty(Client.UserId))
				{
					if (!string.IsNullOrEmpty(PhotonNetwork.NetworkingClient.UserId))
					{
						Client.UserId = PhotonNetwork.NetworkingClient.UserId;
					}
					else
					{
						Client.UserId = Guid.NewGuid().ToString();
					}
				}
				Client.IsUsingNameServer = false;
				Client.MasterServerAddress = ((Settings.Port != 0) ? string.Format("{0}:{1}", Settings.Server, Settings.Port) : Settings.Server);
				return Client.Connect();
			}
			Client.AppId = Settings.AppIdVoice;
			Client.AppVersion = Settings.AppVersion;
			if (!Settings.IsDefaultNameServer)
			{
				Client.NameServerHost = Settings.Server;
			}
			if (Settings.IsBestRegion)
			{
				return Client.ConnectToNameServer();
			}
			return Client.ConnectToRegionMaster(Settings.FixedRegion);
		}

		protected override void Awake()
		{
			base.Awake();
			if (SpeakerFactory == null)
			{
				SpeakerFactory = SimpleSpeakerFactory;
			}
			if (enableSupportLogger)
			{
				supportLoggerComponent = base.gameObject.AddComponent<SupportLogger>();
				supportLoggerComponent.Client = Client;
				supportLoggerComponent.LogTrafficStats = true;
			}
			if (runInBackground)
			{
				Application.runInBackground = runInBackground;
			}
		}

		protected virtual void Update()
		{
			VoiceClient.Service();
		}

		protected virtual void FixedUpdate()
		{
			bool flag = true;
			while (flag)
			{
				flag = Client.LoadBalancingPeer.DispatchIncomingCommands();
			}
		}

		private void LateUpdate()
		{
			int num = (int)(Time.realtimeSinceStartup * 1000f);
			if (num > nextSendTickCount)
			{
				bool flag = true;
				while (flag)
				{
					flag = Client.LoadBalancingPeer.SendOutgoingCommands();
				}
				nextSendTickCount = num + updateInterval;
			}
			if (num > nextStatsTickCount && statsResetInterval > 0)
			{
				CalcStatistics();
				nextStatsTickCount = num + statsResetInterval;
			}
		}

		protected override void OnDestroy()
		{
			CleanUp();
		}

		internal virtual Speaker SimpleSpeakerFactory(int playerId, byte voiceId, object userData)
		{
			Speaker speaker;
			if ((bool)SpeakerPrefab)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(SpeakerPrefab);
				speaker = gameObject.GetComponentInChildren<Speaker>();
				if (speaker == null)
				{
					if (Logger.IsErrorEnabled)
					{
						Logger.LogError("SpeakerPrefab does not have a component of type Speaker in its hierarchy.");
					}
					return null;
				}
			}
			else
			{
				speaker = new GameObject().AddComponent<Speaker>();
			}
			speaker.Actor = ((Client.CurrentRoom == null) ? null : Client.CurrentRoom.GetPlayer(playerId));
			speaker.name = ((speaker.Actor == null || string.IsNullOrEmpty(speaker.Actor.NickName)) ? string.Format("Speaker for Player {0} Voice #{1}", playerId, voiceId) : speaker.Actor.NickName);
			Speaker speaker2 = speaker;
			speaker2.OnRemoteVoiceRemoveAction = (Action<Speaker>)Delegate.Combine(speaker2.OnRemoteVoiceRemoveAction, new Action<Speaker>(DeleteVoiceOnRemoteVoiceRemove));
			return speaker;
		}

		internal void DeleteVoiceOnRemoteVoiceRemove(Speaker speaker)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Remote voice removed, delete speaker");
			}
			UnityEngine.Object.Destroy(speaker.gameObject);
		}

		private void OnRemoteVoiceInfo(int channelId, int playerId, byte voiceId, VoiceInfo voiceInfo, ref RemoteVoiceOptions options)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("OnRemoteVoiceInfo channel {0} player {1} voice #{2} userData {3}", channelId, playerId, voiceId, voiceInfo.UserData);
			}
			bool flag = false;
			for (int i = 0; i < cachedRemoteVoices.Count; i++)
			{
				RemoteVoiceLink remoteVoiceLink = cachedRemoteVoices[i];
				if (remoteVoiceLink.PlayerId == playerId && remoteVoiceLink.VoiceId == voiceId)
				{
					if (Logger.IsWarningEnabled)
					{
						Logger.LogWarning("Duplicate remote voice info event channel {0} player {1} voice #{2} userData {3}", channelId, playerId, voiceId, voiceInfo.UserData);
					}
					flag = true;
					cachedRemoteVoices.RemoveAt(i);
					break;
				}
			}
			RemoteVoiceLink remoteVoice = new RemoteVoiceLink(voiceInfo, playerId, voiceId, channelId, ref options);
			cachedRemoteVoices.Add(remoteVoice);
			if (RemoteVoiceAdded__BackingField != null)
			{
				RemoteVoiceAdded__BackingField(remoteVoice);
			}
			remoteVoice.RemoteVoiceRemoved += delegate
			{
				if (Logger.IsInfoEnabled)
				{
					Logger.LogInfo("RemoteVoiceRemoved channel {0} player {1} voice #{2} userData {3}", channelId, playerId, voiceId, voiceInfo.UserData);
				}
				if (!cachedRemoteVoices.Remove(remoteVoice) && Logger.IsWarningEnabled)
				{
					Logger.LogWarning("Cached remote voice info not removed for channel {0} player {1} voice #{2} userData {3}", channelId, playerId, voiceId, voiceInfo.UserData);
				}
			};
			if (SpeakerFactory == null)
			{
				return;
			}
			Speaker speaker = SpeakerFactory(playerId, voiceId, voiceInfo.UserData);
			if (speaker != null && flag && speaker.IsLinked)
			{
				if (Logger.IsWarningEnabled)
				{
					Logger.LogWarning("Overriding speaker link for channel {0} player {1} voice #{2} userData {3}", channelId, playerId, voiceId, voiceInfo.UserData);
				}
				speaker.OnRemoteVoiceRemove();
			}
			LinkSpeaker(speaker, remoteVoice);
		}

		private void OnOperationResponse(OperationResponse opResponse)
		{
			byte operationCode = opResponse.OperationCode;
			if (operationCode == 220 && Settings != null && Settings.IsBestRegion && Client.RegionHandler != null)
			{
				Client.RegionHandler.PingMinimumOfRegions(OnRegionsPinged, BestRegionSummaryInPreferences);
			}
		}

		protected virtual void OnVoiceStateChanged(ClientState fromState, ClientState toState)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.LogDebug("OnVoiceStateChanged from {0} to {1}", fromState, toState);
			}
			if (fromState == ClientState.Joined)
			{
				ClearRemoteVoicesCache();
			}
		}

		private void OnRegionsPinged(RegionHandler regionHandler)
		{
			cachedRegionHandler = regionHandler;
			Client.ConnectToRegionMaster(regionHandler.BestRegion.Code);
		}

		protected override void OnApplicationQuit()
		{
			CleanUp();
			SupportClass.StopAllBackgroundCalls();
		}

		protected void CalcStatistics()
		{
			float time = Time.time;
			int num = VoiceClient.FramesReceived - referenceFramesReceived;
			int num2 = VoiceClient.FramesLost - referenceFramesLost;
			float num3 = time - statsReferenceTime;
			if (num3 != 0f && num + num2 > 0)
			{
				FramesReceivedPerSecond = (float)num / num3;
				FramesLostPerSecond = (float)num2 / num3;
				FramesLostPercent = 100 * num2 / (num + num2);
			}
			referenceFramesReceived = VoiceClient.FramesReceived;
			referenceFramesLost = VoiceClient.FramesLost;
			statsReferenceTime = time;
		}

		private void CleanUp()
		{
			bool flag = client != null;
			if (Logger.IsDebugEnabled)
			{
				Logger.LogInfo("Client exists? {0}, already cleaned up? {1}", flag, cleanedUp);
			}
			if (cleanedUp)
			{
				return;
			}
			StopFallbackSendAckThread();
			if (flag)
			{
				client.OpResponseReceived -= OnOperationResponse;
				client.StateChanged -= OnVoiceStateChanged;
				client.Disconnect();
				if (client.LoadBalancingPeer != null)
				{
					client.LoadBalancingPeer.Disconnect();
					client.LoadBalancingPeer.StopThread();
				}
				client.Dispose();
			}
			cleanedUp = true;
		}

		protected void LinkSpeaker(Speaker speaker, RemoteVoiceLink remoteVoice)
		{
			if (speaker != null)
			{
				if (speaker.IsLinked)
				{
					if (Logger.IsWarningEnabled)
					{
						Logger.LogWarning("Speaker already linked. Remote voice {0}/{1} not linked.", remoteVoice.PlayerId, remoteVoice.VoiceId);
					}
					return;
				}
				speaker.OnRemoteVoiceInfo(remoteVoice);
				if (speaker.Actor == null && Client.CurrentRoom != null)
				{
					speaker.Actor = Client.CurrentRoom.GetPlayer(remoteVoice.PlayerId);
				}
				if (Logger.IsInfoEnabled)
				{
					Logger.LogInfo("Speaker linked with remote voice {0}/{1}", remoteVoice.PlayerId, remoteVoice.VoiceId);
				}
				if (SpeakerLinked__BackingField != null)
				{
					SpeakerLinked__BackingField(speaker);
				}
			}
			else if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Speaker is null. Remote voice {0}/{1} not linked.", remoteVoice.PlayerId, remoteVoice.VoiceId);
			}
		}

		private void ClearRemoteVoicesCache()
		{
			if (cachedRemoteVoices.Count > 0)
			{
				if (Logger.IsInfoEnabled)
				{
					Logger.LogInfo("{0} cached remote voices info cleared", cachedRemoteVoices.Count);
				}
				cachedRemoteVoices.Clear();
			}
		}
	}
}
