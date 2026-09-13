using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Voice.Unity.UtilityScripts
{
	[RequireComponent(typeof(VoiceConnection))]
	public class ConnectAndJoin : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
	{
		private VoiceConnection voiceConnection;

		public bool RandomRoom = true;

		[SerializeField]
		private bool autoConnect = true;

		[SerializeField]
		private bool autoTransmit = true;

		[SerializeField]
		private byte version = 1;

		public string RoomName;

		private RoomOptions roomOptions = new RoomOptions();

		private TypedLobby typedLobby = TypedLobby.Default;

		public bool IsConnected
		{
			get
			{
				return voiceConnection.Client.IsConnected;
			}
		}

		private void Awake()
		{
			voiceConnection = GetComponent<VoiceConnection>();
		}

		private void OnEnable()
		{
			voiceConnection.Client.AddCallbackTarget(this);
			if (autoConnect)
			{
				ConnectNow();
			}
		}

		private void OnDisable()
		{
			voiceConnection.Client.RemoveCallbackTarget(this);
		}

		public void ConnectNow()
		{
			Debug.Log("ConnectAndJoin.ConnectNow() will now call: VoiceConnection.ConnectUsingSettings().");
			voiceConnection.ConnectUsingSettings();
			voiceConnection.Client.AppVersion = string.Format("{0}.{1}", version, SceneManager.GetActiveScene().buildIndex);
		}

		public void OnCreatedRoom()
		{
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
		}

		public void OnJoinedRoom()
		{
			if (autoTransmit)
			{
				if (voiceConnection.PrimaryRecorder == null)
				{
					voiceConnection.PrimaryRecorder = base.gameObject.AddComponent<Recorder>();
				}
				voiceConnection.PrimaryRecorder.TransmitEnabled = autoTransmit;
				voiceConnection.PrimaryRecorder.Init(voiceConnection.VoiceClient);
			}
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			if (returnCode == 32760)
			{
				voiceConnection.Client.OpCreateRoom(new EnterRoomParams
				{
					RoomName = RoomName,
					RoomOptions = roomOptions,
					Lobby = typedLobby
				});
			}
			else
			{
				Debug.LogErrorFormat("OnJoinRandomFailed errorCode={0} errorMessage={1}", returnCode, message);
			}
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			Debug.LogErrorFormat("OnJoinRoomFailed roomName={0} errorCode={1} errorMessage={2}", RoomName, returnCode, message);
		}

		public void OnLeftRoom()
		{
		}

		public void OnConnected()
		{
		}

		public void OnConnectedToMaster()
		{
			if (RandomRoom)
			{
				voiceConnection.Client.OpJoinRandomRoom(new OpJoinRandomRoomParams());
				return;
			}
			voiceConnection.Client.OpJoinOrCreateRoom(new EnterRoomParams
			{
				RoomName = RoomName,
				RoomOptions = roomOptions,
				Lobby = typedLobby
			});
		}

		public void OnDisconnected(DisconnectCause cause)
		{
			if (cause != DisconnectCause.None && cause != DisconnectCause.DisconnectByClientLogic)
			{
				Debug.LogErrorFormat("OnDisconnected cause={0}", cause);
			}
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
		}
	}
}
