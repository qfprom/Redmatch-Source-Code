using System.IO;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonRoomCustomMatch : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
	public static PhotonRoomCustomMatch room;

	private PhotonView PV;

	public bool isGameLoaded;

	public int currentScene;

	private Player[] photonPlayers;

	public int playersInRoom;

	public int myNumberInRoom;

	public int playersInGame;

	private bool readyToCount;

	private bool readyToStart;

	public float startingTime;

	private float lessThanMaxPlayers;

	private float atMaxPlayers;

	private float timeToStart;

	[SerializeField]
	private Animator canvasAnim;

	[SerializeField]
	private Button roomMenuToggleButton;

	[SerializeField]
	private Transform playersPanel;

	[SerializeField]
	private GameObject playerListingPrefab;

	[SerializeField]
	private GameObject startButton;

	[SerializeField]
	private Button createRoomButton;

	[SerializeField]
	private GameObject roomPasswordScreen;

	[SerializeField]
	private GameObject mainMenuScreen;

	[SerializeField]
	private Text roomNameText;

	[SerializeField]
	private GameObject createRoomScreen;

	[SerializeField]
	private InputField passwordEntryInputField;

	[HideInInspector]
	public RoomInfo selectedRoom;

	private void Awake()
	{
		if ((bool)room)
		{
			Object.Destroy(room.gameObject);
		}
		room = this;
		Object.DontDestroyOnLoad(base.gameObject);
		roomMenuToggleButton.interactable = false;
	}

	public void ForceCloseRoomMenu()
	{
		roomMenuToggleButton.interactable = false;
		canvasAnim.SetTrigger("hideRoomMenu");
		createRoomButton.interactable = true;
	}

	public void AllowRoomMenu()
	{
		roomMenuToggleButton.interactable = true;
		if (createRoomScreen.activeSelf)
		{
			createRoomScreen.SetActive(false);
			createRoomButton.interactable = false;
			mainMenuScreen.SetActive(true);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		PhotonNetwork.AddCallbackTarget(this);
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		PhotonNetwork.RemoveCallbackTarget(this);
		SceneManager.sceneLoaded -= OnSceneFinishedLoading;
	}

	private void Start()
	{
		PV = GetComponent<PhotonView>();
		readyToCount = false;
		readyToStart = false;
		if (PhotonNetwork.CurrentRoom != null)
		{
			JoinedRoomSetup();
		}
		lessThanMaxPlayers = startingTime;
		atMaxPlayers = 6f;
		timeToStart = startingTime;
	}

	private void Update()
	{
		if (!MultiplayerSettings.multiplayerSettings.delayStart)
		{
			return;
		}
		if (playersInRoom == 1)
		{
			RestartTimer();
		}
		if (!isGameLoaded)
		{
			if (readyToStart)
			{
				atMaxPlayers -= Time.deltaTime;
				lessThanMaxPlayers = atMaxPlayers;
				timeToStart = atMaxPlayers;
			}
			else if (readyToCount)
			{
				lessThanMaxPlayers -= Time.deltaTime;
				timeToStart = lessThanMaxPlayers;
			}
			Debug.Log("Display time to show to players: " + timeToStart);
			if (timeToStart <= 0f)
			{
				StartGame();
			}
		}
	}

	public void EnterPassword()
	{
		if (passwordEntryInputField.text == selectedRoom.Name.Split('|')[0])
		{
			PhotonNetwork.JoinRoom(selectedRoom.Name);
			roomPasswordScreen.SetActive(false);
			passwordEntryInputField.text = string.Empty;
		}
		else
		{
			PhotonLobbyCustomMatch.lobby.ShowInfoText("Incorrect password.");
		}
	}

	public void StartPasswordEntry()
	{
		PhotonLobbyCustomMatch.lobby.roomSelectMenu.SetActive(false);
		roomPasswordScreen.SetActive(true);
	}

	public void CancelPasswordEntry()
	{
		PhotonLobbyCustomMatch.lobby.roomSelectMenu.SetActive(true);
		roomPasswordScreen.SetActive(false);
		passwordEntryInputField.text = string.Empty;
	}

	private void JoinedRoomSetup()
	{
		if (!GameSetup.gameSetup)
		{
			roomPasswordScreen.SetActive(false);
			AllowRoomMenu();
			RemovePlayerListings();
			RenderPlayerListings();
		}
		string[] array = PhotonNetwork.CurrentRoom.Name.Split('|');
		roomNameText.text = "Room <b>" + array[array.Length - 1] + "</b>";
		photonPlayers = PhotonNetwork.PlayerList;
		playersInRoom = photonPlayers.Length;
		myNumberInRoom = playersInRoom;
		DiscordController.dc.presence.partyMax = PhotonNetwork.CurrentRoom.MaxPlayers;
		DiscordController.dc.presence.partySize = playersInRoom;
		DiscordController.dc.presence.state = "Room " + PhotonNetwork.CurrentRoom.Name.Split('|')[1];
		DiscordRpc.UpdatePresence(ref DiscordController.dc.presence);
		if (PhotonNetwork.IsMasterClient)
		{
			startButton.SetActive(true);
			if (!GameSetup.gameSetup)
			{
				if (RoomData.data.gameMode == GameMode.TTT || RoomData.data.gameMode == GameMode.GunGame || RoomData.data.gameMode == GameMode.Infection)
				{
					if (playersInRoom < 2)
					{
						startButton.GetComponent<Button>().interactable = false;
					}
					else
					{
						startButton.GetComponent<Button>().interactable = true;
					}
				}
				else
				{
					startButton.GetComponent<Button>().interactable = true;
				}
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("enteredScene", false);
			hashtable.Add("password", RoomData.data.roomPassword);
			hashtable.Add("gameMode", RoomData.data.gameMode);
			hashtable.Add("gameStarted", false);
			PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
		}
		else
		{
			startButton.SetActive(false);
		}
		if (!MultiplayerSettings.multiplayerSettings.delayStart)
		{
			return;
		}
		if (playersInRoom > 1)
		{
			readyToCount = true;
		}
		if (playersInRoom == MultiplayerSettings.multiplayerSettings.maxPlayers)
		{
			readyToStart = true;
			if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
			}
		}
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		if (!GameSetup.gameSetup)
		{
			canvasAnim.SetTrigger("joinedRoom");
		}
		JoinedRoomSetup();
	}

	private void RemovePlayerListings()
	{
		foreach (Transform item in playersPanel)
		{
			if (item.gameObject != null && item != playersPanel)
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void RenderPlayerListings()
	{
		if (PhotonNetwork.InRoom)
		{
			Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Player player in playerList)
			{
				GameObject gameObject = Object.Instantiate(playerListingPrefab, playersPanel);
				gameObject.transform.GetChild(0).GetComponent<Text>().text = player.NickName;
				gameObject.transform.GetChild(1).gameObject.SetActive(player.IsMasterClient);
			}
		}
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		base.OnMasterClientSwitched(newMasterClient);
		if (PhotonNetwork.IsMasterClient)
		{
			startButton.SetActive(true);
			if ((RoomData.data.gameMode == GameMode.TTT || RoomData.data.gameMode == GameMode.GunGame || RoomData.data.gameMode == GameMode.Infection) && !GameSetup.gameSetup)
			{
				photonPlayers = PhotonNetwork.PlayerList;
				playersInRoom = photonPlayers.Length;
				if (playersInRoom < 2)
				{
					startButton.GetComponent<Button>().interactable = false;
				}
				else
				{
					startButton.GetComponent<Button>().interactable = true;
				}
			}
		}
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.SendGameInfo("<b>" + newMasterClient.NickName + "</b> has been promoted to host.");
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		photonPlayers = PhotonNetwork.PlayerList;
		playersInRoom = photonPlayers.Length;
		DiscordController.dc.presence.partySize = playersInRoom;
		DiscordRpc.UpdatePresence(ref DiscordController.dc.presence);
		if ((RoomData.data.gameMode == GameMode.TTT || RoomData.data.gameMode == GameMode.GunGame || RoomData.data.gameMode == GameMode.Infection) && !GameSetup.gameSetup)
		{
			if (playersInRoom < 2)
			{
				startButton.GetComponent<Button>().interactable = false;
			}
			else
			{
				startButton.GetComponent<Button>().interactable = true;
			}
		}
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.SendGameInfo("<b>" + newPlayer.NickName + "</b> joined the game.");
		}
		if (playersInRoom > 1)
		{
			readyToCount = true;
		}
		if (!GameSetup.gameSetup)
		{
			RemovePlayerListings();
			RenderPlayerListings();
		}
		if (playersInRoom == MultiplayerSettings.multiplayerSettings.maxPlayers)
		{
			readyToStart = true;
			if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
			}
		}
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		DiscordController.dc.presence.partyMax = 0;
		DiscordController.dc.presence.partySize = 0;
		DiscordController.dc.presence.state = "In Lobby";
		DiscordRpc.UpdatePresence(ref DiscordController.dc.presence);
		if (!GameSetup.gameSetup)
		{
			ForceCloseRoomMenu();
		}
	}

	public void OnGameModeDropdownChanged(int newGameMode)
	{
		Debug.Log(newGameMode);
		RoomData.data.gameMode = (GameMode)newGameMode;
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.SendGameInfo("<b>" + otherPlayer.NickName + "</b> left the game.");
		}
		photonPlayers = PhotonNetwork.PlayerList;
		playersInRoom = photonPlayers.Length;
		DiscordController.dc.presence.partySize = playersInRoom;
		DiscordRpc.UpdatePresence(ref DiscordController.dc.presence);
		if (!GameSetup.gameSetup)
		{
			if (RoomData.data.gameMode == GameMode.TTT || RoomData.data.gameMode == GameMode.GunGame || RoomData.data.gameMode == GameMode.Infection)
			{
				if (playersInRoom < 2)
				{
					startButton.GetComponent<Button>().interactable = false;
				}
				else
				{
					startButton.GetComponent<Button>().interactable = true;
				}
			}
			RemovePlayerListings();
			RenderPlayerListings();
		}
		if (playersInRoom < MultiplayerSettings.multiplayerSettings.maxPlayers)
		{
			readyToStart = false;
			if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.CurrentRoom.IsOpen = true;
			}
		}
	}

	public void LeaveRoom()
	{
		if (PhotonNetwork.InRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		PhotonNetwork.JoinLobby();
		if (!GameSetup.gameSetup)
		{
			RemovePlayerListings();
		}
	}

	public void StartGame()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (MultiplayerSettings.multiplayerSettings.delayStart)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
			}
			LoadingScreenManager.Instance.RequestLoadingScreen("Loading Room");
			PhotonNetwork.LoadLevel(RoomData.data.map);
		}
	}

	private void RestartTimer()
	{
		lessThanMaxPlayers = startingTime;
		timeToStart = startingTime;
		atMaxPlayers = 6f;
		readyToCount = false;
		readyToStart = false;
	}

	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		currentScene = scene.buildIndex;
		if (currentScene != MultiplayerSettings.multiplayerSettings.menuScene && scene.name != "Unfriendly")
		{
			isGameLoaded = true;
			if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["enteredScene"])
			{
				RPC_CreatePlayer();
			}
			else
			{
				PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
			}
		}
	}

	[PunRPC]
	private void RPC_LoadedGameScene()
	{
		playersInGame++;
		if (playersInGame == PhotonNetwork.PlayerList.Length)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("gameStarted", false);
			hashtable.Add("enteredScene", true);
			PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
			Invoke("CreatePlayers", 5f);
		}
	}

	private void CreatePlayers()
	{
		PV.RPC("RPC_CreatePlayer", RpcTarget.All);
	}

	[PunRPC]
	private void RPC_CreatePlayer()
	{
		PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), Vector3.zero, Quaternion.identity, 0);
	}
}
