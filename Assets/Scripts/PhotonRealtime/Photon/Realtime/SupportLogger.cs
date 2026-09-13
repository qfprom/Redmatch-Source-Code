using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime
{
	[AddComponentMenu("")]
	public class SupportLogger : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks
	{
		public bool LogTrafficStats;

		private bool loggedStillOfflineMessage;

		private LoadBalancingClient client;

		private Stopwatch startStopwatch;

		public LoadBalancingClient Client
		{
			get
			{
				return client;
			}
			set
			{
				if (client != value)
				{
					if (client != null)
					{
						client.RemoveCallbackTarget(this);
					}
					client = value;
					client.AddCallbackTarget(this);
				}
			}
		}

		protected void Start()
		{
			if (startStopwatch == null)
			{
				startStopwatch = new Stopwatch();
				startStopwatch.Start();
			}
			if (LogTrafficStats)
			{
				InvokeRepeating("LogStats", 10f, 10f);
			}
		}

		protected void OnApplicationPause(bool pause)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnApplicationPause: " + pause + " connected: " + client.IsConnected);
		}

		protected void OnApplicationQuit()
		{
			CancelInvoke();
		}

		private string GetFormattedTimestamp()
		{
			if (startStopwatch == null)
			{
				startStopwatch = new Stopwatch();
				startStopwatch.Start();
			}
			return string.Format("[{0}.{1}]", startStopwatch.Elapsed.Seconds, startStopwatch.Elapsed.Milliseconds);
		}

		public void LogStats()
		{
			if (client.State == ClientState.PeerCreated)
			{
				if (!loggedStillOfflineMessage)
				{
					loggedStillOfflineMessage = true;
					UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger Photon Client is not connected yet: this logger won't be able to capture the state.");
				}
			}
			else if (LogTrafficStats)
			{
				UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger " + client.LoadBalancingPeer.VitalStatsToString(false));
			}
		}

		private void LogBasics()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0} SupportLogger Info: ", GetFormattedTimestamp());
			stringBuilder.AppendFormat("AppID: \"{0}\" AppVersion: \"{1}\" PeerID: {2} ", (!string.IsNullOrEmpty(client.AppId) && client.AppId.Length >= 8) ? (client.AppId.Substring(0, 8) + "***") : client.AppId, client.AppVersion, client.LoadBalancingPeer.PeerID);
			stringBuilder.AppendFormat("NameServer: {0} Server: {1} IP: {2} Region: {3}", client.NameServerHost, client.CurrentServerAddress, client.LoadBalancingPeer.ServerIpAddress, client.CloudRegion);
			UnityEngine.Debug.Log(stringBuilder.ToString());
		}

		public void OnConnected()
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnConnected().");
			LogBasics();
			if (LogTrafficStats)
			{
				client.EnableLobbyStatistics = true;
			}
		}

		public void OnConnectedToMaster()
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnConnectedToMaster().");
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnFriendListUpdate(friendList).");
		}

		public void OnJoinedLobby()
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnJoinedLobby(", client.CurrentLobby, ")."));
		}

		public void OnLeftLobby()
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLeftLobby().");
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCreateRoomFailed(" + returnCode + "," + message + ").");
		}

		public void OnJoinedRoom()
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnJoinedRoom(", client.CurrentRoom, "). ", client.CurrentLobby, " GameServer:", client.GameServerAddress));
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinRandomFailed(" + returnCode + "," + message + ").");
		}

		public void OnCreatedRoom()
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnCreatedRoom(", client.CurrentRoom, "). ", client.CurrentLobby, " GameServer:", client.GameServerAddress));
		}

		public void OnLeftRoom()
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLeftRoom().");
		}

		public void OnDisconnected(DisconnectCause cause)
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnDisconnected(", cause, ")."));
			LogBasics();
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRegionListReceived(regionHandler).");
			LogBasics();
		}

		public void OnRoomListUpdate(List<RoomInfo> roomList)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRoomListUpdate(roomList). roomList.Count: " + roomList.Count);
		}

		public void OnPlayerEnteredRoom(Player newPlayer)
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnPlayerEnteredRoom(", newPlayer, ")."));
		}

		public void OnPlayerLeftRoom(Player otherPlayer)
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnPlayerLeftRoom(", otherPlayer, ")."));
		}

		public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRoomPropertiesUpdate(propertiesThatChanged).");
		}

		public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerPropertiesUpdate(targetPlayer,changedProps).");
		}

		public void OnMasterClientSwitched(Player newMasterClient)
		{
			UnityEngine.Debug.Log(string.Concat(GetFormattedTimestamp(), " SupportLogger OnMasterClientSwitched(", newMasterClient, ")."));
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationResponse(" + data.ToStringFull() + ").");
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationFailed(" + debugMessage + ").");
		}

		public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
		{
			UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLobbyStatisticsUpdate(lobbyStatistics).");
		}
	}
}
