using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public class ConnectAndJoinRandom : MonoBehaviourPunCallbacks
	{
		public bool AutoConnect = true;

		public byte Version = 1;

		public void Start()
		{
			if (AutoConnect)
			{
				ConnectNow();
			}
		}

		public void ConnectNow()
		{
			Debug.Log("ConnectAndJoinRandom.ConnectNow() will now call: PhotonNetwork.ConnectUsingSettings().");
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.GameVersion = Version + "." + SceneManagerHelper.ActiveSceneBuildIndex;
		}

		public override void OnConnectedToMaster()
		{
			Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
			PhotonNetwork.JoinRandomRoom();
		}

		public override void OnJoinedLobby()
		{
			Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
			PhotonNetwork.JoinRandomRoom();
		}

		public override void OnJoinRandomFailed(short returnCode, string message)
		{
			Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
			PhotonNetwork.CreateRoom(null, new RoomOptions
			{
				MaxPlayers = 4
			});
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			Debug.Log(string.Concat("OnDisconnected(", cause, ")"));
		}

		public override void OnJoinedRoom()
		{
			Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running.");
		}
	}
}
