using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.PUN
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Photon Voice/Photon Voice Network")]
	public class PhotonVoiceNetwork : VoiceConnection
	{
		public const string VoiceRoomNameSuffix = "_voice_";

		public bool AutoConnectAndJoin = true;

		public bool AutoLeaveAndDisconnect = true;

		public bool AutoCreateSpeakerIfNotFound = true;

		private RoomOptions voiceRoomOptions = new RoomOptions
		{
			IsVisible = false
		};

		private bool clientCalledConnectAndJoin;

		private bool clientCalledDisconnect;

		private static object instanceLock = new object();

		private static PhotonVoiceNetwork instance;

		private static bool instantiated;

		private static bool applicationIsQuitting;

		public static PhotonVoiceNetwork Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (applicationIsQuitting)
					{
						if (instance.Logger.IsWarningEnabled)
						{
							instance.Logger.LogWarning("PhotonVoiceNetwork Instance already destroyed on application quit. Won't create again - returning null.");
						}
						return null;
					}
					if (!instantiated)
					{
						PhotonVoiceNetwork[] array = Object.FindObjectsOfType<PhotonVoiceNetwork>();
						if (array == null || array.Length < 1)
						{
							GameObject gameObject = new GameObject();
							gameObject.name = "PhotonVoiceNetwork singleton";
							instance = gameObject.AddComponent<PhotonVoiceNetwork>();
							if (instance.Logger.IsWarningEnabled)
							{
								instance.Logger.LogWarning("An instance of PhotonVoiceNetwork was automatically created in the scene.");
							}
						}
						else if (array.Length >= 1)
						{
							instance = array[0];
							if (array.Length > 1 && instance.Logger.IsWarningEnabled)
							{
								instance.Logger.LogWarning("{0} PhotonVoiceNetwork instances found. Using first one only.", array.Length);
							}
						}
						instantiated = true;
					}
					return instance;
				}
			}
			set
			{
				lock (instanceLock)
				{
					if (value == null)
					{
						if (instance.Logger.IsWarningEnabled)
						{
							instance.Logger.LogWarning("Cannot set PhotonVoiceNetwork.Instance to null.");
						}
					}
					else if (instantiated)
					{
						if (instance.GetInstanceID() != value.GetInstanceID() && instance.Logger.IsWarningEnabled)
						{
							instance.Logger.LogWarning("An instance of PhotonVoiceNetwork is already set.");
						}
					}
					else
					{
						instantiated = true;
						instance = value;
					}
				}
			}
		}

		public bool ConnectAndJoinRoom()
		{
			if (!PhotonNetwork.InRoom)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Cannot connect and join if PUN is not joined.");
				}
				return false;
			}
			AppSettings overwriteSettings = null;
			if (usePunSettings)
			{
				overwriteSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
			}
			if (ConnectUsingSettings(overwriteSettings))
			{
				clientCalledConnectAndJoin = true;
				clientCalledDisconnect = false;
				return true;
			}
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Connecting to server failed.");
			}
			return false;
		}

		public void Disconnect()
		{
			if (!base.Client.IsConnected)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Cannot Disconnect if not connected.");
				}
			}
			else
			{
				clientCalledDisconnect = true;
				clientCalledConnectAndJoin = false;
				base.Client.Disconnect();
			}
		}

		protected override void Awake()
		{
			base.Awake();
			Instance = this;
		}

		private void OnEnable()
		{
			PhotonNetwork.NetworkingClient.StateChanged += OnPunStateChanged;
			FollowPun();
			clientCalledConnectAndJoin = false;
			clientCalledDisconnect = false;
		}

		private void OnDisable()
		{
			PhotonNetwork.NetworkingClient.StateChanged -= OnPunStateChanged;
		}

		protected override void OnApplicationQuit()
		{
			applicationIsQuitting = true;
			base.OnApplicationQuit();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			lock (instanceLock)
			{
				instantiated = false;
			}
		}

		private void OnPunStateChanged(ClientState fromState, ClientState toState)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("OnPunStateChanged from {0} to {1}", fromState, toState);
			}
			FollowPun(toState);
		}

		protected override void OnVoiceStateChanged(ClientState fromState, ClientState toState)
		{
			base.OnVoiceStateChanged(fromState, toState);
			if (!clientCalledDisconnect && toState == ClientState.Disconnected && base.Client.DisconnectedCause == DisconnectCause.DisconnectByClientLogic)
			{
				clientCalledDisconnect = true;
			}
			FollowPun(toState);
		}

		private void FollowPun(ClientState toState)
		{
			if (toState == ClientState.Joined || toState == ClientState.Disconnected || toState == ClientState.ConnectedToMasterserver)
			{
				FollowPun();
			}
		}

		internal override Speaker SimpleSpeakerFactory(int playerId, byte voiceId, object userData)
		{
			if (!(userData is int))
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("UserData ({0}) does not contain PhotonViewId. Remote voice {1}/{2} not linked", (userData != null) ? userData.ToString() : "null", playerId, voiceId);
				}
				return null;
			}
			int viewID = (int)userData;
			PhotonView photonView = PhotonView.Find(viewID);
			if (photonView == null)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("No PhotonView with ID {0} found. Remote voice {1}/{2} not linked.", userData, playerId, voiceId);
				}
				return null;
			}
			PhotonVoiceView component = photonView.GetComponent<PhotonVoiceView>();
			if (component == null)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("No PhotonVoiceView attached to the PhotonView with ID {0}. Remote voice {1}/{2} not linked.", userData, playerId, voiceId);
				}
				return null;
			}
			if (!component.IsSpeaker)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("No Speaker found for the PhotonView with ID {0}. Remote voice {1}/{2} not linked.", userData, playerId, voiceId);
				}
				return null;
			}
			return component.SpeakerInUse;
		}

		internal static string GetVoiceRoomName()
		{
			if (PhotonNetwork.InRoom)
			{
				return string.Format("{0}{1}", PhotonNetwork.CurrentRoom.Name, "_voice_");
			}
			return null;
		}

		private void ConnectOrJoin()
		{
			switch (base.ClientState)
			{
			case ClientState.PeerCreated:
			case ClientState.Disconnected:
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("PUN joined room, now connecting Voice client");
				}
				if (!ConnectUsingSettings() && base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Connecting to server failed.");
				}
				break;
			case ClientState.ConnectedToMasterserver:
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("PUN joined room, now joining Voice room");
				}
				JoinRoom(GetVoiceRoomName());
				break;
			default:
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("PUN joined room, Voice client is busy ({0}). Is this expected?", base.ClientState);
				}
				break;
			}
		}

		private void JoinRoom(string voiceRoomName)
		{
			if (string.IsNullOrEmpty(voiceRoomName))
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Voice room name is null or empty.");
				}
			}
			else
			{
				base.Client.OpJoinOrCreateRoom(new EnterRoomParams
				{
					RoomName = voiceRoomName,
					RoomOptions = voiceRoomOptions
				});
			}
		}

		private void FollowPun()
		{
			if (applicationIsQuitting)
			{
				return;
			}
			if (PhotonNetwork.NetworkClientState == base.ClientState)
			{
				if (PhotonNetwork.NetworkClientState != ClientState.Joined || !AutoConnectAndJoin)
				{
					return;
				}
				string voiceRoomName = GetVoiceRoomName();
				string text = base.Client.CurrentRoom.Name;
				if (!text.Equals(voiceRoomName))
				{
					if (base.Logger.IsWarningEnabled)
					{
						base.Logger.LogWarning("Voice room mismatch: Expected:\"{0}\" Current:\"{1}\", leaving the second to join the first.", voiceRoomName, text);
					}
					base.Client.OpLeaveRoom(false);
				}
			}
			else if (PhotonNetwork.InRoom)
			{
				if (clientCalledConnectAndJoin || (AutoConnectAndJoin && !clientCalledDisconnect))
				{
					ConnectOrJoin();
				}
			}
			else if (base.ClientState == ClientState.Joined && AutoLeaveAndDisconnect && !clientCalledConnectAndJoin)
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("PUN left room, disconnecting Voice");
				}
				base.Client.Disconnect();
			}
		}

		internal void CheckLateLinking(PhotonVoiceView photonVoiceView, int viewId)
		{
			if (!base.Client.InRoom || !(photonVoiceView != null) || viewId <= 0)
			{
				return;
			}
			for (int i = 0; i < cachedRemoteVoices.Count; i++)
			{
				RemoteVoiceLink remoteVoiceLink = cachedRemoteVoices[i];
				if (!(remoteVoiceLink.Info.UserData is int))
				{
					continue;
				}
				int num = (int)remoteVoiceLink.Info.UserData;
				if (viewId == num)
				{
					Speaker speakerInUse = photonVoiceView.SpeakerInUse;
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Speaker 'late-linking' for the PhotonView with ID {0} with remote voice {1}/{2}.", viewId, remoteVoiceLink.PlayerId, remoteVoiceLink.VoiceId);
					}
					LinkSpeaker(speakerInUse, remoteVoiceLink);
					break;
				}
			}
		}
	}
}
