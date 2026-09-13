using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public static class TeamExtensions
	{
		public static PunTeams.Team GetTeam(this Player player)
		{
			object value;
			if (player.CustomProperties.TryGetValue("team", out value))
			{
				return (PunTeams.Team)value;
			}
			return PunTeams.Team.none;
		}

		public static void SetTeam(this Player player, PunTeams.Team team)
		{
			if (!PhotonNetwork.IsConnectedAndReady)
			{
				Debug.LogWarning(string.Concat("JoinTeam was called in state: ", PhotonNetwork.NetworkClientState, ". Not IsConnectedAndReady."));
				return;
			}
			PunTeams.Team team2 = player.GetTeam();
			if (team2 != team)
			{
				player.SetCustomProperties(new Hashtable { 
				{
					"team",
					(byte)team
				} });
			}
		}
	}
}
