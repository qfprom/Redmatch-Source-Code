using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public class OnJoinedInstantiate : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks
	{
		public Transform SpawnPosition;

		public float PositionOffset = 2f;

		public GameObject[] PrefabsToInstantiate;

		public virtual void OnEnable()
		{
			PhotonNetwork.AddCallbackTarget(this);
		}

		public virtual void OnDisable()
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		public void OnJoinedRoom()
		{
			if (PrefabsToInstantiate == null)
			{
				return;
			}
			GameObject[] prefabsToInstantiate = PrefabsToInstantiate;
			foreach (GameObject gameObject in prefabsToInstantiate)
			{
				Debug.Log("Instantiating: " + gameObject.name);
				Vector3 vector = Vector3.up;
				if (SpawnPosition != null)
				{
					vector = SpawnPosition.position;
				}
				Vector3 insideUnitSphere = Random.insideUnitSphere;
				insideUnitSphere.y = 0f;
				insideUnitSphere = insideUnitSphere.normalized;
				Vector3 position = vector + PositionOffset * insideUnitSphere;
				PhotonNetwork.Instantiate(gameObject.name, position, Quaternion.identity, 0);
			}
		}

		public void OnConnected()
		{
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
		}

		public void OnConnectedToMaster()
		{
		}

		public void OnDisconnected(DisconnectCause cause)
		{
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
		}

		public void OnRoomListUpdate(List<RoomInfo> roomList)
		{
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
		}

		public void OnJoinedLobby()
		{
		}

		public void OnLeftLobby()
		{
		}

		public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
		{
		}

		public void OnCreatedRoom()
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
	}
}
