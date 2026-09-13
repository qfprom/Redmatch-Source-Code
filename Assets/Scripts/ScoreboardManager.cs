using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
	public static ScoreboardManager scoreboard;

	[SerializeField]
	private GameObject playerScoreboardListingPrefab;

	[SerializeField]
	private GameObject scoreboardContainer;

	[SerializeField]
	private Transform scoreboardContent;

	private void Awake()
	{
		scoreboard = this;
	}

	private void Update()
	{
		if (InputManager.IM.GetButtonDown("Scoreboard"))
		{
			DataUpdate();
			scoreboardContainer.SetActive(true);
			if (PhotonNetwork.IsMasterClient)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
		else if (InputManager.IM.GetButtonUp("Scoreboard"))
		{
			scoreboardContainer.SetActive(false);
			if (GameMenuManager.menuManager.GetMenuCount() == 0 && PhotonNetwork.IsMasterClient)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}

	public void DataUpdate()
	{
		RemovePlayerListings();
		RenderPlayerListings();
	}

	public override void OnJoinedRoom()
	{
		DataUpdate();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		DataUpdate();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		DataUpdate();
	}

	private void RemovePlayerListings()
	{
		foreach (Transform item in scoreboardContent)
		{
			if (item.gameObject != null && item != scoreboard)
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void RenderPlayerListings()
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			GameObject gameObject = Object.Instantiate(playerScoreboardListingPrefab, scoreboardContent);
			gameObject.transform.GetChild(0).GetComponent<Text>().text = player.NickName;
			if (GameManager.gameManager.gameMode == GameMode.TTT)
			{
				if (GameManager.gameManager.gameStarted && (bool)player.CustomProperties["inGame"])
				{
					TTTTeam tTTTeam = (TTTTeam)player.CustomProperties["tttteam"];
					if (GameSetup.gameSetup.player.tttteam == TTTTeam.Traitor && tTTTeam == TTTTeam.Traitor)
					{
						gameObject.GetComponent<Image>().color = Color.red;
					}
					if (tTTTeam == TTTTeam.Detective)
					{
						gameObject.GetComponent<Image>().color = Color.blue;
					}
				}
				int num = (int)player.CustomProperties["tttkarma"];
				gameObject.transform.GetChild(1).GetComponent<Text>().text = "Karma: " + num;
				gameObject.transform.GetChild(2).gameObject.SetActive(false);
				gameObject.transform.GetChild(3).gameObject.SetActive(false);
			}
			else if (GameManager.gameManager.gameMode == GameMode.GunGame)
			{
				int num2 = (int)player.CustomProperties["kills"];
				int num3 = (int)player.CustomProperties["deaths"];
				gameObject.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0} ({1}/{2})", MultiplayerSettings.multiplayerSettings.gunGameWeapons[num2].itemName, num2, MultiplayerSettings.multiplayerSettings.gunGameWeapons.Length);
				gameObject.transform.GetChild(2).gameObject.SetActive(false);
				gameObject.transform.GetChild(3).GetComponent<Text>().text = "K/D: " + ((float)num2 / Mathf.Max(num3, 1f)).ToString("0.0");
			}
			else
			{
				int num4 = (int)player.CustomProperties["kills"];
				int num5 = (int)player.CustomProperties["deaths"];
				gameObject.transform.GetChild(1).GetComponent<Text>().text = "Kills: " + num4;
				gameObject.transform.GetChild(2).GetComponent<Text>().text = "Deaths: " + num5;
				gameObject.transform.GetChild(3).GetComponent<Text>().text = "K/D: " + ((float)num4 / Mathf.Max(num5, 1f)).ToString("0.0");
				if (GameManager.gameManager.gameMode == GameMode.Infection && (bool)player.CustomProperties["infected"])
				{
					gameObject.GetComponent<Image>().color = Color.green;
				}
			}
			if (PhotonNetwork.IsMasterClient && player != PhotonNetwork.LocalPlayer)
			{
				gameObject.transform.GetChild(4).GetComponent<KickButton>().targetPlayer = player;
			}
			else
			{
				gameObject.transform.GetChild(4).gameObject.SetActive(false);
			}
		}
	}
}
