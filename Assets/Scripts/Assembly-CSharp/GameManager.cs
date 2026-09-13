using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
	public static GameManager gameManager;

	[HideInInspector]
	public GameMode gameMode;

	public float timer;

	private int timerDuration;

	private int serverTimerStartTime;

	public PhotonView PV;

	[HideInInspector]
	public bool gameStarted;

	[HideInInspector]
	public bool gameEnded;

	private bool timing;

	private Color[] colors = new Color[4]
	{
		Color.green,
		Color.grey,
		Color.red,
		Color.cyan
	};

	public Dictionary<AmmoType, string> ammoFloorItemNames = new Dictionary<AmmoType, string>
	{
		{
			AmmoType.Light,
			"LightAmmo"
		},
		{
			AmmoType.Medium,
			"MediumAmmo"
		},
		{
			AmmoType.Heavy,
			"HeavyAmmo"
		},
		{
			AmmoType.Shotgun,
			"ShotgunAmmo"
		}
	};

	private void Awake()
	{
		gameManager = this;
		gameMode = (GameMode)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
		PV = GetComponent<PhotonView>();
		if (gameMode == GameMode.TTT)
		{
			Hashtable hashtable = new Hashtable();
			if (PlayerPrefs.HasKey("tttkarma"))
			{
				hashtable.Add("tttkarma", PlayerPrefs.GetInt("tttkarma"));
			}
			else
			{
				hashtable.Add("tttkarma", 100);
				PlayerPrefs.SetInt("tttkarma", 100);
			}
			int num = 0;
			int num2 = 0;
			hashtable.Add("kills", num);
			hashtable.Add("deaths", num2);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
		}
		else
		{
			Hashtable hashtable2 = new Hashtable();
			int num3 = 0;
			int num4 = 0;
			if (gameMode == GameMode.Infection)
			{
				hashtable2.Add("infected", false);
			}
			hashtable2.Add("kills", num3);
			hashtable2.Add("deaths", num4);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable2);
		}
	}

	public void LocalPlayerWinsTheGame(bool rpc = true)
	{
		if (gameMode == GameMode.GunGame)
		{
			UserDataManager.Instance.IncrementProperty("GUNGAME_WINS");
		}
		else if (gameMode == GameMode.BattleRoyale)
		{
			UserDataManager.Instance.IncrementProperty("BATTLEROYALE_WINS");
		}
		if (rpc)
		{
			PV.RPC("RPC_WinGame", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, "K/D: " + ((float)(int)PhotonNetwork.LocalPlayer.CustomProperties["kills"] / Mathf.Max((int)PhotonNetwork.LocalPlayer.CustomProperties["deaths"], 1f)).ToString("0.0"), 1);
		}
	}

	public GameModeInfo GetGameModeInfo()
	{
		GameModeInfo[] gameModeInfo = MultiplayerSettings.multiplayerSettings.gameModeInfo;
		for (int i = 0; i < gameModeInfo.Length; i++)
		{
			GameModeInfo result = gameModeInfo[i];
			if (result.gameMode == gameMode)
			{
				return result;
			}
		}
		Debug.LogError("GameMode missing from gameModeInfo array");
		return MultiplayerSettings.multiplayerSettings.gameModeInfo[0];
	}

	public void GameStart()
	{
		GameSetup.gameSetup.defaultCamObj.SetActive(false);
		if (GetGameModeInfo().floorItems)
		{
			SpawnGuns();
		}
		if (gameMode == GameMode.TTT)
		{
			GameSetup.gameSetup.stageText.text = "Preperation Phase";
			GameSetup.gameSetup.killAmountText.gameObject.SetActive(false);
			if (PhotonNetwork.IsMasterClient)
			{
				PV.RPC("RPC_StartTimer", RpcTarget.AllBuffered, PhotonNetwork.ServerTimestamp, 30000);
			}
		}
		else if (gameMode == GameMode.Infection)
		{
			GameSetup.gameSetup.stageText.text = "Preperation Phase";
			PV.RPC("RPC_StartTimer", RpcTarget.AllBuffered, PhotonNetwork.ServerTimestamp, 18000);
		}
	}

	private void Update()
	{
		if (!timing)
		{
			return;
		}
		timer = PhotonNetwork.ServerTimestamp - serverTimerStartTime;
		if (timer <= (float)timerDuration)
		{
			if (!GameSetup.gameSetup.timerText.gameObject.activeSelf)
			{
				GameSetup.gameSetup.timerText.gameObject.SetActive(true);
			}
			int num = Mathf.FloorToInt(((float)timerDuration - timer) / 1000f / 60f);
			int num2 = Mathf.FloorToInt(((float)timerDuration - timer) / 1000f - (float)(num * 60));
			GameSetup.gameSetup.timerText.text = string.Format("{0:0}:{1:00}", num, num2);
		}
		else if (GameSetup.gameSetup.timerText.gameObject.activeSelf)
		{
			GameSetup.gameSetup.timerText.gameObject.SetActive(false);
		}
		if (gameMode == GameMode.TTT && timer >= (float)timerDuration)
		{
			if (gameStarted)
			{
				if (gameEnded)
				{
					return;
				}
				string text = "Survivors: ";
				foreach (PhotonPlayer alivePlayer in GetAlivePlayers())
				{
					text = text + alivePlayer.PV.Owner.NickName + " ";
				}
				if (PhotonNetwork.IsMasterClient)
				{
					PV.RPC("RPC_WinGame", RpcTarget.All, "Innocents Win", text, 0);
				}
			}
			else
			{
				gameStarted = true;
				GameSetup.gameSetup.stageText.text = "Game Started";
				GameSetup.gameSetup.timerText.text = string.Empty;
				if (PhotonNetwork.IsMasterClient)
				{
					Hashtable hashtable = new Hashtable();
					hashtable.Add("gameStarted", true);
					PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
					AssignTTTTeams();
					PV.RPC("RPC_StartTimer", RpcTarget.AllBuffered, PhotonNetwork.ServerTimestamp, 300000);
				}
			}
		}
		else
		{
			if (gameMode != GameMode.Infection || !(timer >= (float)timerDuration))
			{
				return;
			}
			if (gameStarted)
			{
				if (gameEnded)
				{
					return;
				}
				string text2 = "The uninfected win! Survivors: ";
				foreach (PhotonPlayer alivePlayer2 in GetAlivePlayers())
				{
					if (!alivePlayer2.infected)
					{
						text2 = text2 + alivePlayer2.PV.Owner.NickName + " ";
					}
				}
				if (PhotonNetwork.IsMasterClient)
				{
					PV.RPC("RPC_WinGame", RpcTarget.All, "Cure Discovered", text2, 3);
				}
			}
			else
			{
				gameStarted = true;
				GameSetup.gameSetup.stageText.text = "Game Started";
				GameSetup.gameSetup.timerText.text = string.Empty;
				if (PhotonNetwork.IsMasterClient)
				{
					Hashtable hashtable2 = new Hashtable();
					hashtable2.Add("gameStarted", true);
					PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable2);
					AssignInfectionTeams();
					PV.RPC("RPC_StartTimer", RpcTarget.AllBuffered, PhotonNetwork.ServerTimestamp, 200000);
				}
			}
		}
	}

	[PunRPC]
	private void RPC_StartTimer(int serverTime, int duration)
	{
		timing = true;
		serverTimerStartTime = serverTime;
		timerDuration = duration;
	}

	public void OnPlayerDeath()
	{
		PV.RPC("RPC_OnPlayerDeath", RpcTarget.MasterClient);
	}

	[PunRPC]
	private void RPC_OnPlayerDeath()
	{
		if (gameMode == GameMode.TTT)
		{
			if (!gameStarted || gameEnded)
			{
				return;
			}
			List<PhotonPlayer> alivePlayers = GetAlivePlayers();
			int num = 0;
			int num2 = 0;
			foreach (PhotonPlayer item in alivePlayers)
			{
				if (item.tttteam == TTTTeam.Traitor)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
			if (num == 0)
			{
				string text = "Survivors: ";
				foreach (PhotonPlayer item2 in alivePlayers)
				{
					text = text + item2.PV.Owner.NickName + " ";
				}
				PV.RPC("RPC_WinGame", RpcTarget.All, "Traitors Win", text, 2);
			}
			else
			{
				if (num2 != 0)
				{
					return;
				}
				string text2 = "Survivors: ";
				foreach (PhotonPlayer item3 in alivePlayers)
				{
					text2 = text2 + item3.PV.Owner.NickName + " ";
				}
				PV.RPC("RPC_WinGame", RpcTarget.All, "Innocents Win", text2, 0);
			}
		}
		else if (gameMode == GameMode.Infection)
		{
			int num3 = 0;
			int num4 = 0;
			foreach (PhotonPlayer alivePlayer in GetAlivePlayers())
			{
				if (alivePlayer.alive)
				{
					if ((bool)alivePlayer.PV.Owner.CustomProperties["infected"])
					{
						num3++;
					}
					else
					{
						num4++;
					}
				}
			}
			if (num4 <= 0)
			{
				PV.RPC("RPC_WinGame", RpcTarget.All, "Infected Win", string.Empty, 0);
			}
		}
		else if (gameMode == GameMode.BattleRoyale)
		{
			List<PhotonPlayer> alivePlayers2 = GetAlivePlayers();
			if (alivePlayers2.Count == 1)
			{
				PV.RPC("RPC_PlayerWin", RpcTarget.All, alivePlayers2[0].PV.Owner.NickName + " Wins", string.Empty, 0, alivePlayers2[0].PV.Owner);
			}
			else if (alivePlayers2.Count == 0)
			{
				PV.RPC("RPC_WinGame", RpcTarget.All, "Tie", string.Empty, 0);
			}
		}
	}

	[PunRPC]
	private void RPC_WinGame(string winText, string descriptionText, int winScreenColorindex)
	{
		if (!gameEnded)
		{
			gameEnded = true;
			GameSetup.gameSetup.winScreen.SetActive(true);
			GameSetup.gameSetup.winText.text = winText;
			GameSetup.gameSetup.winTitleBackground.color = colors[winScreenColorindex];
			GameSetup.gameSetup.winDescriptionText.text = descriptionText;
			GameSetup.gameSetup.stageText.text = "Returning to Lobby";
			RPC_StartTimer(PhotonNetwork.ServerTimestamp, 10000);
			Invoke("GameEnd", 10f);
		}
	}

	[PunRPC]
	private void RPC_PlayerWin(string winText, string descriptionText, int winScreenColorindex, Player player)
	{
		if (!gameEnded)
		{
			gameEnded = true;
			GameSetup.gameSetup.winScreen.SetActive(true);
			GameSetup.gameSetup.winText.text = winText;
			GameSetup.gameSetup.winTitleBackground.color = colors[winScreenColorindex];
			GameSetup.gameSetup.winDescriptionText.text = descriptionText;
			GameSetup.gameSetup.stageText.text = "Returning to Lobby";
			RPC_StartTimer(PhotonNetwork.ServerTimestamp, 10000);
			Invoke("GameEnd", 10f);
			if (player == PhotonNetwork.LocalPlayer)
			{
				LocalPlayerWinsTheGame(false);
			}
		}
	}

	private void GameEnd()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		if (PhotonNetwork.IsMasterClient)
		{
			Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Player targetPlayer in playerList)
			{
				PhotonNetwork.RemoveRPCs(targetPlayer);
			}
		}
		GameSetup.gameSetup.SoftDisconnect();
	}

	private List<PhotonPlayer> GetAlivePlayers()
	{
		PhotonPlayer[] array = Object.FindObjectsOfType<PhotonPlayer>();
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		PhotonPlayer[] array2 = array;
		foreach (PhotonPlayer photonPlayer in array2)
		{
			if (photonPlayer.alive)
			{
				list.Add(photonPlayer);
			}
		}
		return list;
	}

	[PunRPC]
	public void RPC_TTTGlobalChat(GameChatChannel channel, string text, TTTTeam tttteam)
	{
		if (channel == GameChatChannel.Team)
		{
			switch (tttteam)
			{
			case TTTTeam.Traitor:
				if (GameSetup.gameSetup.player.tttteam == TTTTeam.Traitor)
				{
					GameSetup.gameSetup.SendGameInfo("<color=#ff7272>" + text + "</color>");
				}
				break;
			case TTTTeam.Detective:
				if (GameSetup.gameSetup.player.tttteam == TTTTeam.Detective)
				{
					GameSetup.gameSetup.SendGameInfo("<color=#728aff>" + text + "</color>");
				}
				break;
			}
		}
		else
		{
			GameSetup.gameSetup.SendGameInfo(text);
		}
	}

	[PunRPC]
	public void RPC_GlobalChat(GameChatChannel channel, string text)
	{
		GameSetup.gameSetup.SendGameInfo(text);
	}

	public void KickPlayer(Player kickPlayer)
	{
		PV.RPC("RPC_KickPlayer", RpcTarget.All, kickPlayer);
	}

	[PunRPC]
	private void RPC_KickPlayer(Player kickPlayer)
	{
		if (kickPlayer == PhotonNetwork.LocalPlayer)
		{
			GameSetup.gameSetup.DisconnectPlayer();
		}
	}

	private void AssignInfectionTeams()
	{
		List<PhotonPlayer> alivePlayers = GetAlivePlayers();
		string text = string.Empty;
		if (alivePlayers.Count >= 2)
		{
			int num = (int)Mathf.Max(Mathf.Floor((float)alivePlayers.Count * 0.2f), 1f);
			List<bool> list = new List<bool>();
			for (int i = 0; i < alivePlayers.Count - num; i++)
			{
				list.Add(false);
			}
			for (int j = 0; j < num; j++)
			{
				list.Add(true);
			}
			foreach (PhotonPlayer item in alivePlayers)
			{
				int index = Random.Range(0, list.Count);
				if (list[index])
				{
					text = ((!string.IsNullOrEmpty(text)) ? (text + ", " + item.PV.Owner.NickName) : item.PV.Owner.NickName);
					item.PV.RPC("RPC_AssignInfectedTeam", RpcTarget.AllBuffered);
				}
				list.RemoveAt(index);
			}
			PV.RPC("RPC_GlobalChat", RpcTarget.All, GameChatChannel.All, string.Format("<color=green><b>{0}</b> {1} infected.</color>", text, (num != 1) ? "are" : "is"));
		}
		else
		{
			GameSetup.gameSetup.SendGameInfo(((alivePlayers.Count != 1) ? (alivePlayers.Count + "players") : "1 player") + " is not enough for Infection. You need at least 2 to play. 5+ players is recommended for a fun experience. The more you have, the better it is.");
		}
	}

	private void AssignTTTTeams()
	{
		List<PhotonPlayer> alivePlayers = GetAlivePlayers();
		if (alivePlayers.Count >= 2)
		{
			int num = (int)Mathf.Max(Mathf.Floor((float)alivePlayers.Count * 0.3f), 1f);
			int num2 = Mathf.FloorToInt((float)alivePlayers.Count * 0.18f);
			int num3 = alivePlayers.Count - (num + num2);
			List<TTTTeam> list = new List<TTTTeam>();
			for (int i = 0; i < num; i++)
			{
				list.Add(TTTTeam.Traitor);
			}
			for (int j = 0; j < num2; j++)
			{
				list.Add(TTTTeam.Detective);
			}
			for (int k = 0; k < num3; k++)
			{
				list.Add(TTTTeam.Innocent);
			}
			foreach (TTTTeam item in list)
			{
			}
			foreach (PhotonPlayer item2 in alivePlayers)
			{
				int index = Random.Range(0, list.Count);
				item2.PV.RPC("RPC_AssignTTTTeam", RpcTarget.AllBuffered, list[index]);
				list.RemoveAt(index);
			}
			PV.RPC("RPC_GlobalChat", RpcTarget.All, GameChatChannel.All, (num != 1) ? ("<color=red>There are <b>" + num + "</b> traitors.</color>") : "<color=red>There is <b>1</b> traitor.</color>");
		}
		else
		{
			GameSetup.gameSetup.SendGameInfo(((alivePlayers.Count != 1) ? (alivePlayers.Count + "players") : "1 player") + " is not enough for TTT. You need at least 2 to play. 5+ players is recommended for a fun experience. The more you have, the better it is.");
		}
	}

	private void SpawnGuns()
	{
		foreach (Transform gunSpawnPoint in GameSetup.gameSetup.gunSpawnPoints)
		{
			int num = Random.Range(0, GameSetup.gameSetup.spawnItems.Length);
			PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs/FloorItems", GameSetup.gameSetup.spawnItems[num].floorItemName), gunSpawnPoint.position, gunSpawnPoint.rotation, 0);
			for (int i = 0; i < Random.Range(1, 2); i++)
			{
				PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs/FloorItems", ammoFloorItemNames[GameSetup.gameSetup.spawnItems[num].ammoType]), gunSpawnPoint.position + Vector3.up * i * 0.35f, gunSpawnPoint.rotation, 0);
			}
		}
	}
}
