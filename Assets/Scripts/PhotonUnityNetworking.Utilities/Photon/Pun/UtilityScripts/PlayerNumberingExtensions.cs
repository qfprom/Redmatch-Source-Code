using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public static class PlayerNumberingExtensions
	{
		public static int GetPlayerNumber(this Player player)
		{
			if (player == null)
			{
				return -1;
			}
			if (PhotonNetwork.OfflineMode)
			{
				return 0;
			}
			if (!PhotonNetwork.IsConnectedAndReady)
			{
				return -1;
			}
			object value;
			if (player.CustomProperties.TryGetValue("pNr", out value))
			{
				return (byte)value;
			}
			return -1;
		}

		public static void SetPlayerNumber(this Player player, int playerNumber)
		{
			if (player == null || PhotonNetwork.OfflineMode)
			{
				return;
			}
			if (playerNumber < 0)
			{
				Debug.LogWarning("Setting invalid playerNumber: " + playerNumber + " for: " + player.ToStringFull());
			}
			if (!PhotonNetwork.IsConnectedAndReady)
			{
				Debug.LogWarning(string.Concat("SetPlayerNumber was called in state: ", PhotonNetwork.NetworkClientState, ". Not IsConnectedAndReady."));
				return;
			}
			int playerNumber2 = player.GetPlayerNumber();
			if (playerNumber2 != playerNumber)
			{
				Debug.Log("PlayerNumbering: Set number " + playerNumber);
				player.SetCustomProperties(new Hashtable { 
				{
					"pNr",
					(byte)playerNumber
				} });
			}
		}
	}
}
