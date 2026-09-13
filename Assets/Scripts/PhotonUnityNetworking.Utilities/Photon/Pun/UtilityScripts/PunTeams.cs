using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.UtilityScripts
{
	public class PunTeams : MonoBehaviourPunCallbacks
	{
		public enum Team : byte
		{
			none = 0,
			red = 1,
			blue = 2
		}

		public static Dictionary<Team, List<Player>> PlayersPerTeam;

		public const string TeamPlayerProp = "team";

		public void Start()
		{
			PlayersPerTeam = new Dictionary<Team, List<Player>>();
			Array values = Enum.GetValues(typeof(Team));
			foreach (object item in values)
			{
				PlayersPerTeam[(Team)item] = new List<Player>();
			}
		}

		public override void OnDisable()
		{
			PlayersPerTeam = new Dictionary<Team, List<Player>>();
		}

		public override void OnJoinedRoom()
		{
			UpdateTeams();
		}

		public override void OnLeftRoom()
		{
			Start();
		}

		public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
		{
			UpdateTeams();
		}

		public override void OnPlayerLeftRoom(Player otherPlayer)
		{
			UpdateTeams();
		}

		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			UpdateTeams();
		}

		public void UpdateTeams()
		{
			Array values = Enum.GetValues(typeof(Team));
			foreach (object item in values)
			{
				PlayersPerTeam[(Team)item].Clear();
			}
			for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
			{
				Player player = PhotonNetwork.PlayerList[i];
				Team team = player.GetTeam();
				PlayersPerTeam[team].Add(player);
			}
		}
	}
}
