using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun
{
	internal class PhotonHandler : ConnectionHandler, IInRoomCallbacks, IMatchmakingCallbacks
	{
		internal static PhotonHandler Instance;

		protected internal static bool AppQuits;

		protected internal int UpdateInterval;

		protected internal int UpdateIntervalOnSerialize;

		private int nextSendTickCount;

		private int nextSendTickCountOnSerialize;

		private SupportLogger supportLoggerComponent;

		public static int MaxDatagrams = 10;

		public static bool SendAsap;

		private const int SerializeRateFrameCorrection = 8;

		protected override void Awake()
		{
			if (Instance != null && Instance != this && Instance.gameObject != null)
			{
				Object.DestroyImmediate(Instance.gameObject);
			}
			Instance = this;
			base.Client = PhotonNetwork.NetworkingClient;
			base.Awake();
			if (PhotonNetwork.PhotonServerSettings.EnableSupportLogger)
			{
				supportLoggerComponent = base.gameObject.AddComponent<SupportLogger>();
				supportLoggerComponent.Client = PhotonNetwork.NetworkingClient;
				supportLoggerComponent.LogTrafficStats = true;
			}
			UpdateInterval = 1000 / PhotonNetwork.SendRate;
			UpdateIntervalOnSerialize = 1000 / PhotonNetwork.SerializationRate;
			StartFallbackSendAckThread();
		}

		public virtual void OnEnable()
		{
			PhotonNetwork.AddCallbackTarget(this);
		}

		public virtual void OnDisable()
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		protected void Start()
		{
			SceneManager.sceneLoaded += delegate
			{
				PhotonNetwork.NewSceneLoaded();
				PhotonNetwork.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
			};
		}

		protected override void OnApplicationQuit()
		{
			AppQuits = true;
			base.OnApplicationQuit();
		}

		protected void FixedUpdate()
		{
			if (PhotonNetwork.NetworkingClient == null)
			{
				Debug.LogError("NetworkPeer broke!");
				return;
			}
			bool flag = true;
			while (PhotonNetwork.IsMessageQueueRunning && flag)
			{
				flag = PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
			}
		}

		protected void LateUpdate()
		{
			int num = (int)(Time.realtimeSinceStartup * 1000f);
			if (PhotonNetwork.IsMessageQueueRunning && num > nextSendTickCountOnSerialize)
			{
				PhotonNetwork.RunViewUpdate();
				nextSendTickCountOnSerialize = num + UpdateIntervalOnSerialize - 8;
				nextSendTickCount = 0;
			}
			num = (int)(Time.realtimeSinceStartup * 1000f);
			if (SendAsap || num > nextSendTickCount)
			{
				SendAsap = false;
				bool flag = true;
				int num2 = 0;
				while (PhotonNetwork.IsMessageQueueRunning && flag && num2 < MaxDatagrams)
				{
					flag = PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
					num2++;
				}
				nextSendTickCount = num + UpdateInterval;
			}
		}

		public void OnJoinedRoom()
		{
			PhotonNetwork.LoadLevelIfSynced();
		}

		public void OnCreatedRoom()
		{
			PhotonNetwork.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
		}

		public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			PhotonNetwork.LoadLevelIfSynced();
		}

		public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
		{
		}

		public void OnMasterClientSwitched(Player newMasterClient)
		{
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
		}

		public void OnLeftRoom()
		{
		}

		public void OnPlayerEnteredRoom(Player newPlayer)
		{
		}

		public void OnPlayerLeftRoom(Player otherPlayer)
		{
		}
	}
}
