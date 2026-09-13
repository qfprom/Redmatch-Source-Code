using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PhotonLobbyCustomMatch : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
	public static PhotonLobbyCustomMatch lobby;

	[SerializeField]
	private Text infoText;

	[SerializeField]
	private Text usernameText;

	[SerializeField]
	private GameObject loginScreen;

	[SerializeField]
	private InputField playerNameInput;

	[SerializeField]
	private GameObject mainScreen;

	public GameObject roomSelectMenu;

	[SerializeField]
	private GameObject roomListingPrefab;

	[SerializeField]
	private Transform roomsPanel;

	[SerializeField]
	private Text roomCreationMaxPlayersText;

	[SerializeField]
	private Text currentPlayersText;

	[SerializeField]
	private Button findRoomButton;

	[SerializeField]
	private Transform mapsPanel;

	[SerializeField]
	private GameObject mapListingPrefab;

	[SerializeField]
	private Text mapText;

	[SerializeField]
	private Button standingsButton;

	[SerializeField]
	private GameObject newRoomNotificationPrefab;

	[SerializeField]
	private Transform newRoomNotificationContainer;

	private List<RoomInfo> previousRoomList = new List<RoomInfo>();

	private bool mainMenuAvailable;

	private void Awake()
	{
		lobby = this;
	}

	private void Start()
	{
		if (PhotonNetwork.IsConnected)
		{
			if (PhotonNetwork.CurrentRoom == null)
			{
				PhotonRoomCustomMatch.room.ForceCloseRoomMenu();
				RoomData.data.roomName = PhotonNetwork.NickName + "'s Room";
			}
			mainScreen.SetActive(true);
			usernameText.text = "Signed in as <b>" + PhotonNetwork.NickName + "</b>";
			OnDataLoaded();
			loginScreen.SetActive(false);
			findRoomButton.interactable = true;
		}
		else
		{
			PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = File.ReadAllText(Path.Combine(Path.Combine(Application.dataPath, ".."), "server.txt"));
			PhotonNetwork.ConnectUsingSettings();
		}
		mapText.text = "Map: " + GetMapName(RoomData.data.map);
	}

	public void FindRoom()
	{
		if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
		{
			PhotonNetwork.JoinLobby();
		}
	}

	public void OnDataLoaded()
	{
		standingsButton.interactable = true;
		SkinManager.skinManager.Setup();
	}

	private string GetMapName(int buildIndex)
	{
		string result = "No Map";
		Map[] maps = MultiplayerSettings.multiplayerSettings.maps;
		for (int i = 0; i < maps.Length; i++)
		{
			Map map = maps[i];
			if (map.buildIndex == buildIndex)
			{
				result = map.name;
				break;
			}
		}
		return result;
	}

	private void Update()
	{
		int num = PhotonNetwork.CountOfPlayersOnMaster + PhotonNetwork.CountOfPlayersInRooms;
		if (num == 1)
		{
			currentPlayersText.text = "1 Current Player";
		}
		else
		{
			currentPlayersText.text = num + " Current Players";
		}
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
		if (mainMenuAvailable)
		{
			LoggedInSetup();
		}
	}

	public override void OnJoinedLobby()
	{
		base.OnJoinedLobby();
		findRoomButton.interactable = true;
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		if (SettingsManager.settings.showRoomNotifications && UserDataManager.IsLoggedIn)
		{
			foreach (RoomInfo room in roomList)
			{
				if (!previousRoomList.Contains(room) && room.PlayerCount > 0)
				{
					Object.Instantiate(newRoomNotificationPrefab, newRoomNotificationContainer).GetComponent<NewRoomNotification>().AssignRoom(room);
				}
			}
		}
		previousRoomList = roomList;
		RemoveRoomListings();
		RenderRoomListings(roomList);
	}

	private void RemoveRoomListings()
	{
		foreach (Transform item in roomsPanel)
		{
			Object.Destroy(item.gameObject);
		}
	}

	private void RenderRoomListings(List<RoomInfo> roomList)
	{
		foreach (RoomInfo room in roomList)
		{
			if (room.PlayerCount != 0)
			{
				ListRoom(room);
			}
		}
	}

	private void ListRoom(RoomInfo room)
	{
		if (room.IsOpen && room.IsVisible)
		{
			GameObject gameObject = Object.Instantiate(roomListingPrefab, roomsPanel);
			RoomButton component = gameObject.GetComponent<RoomButton>();
			component.SetRoom(room, room.PlayerCount, room.MaxPlayers);
		}
	}

	public void CreateRoom()
	{
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = true;
		roomOptions.IsOpen = true;
		roomOptions.MaxPlayers = (byte)RoomData.data.roomSize;
		RoomOptions roomOptions2 = roomOptions;
		if (RoomData.data.roomPassword.Contains("|"))
		{
			ShowInfoText("Invalid password");
		}
		else if (RoomData.data.roomName.Contains("|"))
		{
			ShowInfoText("Invalid name");
		}
		else
		{
			PhotonNetwork.CreateRoom(RoomData.data.roomPassword + "|" + ((!string.IsNullOrEmpty(RoomData.data.roomName)) ? RoomData.data.roomName : (PhotonNetwork.NickName + "'s Room")), roomOptions2);
		}
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		ShowInfoText("Room failed to create, there must be another room with the same name.");
	}

	public void OnRoomNameChanged(string name)
	{
		RoomData.data.roomName = name;
	}

	public void OnRoomSizeChanged(float size)
	{
		RoomData.data.roomSize = (int)size;
		roomCreationMaxPlayersText.text = "Max Players: " + size;
	}

	public void SetMap(int mapIndex)
	{
		RoomData.data.map = mapIndex;
		mapText.text = "Map: " + GetMapName(mapIndex);
	}

	private void RemoveMapListings()
	{
		foreach (Transform item in mapsPanel)
		{
			if (item.gameObject != null)
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void RenderMapListings()
	{
		Map[] maps = MultiplayerSettings.multiplayerSettings.maps;
		for (int i = 0; i < maps.Length; i++)
		{
			Map map = maps[i];
			GameObject gameObject = Object.Instantiate(mapListingPrefab, mapsPanel);
			gameObject.GetComponent<MapButton>().SetMap(map.name, map.image, map.buildIndex);
		}
	}

	public void OnRoomBeginCreation()
	{
		RemoveMapListings();
		RenderMapListings();
	}

	public void OnRoomPasswordChanged(string newPassword)
	{
		RoomData.data.roomPassword = newPassword;
	}

	public void LoggedInSetup()
	{
		RoomData.data.roomName = UserDataManager.PlayerUsername + "'s Room";
		if (Application.isEditor)
		{
			PhotonNetwork.LocalPlayer.NickName = "<i>[From The Unity Editor]</i>: " + UserDataManager.PlayerUsername;
		}
		else
		{
			PhotonNetwork.LocalPlayer.NickName = UserDataManager.PlayerUsername;
		}
		usernameText.text = "Logged in as <b>" + UserDataManager.PlayerUsername + "</b>";
		LoadingScreenManager.Instance.ResolveLoadingScreen();
		if (!PhotonNetwork.InLobby)
		{
			PhotonNetwork.JoinLobby();
		}
		mainScreen.SetActive(true);
	}

	public void ShowInfoText(string text)
	{
		infoText.gameObject.SetActive(true);
		infoText.text = text;
		CancelInvoke("HideInfoText");
		Invoke("HideInfoText", 2f);
	}

	private void HideInfoText()
	{
		infoText.gameObject.SetActive(false);
	}

	public void OnLoggedIn()
	{
		mainMenuAvailable = true;
		PhotonNetwork.NickName = UserDataManager.PlayerUsername;
		if (PhotonNetwork.IsConnected)
		{
			LoggedInSetup();
		}
	}

	public void OnLoggedOut()
	{
		mainMenuAvailable = false;
		mainScreen.SetActive(false);
	}
}
