using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public class PlayerNumbering : MonoBehaviourPunCallbacks
	{
		public delegate void PlayerNumberingChanged();

		public static PlayerNumbering instance;

		public static Player[] SortedPlayers;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static PlayerNumberingChanged OnPlayerNumberingChanged__BackingField;

		public const string RoomPlayerIndexedProp = "pNr";

		public bool dontDestroyOnLoad;

		public static event PlayerNumberingChanged OnPlayerNumberingChanged
		{
			add
			{
				PlayerNumberingChanged playerNumberingChanged = OnPlayerNumberingChanged__BackingField;
				PlayerNumberingChanged playerNumberingChanged2;
				do
				{
					playerNumberingChanged2 = playerNumberingChanged;
					playerNumberingChanged = Interlocked.CompareExchange(ref OnPlayerNumberingChanged__BackingField, (PlayerNumberingChanged)Delegate.Combine(playerNumberingChanged2, value), playerNumberingChanged);
				}
				while ((object)playerNumberingChanged != playerNumberingChanged2);
			}
			remove
			{
				PlayerNumberingChanged playerNumberingChanged = OnPlayerNumberingChanged__BackingField;
				PlayerNumberingChanged playerNumberingChanged2;
				do
				{
					playerNumberingChanged2 = playerNumberingChanged;
					playerNumberingChanged = Interlocked.CompareExchange(ref OnPlayerNumberingChanged__BackingField, (PlayerNumberingChanged)Delegate.Remove(playerNumberingChanged2, value), playerNumberingChanged);
				}
				while ((object)playerNumberingChanged != playerNumberingChanged2);
			}
		}

		public void Awake()
		{
			if (instance != null && instance != this && instance.gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(instance.gameObject);
			}
			instance = this;
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			RefreshData();
		}

		public override void OnJoinedRoom()
		{
			RefreshData();
		}

		public override void OnLeftRoom()
		{
			PhotonNetwork.LocalPlayer.CustomProperties.Remove("pNr");
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			RefreshData();
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			RefreshData();
		}

		public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
		{
			if (changedProps != null && changedProps.ContainsKey("pNr"))
			{
				RefreshData();
			}
		}

		public void RefreshData()
		{
			if (PhotonNetwork.CurrentRoom == null)
			{
				return;
			}
			if (PhotonNetwork.LocalPlayer.GetPlayerNumber() >= 0)
			{
				SortedPlayers = PhotonNetwork.CurrentRoom.Players.Values.OrderBy(PlayerNumberingExtensions.GetPlayerNumber).ToArray();
				if (OnPlayerNumberingChanged__BackingField != null)
				{
					OnPlayerNumberingChanged__BackingField();
				}
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			Player[] array = PhotonNetwork.PlayerList.OrderBy((Player p) => p.ActorNumber).ToArray();
			string text = "all players: ";
			Player[] array2 = array;
			foreach (Player player in array2)
			{
				string text2 = text;
				text = text2 + player.ActorNumber + "=pNr:" + player.GetPlayerNumber() + ", ";
				int playerNumber = player.GetPlayerNumber();
				if (player.IsLocal)
				{
					Debug.Log("PhotonNetwork.CurrentRoom.PlayerCount = " + PhotonNetwork.CurrentRoom.PlayerCount);
					for (int num2 = 0; num2 < PhotonNetwork.CurrentRoom.PlayerCount; num2++)
					{
						if (!hashSet.Contains(num2))
						{
							player.SetPlayerNumber(num2);
							break;
						}
					}
					break;
				}
				if (playerNumber < 0)
				{
					break;
				}
				hashSet.Add(playerNumber);
			}
			SortedPlayers = PhotonNetwork.CurrentRoom.Players.Values.OrderBy(PlayerNumberingExtensions.GetPlayerNumber).ToArray();
			if (OnPlayerNumberingChanged__BackingField != null)
			{
				OnPlayerNumberingChanged__BackingField();
			}
		}
	}
}
