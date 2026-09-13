using System.Collections;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PhotonPlayer : MonoBehaviour
{
	[HideInInspector]
	public PhotonView PV;

	[HideInInspector]
	public GameObject avatar;

	[HideInInspector]
	public PlayerController avatarPlayerController;

	[SerializeField]
	private GameObject spectatorAvatarPrefab;

	[HideInInspector]
	public TTTTeam tttteam;

	[HideInInspector]
	public bool alive;

	[HideInInspector]
	public int currentGunGameItem;

	[HideInInspector]
	public bool infected;

	public int tttkarma;

	private bool teamkilledThisGame;

	private int goodKills;

	private int killStreak;

	[HideInInspector]
	public bool inGame = true;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		if (PV.IsMine)
		{
			GameManager.gameManager.GameStart();
			LoadingScreenManager.Instance.ResolveLoadingScreen();
			GameSetup.gameSetup.player = this;
			tttkarma = PlayerPrefs.GetInt("tttkarma");
		}
	}

	private void Start()
	{
		if (!PV.IsMine)
		{
			return;
		}
		switch (GameManager.gameManager.gameMode)
		{
		case GameMode.GunGame:
			UserDataManager.Instance.IncrementProperty("GUNGAME_GAMES_PLAYED");
			break;
		case GameMode.TTT:
			UserDataManager.Instance.IncrementProperty("TTT_GAMES_PLAYED");
			break;
		case GameMode.Skirmish:
			UserDataManager.Instance.IncrementProperty("SKIRMISH_GAMES_PLAYED");
			break;
		case GameMode.Infection:
			UserDataManager.Instance.IncrementProperty("INFECTION_GAMES_PLAYED");
			break;
		case GameMode.BattleRoyale:
			UserDataManager.Instance.IncrementProperty("BATTLEROYALE_GAMES_PLAYED");
			break;
		}
		if (GameManager.gameManager.GetGameModeInfo().canJoinAfterStart)
		{
			if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["gameStarted"])
			{
				CreateSpectatorAvatar(Vector3.zero + Vector3.up * 10f);
				ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
				hashtable.Add("inGame", false);
				inGame = false;
				PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
			}
			else
			{
				ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
				hashtable2.Add("inGame", true);
				PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable2);
				CreateAvatar();
			}
		}
		else
		{
			CreateAvatar();
		}
	}

	[PunRPC]
	public void RPC_AssignInfectedTeam()
	{
		infected = true;
		if (PV.IsMine)
		{
			avatarPlayerController.PV.RPC("AssignInfected", RpcTarget.All);
			GameSetup.gameSetup.teamText.text = "<color=green>Infected</color>";
			GameSetup.gameSetup.SendGameInfo("<color=green>You are infected. Infect everyone!</color>");
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			hashtable.Add("infected", true);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
		}
	}

	[PunRPC]
	public void RPC_AssignTTTTeam(TTTTeam team)
	{
		tttteam = team;
		if (PV.IsMine)
		{
			avatarPlayerController.PV.RPC("RPC_AssignTTTTeam", RpcTarget.AllBuffered, tttteam);
			string info;
			if (tttteam == TTTTeam.Traitor)
			{
				info = "<color=red>You have been assigned the role of a <b>Traitor</b>. Kill all the innocents.</color>";
				GameSetup.gameSetup.teamText.text = "<color=red>Traitor</color>";
			}
			else if (tttteam == TTTTeam.Detective)
			{
				info = "<color=blue>You have been assigned the role of a <b>Detective</b>. Save all the innocents.</color>";
				GameSetup.gameSetup.teamText.text = "<color=blue>Detective</color>";
			}
			else
			{
				info = "<color=lime>You have been assigned the role of an <b>Innocent</b>. Survive!</color>";
				GameSetup.gameSetup.teamText.text = "<color=lime>Innocent</color>";
			}
			ItemShop.shop.UpdateUI();
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			hashtable.Add("tttteam", tttteam);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
			GameSetup.gameSetup.SendGameInfo(info);
		}
	}

	public void SendGlobalGameInfo(string info)
	{
		PV.RPC("RPC_SendGlobalGameInfo", RpcTarget.All, info);
	}

	[PunRPC]
	private void RPC_SendGlobalGameInfo(string info)
	{
		GameSetup.gameSetup.SendGameInfo(info);
	}

	[PunRPC]
	private void RPC_GetKill(int killedViewID)
	{
		if (!PV.IsMine)
		{
			return;
		}
		killStreak++;
		if (GameManager.gameManager.gameMode == GameMode.Skirmish)
		{
			UserDataManager.Instance.IncrementProperty("SKIRMISH_KILLS");
			if (killStreak > UserDataManager.Instance.GetIntProperty("SKIRMISH_HIGHEST_KILLSTREAK"))
			{
				UserDataManager.Instance.SetProperty("SKIRMISH_HIGHEST_KILLSTREAK", killStreak);
			}
		}
		else if (GameManager.gameManager.gameMode == GameMode.GunGame)
		{
			UserDataManager.Instance.IncrementProperty("GUNGAME_KILLS");
			if (killStreak > UserDataManager.Instance.GetIntProperty("GUNGAME_HIGHEST_KILLSTREAK"))
			{
				UserDataManager.Instance.SetProperty("GUNGAME_HIGHEST_KILLSTREAK", killStreak);
			}
		}
		else if (GameManager.gameManager.gameMode == GameMode.Infection)
		{
			if (infected)
			{
				UserDataManager.Instance.IncrementProperty("INFECTION_AS_INFECTED_KILLS");
			}
			else
			{
				UserDataManager.Instance.IncrementProperty("INFECTION_AS_CLEAN_KILLS");
			}
		}
		else if (GameManager.gameManager.gameMode == GameMode.BattleRoyale)
		{
			UserDataManager.Instance.IncrementProperty("BATTLEROYALE_KILLS");
			if (killStreak > UserDataManager.Instance.GetIntProperty("BATTLEROYALE_HIGHEST_KILLSTREAK"))
			{
				UserDataManager.Instance.SetProperty("BATTLEROYALE_HIGHEST_KILLSTREAK", killStreak);
			}
		}
		PhotonView photonView = PhotonView.Find(killedViewID);
		if (GameManager.gameManager.gameMode == GameMode.TTT)
		{
			PhotonPlayer component = photonView.GetComponent<PhotonPlayer>();
			bool flag = component.tttteam == TTTTeam.Detective || component.tttteam == TTTTeam.Innocent;
			bool flag2 = component.tttteam == TTTTeam.Traitor;
			bool flag3 = tttteam == TTTTeam.Detective || tttteam == TTTTeam.Innocent;
			bool flag4 = tttteam == TTTTeam.Traitor;
			if (tttteam == TTTTeam.Detective && component.tttteam == TTTTeam.Detective)
			{
				PlayerPrefs.SetInt("tttkarma", ClampKarma(PlayerPrefs.GetInt("tttkarma") - 13));
				teamkilledThisGame = true;
			}
			else if (flag3 && flag)
			{
				PlayerPrefs.SetInt("tttkarma", ClampKarma(PlayerPrefs.GetInt("tttkarma") - 7));
				teamkilledThisGame = true;
				UserDataManager.Instance.IncrementProperty("TTT_INNOCENT_BAD_KILLS");
			}
			else if (flag4 && flag2)
			{
				PlayerPrefs.SetInt("tttkarma", ClampKarma(PlayerPrefs.GetInt("tttkarma") - 3));
				teamkilledThisGame = true;
				UserDataManager.Instance.IncrementProperty("TTT_TRAITOR_BAD_KILLS");
			}
			else if (flag3 && flag2)
			{
				PlayerPrefs.SetInt("tttkarma", ClampKarma(PlayerPrefs.GetInt("tttkarma") + 7));
				goodKills++;
				ItemShop.shop.AddPoints(1);
				UserDataManager.Instance.IncrementProperty("TTT_INNOCENT_GOOD_KILLS");
			}
			else if (flag4 && flag)
			{
				goodKills++;
				ItemShop.shop.AddPoints(1);
				PlayerPrefs.SetInt("tttkarma", ClampKarma(PlayerPrefs.GetInt("tttkarma") + 3));
				UserDataManager.Instance.IncrementProperty("TTT_TRAITOR_GOOD_KILLS");
			}
		}
		else
		{
			if (GameManager.gameManager.gameMode == GameMode.GunGame)
			{
				if (currentGunGameItem == MultiplayerSettings.multiplayerSettings.gunGameWeapons.Length - 1)
				{
					GameManager.gameManager.LocalPlayerWinsTheGame();
				}
				else
				{
					IncrementGunGameItem();
				}
			}
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			int num = (int)PhotonNetwork.LocalPlayer.CustomProperties["kills"];
			int num2 = (int)PhotonNetwork.LocalPlayer.CustomProperties["deaths"];
			num++;
			hashtable.Add("kills", num);
			hashtable.Add("deaths", num2);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
			if (num == 1)
			{
				GameSetup.gameSetup.killAmountText.text = "1 Kill";
			}
			else
			{
				GameSetup.gameSetup.killAmountText.text = num + " Kills";
			}
			GameSetup.gameSetup.canvasAnim.SetTrigger("getKill");
		}
		GameSetup.gameSetup.killText.text = "Killed <b>" + photonView.Owner.NickName + "</b>";
	}

	private void IncrementGunGameItem()
	{
		currentGunGameItem++;
		avatarPlayerController.UpdateGunGameItem();
	}

	public void OnTTTGameEnded()
	{
		if (!teamkilledThisGame)
		{
			PlayerPrefs.SetInt("tttkarma", ClampKarma(PlayerPrefs.GetInt("tttkarma") + 10));
		}
		Debug.Log("TTT Game Ended");
	}

	private int ClampKarma(int karmaIn)
	{
		int result = karmaIn;
		if (karmaIn < 60)
		{
			result = 60;
		}
		else if (karmaIn > 100)
		{
			result = 100;
		}
		return result;
	}

	[PunRPC]
	private void RPC_SetAliveState(bool aliveState)
	{
		alive = aliveState;
	}

	public void Respawn()
	{
		if (!PV.IsMine)
		{
			return;
		}
		killStreak = 0;
		if (GameManager.gameManager.gameMode == GameMode.Skirmish)
		{
			UserDataManager.Instance.IncrementProperty("SKIRMISH_DEATHS");
		}
		else if (GameManager.gameManager.gameMode == GameMode.GunGame)
		{
			UserDataManager.Instance.IncrementProperty("GUNGAME_DEATHS");
		}
		else if (GameManager.gameManager.gameMode == GameMode.TTT)
		{
			if (tttteam == TTTTeam.Innocent || tttteam == TTTTeam.Detective)
			{
				UserDataManager.Instance.IncrementProperty("TTT_INNOCENT_DEATHS");
			}
			else if (tttteam == TTTTeam.Traitor)
			{
				UserDataManager.Instance.IncrementProperty("TTT_TRAITOR_DEATHS");
			}
		}
		else if (GameManager.gameManager.gameMode == GameMode.Infection)
		{
			if (infected)
			{
				UserDataManager.Instance.IncrementProperty("INFECTION_AS_INFECTED_DEATHS");
			}
			else
			{
				UserDataManager.Instance.IncrementProperty("INFECTION_AS_CLEAN_DEATHS");
			}
			RPC_AssignInfectedTeam();
		}
		else if (GameManager.gameManager.gameMode == GameMode.BattleRoyale)
		{
			UserDataManager.Instance.IncrementProperty("BATTLEROYALE_DEATHS");
		}
		PV.RPC("RPC_SetAliveState", RpcTarget.All, false);
		if (GameManager.gameManager.GetGameModeInfo().onPlayerDeath)
		{
			GameManager.gameManager.OnPlayerDeath();
		}
		if (GameManager.gameManager.GetGameModeInfo().elimination)
		{
			GameSetup.gameSetup.HUD.SetActive(false);
			CreateSpectatorAvatar(avatar.transform.position);
			RemoveAvatar();
			return;
		}
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		int num = (int)PhotonNetwork.LocalPlayer.CustomProperties["kills"];
		int num2 = (int)PhotonNetwork.LocalPlayer.CustomProperties["deaths"];
		num2++;
		hashtable.Add("kills", num);
		hashtable.Add("deaths", num2);
		PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
		StartCoroutine(TimedRespawn());
	}

	[PunRPC]
	private void RPC_ScoreboardDataUpdate()
	{
		ScoreboardManager.scoreboard.DataUpdate();
	}

	private IEnumerator TimedRespawn()
	{
		RemoveAvatar();
		GameSetup.gameSetup.respawnMenu.SetActive(true);
		GameSetup.gameSetup.defaultCamObj.SetActive(true);
		GameSetup.gameSetup.HUD.SetActive(false);
		float timer = GameManager.gameManager.GetGameModeInfo().respawnTime;
		while (timer > 0f)
		{
			GameSetup.gameSetup.respawnTimeText.text = timer.ToString("0.00");
			timer -= Time.deltaTime;
			yield return null;
		}
		GameSetup.gameSetup.defaultCamObj.SetActive(false);
		GameSetup.gameSetup.respawnMenu.SetActive(false);
		CreateAvatar();
	}

	private void CreateAvatar()
	{
		GameSetup.gameSetup.HUD.SetActive(true);
		PV.RPC("RPC_SetAliveState", RpcTarget.All, true);
		avatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerAvatar"), GameSetup.gameSetup.RandomSpawn(), Quaternion.identity, 0);
		avatarPlayerController = avatar.GetComponent<PlayerController>();
		avatarPlayerController.PV.RPC("RPC_SetPhotonPlayer", RpcTarget.AllBuffered, PV.ViewID);
		avatarPlayerController.PV.RPC("RPC_ApplySkin", RpcTarget.AllBuffered, SkinManager.skinManager.GetCurrentSkin().name);
	}

	private void CreateSpectatorAvatar(Vector3 position)
	{
		Object.Instantiate(spectatorAvatarPrefab, position, Quaternion.identity);
	}

	public void RemoveAvatar()
	{
		if ((bool)avatar)
		{
			PhotonNetwork.Destroy(avatar);
		}
	}
}
