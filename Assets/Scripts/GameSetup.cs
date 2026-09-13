using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
	public static GameSetup gameSetup;

	[HideInInspector]
	public PhotonPlayer player;

	public Image healthAmountImage;

	public Image healthAmountBackgroundImage;

	public Text healthAmountText;

	public Text ammoText;

	public Text totalAmmoText;

	public Text killAmountText;

	public Text killText;

	public Animator canvasAnim;

	public InputField chatInputField;

	public Recorder voiceRecorder;

	public GameObject loadingScreen;

	public Text teamText;

	public Text stageText;

	public Text timerText;

	public GameObject winScreen;

	public Text winText;

	public Image winTitleBackground;

	public Text winDescriptionText;

	public ItemInfo[] spawnItems;

	public Button chatChannelButton;

	public Text interactText;

	public Transform inventoryContainer;

	public GameObject inventorySlotPrefab;

	public GameObject bodyIdentificationPanel;

	public Text bodyInfoText;

	public Text[] itemInfoEntries;

	public GameObject itemInfoPanel;

	public Text shopPointCountText;

	public Text shopCostText;

	public Text shopTitleText;

	public Text shopDescriptionText;

	public Transform shopContent;

	public GameObject shopListingPrefab;

	public FireModeImage[] fireModeImages;

	[SerializeField]
	private Transform gameInfoContainer;

	[SerializeField]
	private GameObject gameInfoPrefab;

	public GameObject HUD;

	public GameObject respawnMenu;

	public Text respawnTimeText;

	[HideInInspector]
	public List<Transform> spawnPoints;

	[HideInInspector]
	public List<Transform> gunSpawnPoints;

	[SerializeField]
	private Transform spawnPointsContainer;

	[SerializeField]
	private Transform gunSpawnPointsContainer;

	public GameObject defaultCamObj;

	public Text pingText;

	[SerializeField]
	private Text fpsText;

	public GameMode gameMode;

	private void Awake()
	{
		gameSetup = this;
		foreach (Transform item3 in spawnPointsContainer)
		{
			spawnPoints.Add(item3);
		}
		foreach (Transform item4 in gunSpawnPointsContainer)
		{
			gunSpawnPoints.Add(item4);
		}
	}

	private void Start()
	{
		if (GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
		{
			totalAmmoText.gameObject.SetActive(false);
		}
	}

	public void RequestItemInfoRender(string[] itemInfo)
	{
		if (itemInfoEntries.Length < itemInfo.Length)
		{
			Debug.LogError("Not enough item info slots for requested info.");
		}
		int num = 0;
		Text[] array = itemInfoEntries;
		foreach (Text text in array)
		{
			if (num < itemInfo.Length)
			{
				text.gameObject.SetActive(true);
				text.text = itemInfo[num];
			}
			else
			{
				text.gameObject.SetActive(false);
			}
			num++;
		}
	}

	private void Update()
	{
		pingText.text = PhotonNetwork.GetPing() + "ms";
		fpsText.text = (1f / Time.deltaTime).ToString("0") + "fps";
	}

	public Vector3 RandomSpawn()
	{
		int index = Random.Range(0, spawnPoints.Count);
		return spawnPoints[index].position;
	}

	public void DisconnectPlayer()
	{
		Object.Destroy(PhotonRoomCustomMatch.room.gameObject);
		Object.Destroy(RoomData.data.gameObject);
		StartCoroutine(DisconnectAndLoad());
	}

	public void DisconnectPlayerAndDontLoad()
	{
		Object.Destroy(PhotonRoomCustomMatch.room.gameObject);
		Object.Destroy(RoomData.data.gameObject);
		StartCoroutine(Disconnect());
	}

	private IEnumerator DisconnectAndLoad()
	{
		defaultCamObj.SetActive(true);
		GameMenuManager.menuManager.CloseMenu("escape");
		LoadingScreenManager.Instance.RequestLoadingScreen("Disconnecting and Returning to Lobby");
		DiscordController.dc.presence.partyMax = 0;
		DiscordController.dc.presence.partySize = 0;
		DiscordController.dc.presence.state = "In Lobby";
		DiscordRpc.UpdatePresence(ref DiscordController.dc.presence);
		player.RemoveAvatar();
		if (GameManager.gameManager.GetGameModeInfo().elimination && player.inGame)
		{
			GameManager.gameManager.OnPlayerDeath();
		}
		if (PhotonNetwork.InRoom)
		{
			PhotonNetwork.LeaveRoom();
			while (PhotonNetwork.InRoom)
			{
				yield return null;
			}
			PhotonNetwork.JoinLobby();
		}
		yield return new WaitForSecondsRealtime(1f);
		SceneManager.LoadScene(MultiplayerSettings.multiplayerSettings.menuScene);
	}

	private IEnumerator Disconnect()
	{
		defaultCamObj.SetActive(true);
		GameMenuManager.menuManager.CloseMenu("escape");
		LoadingScreenManager.Instance.RequestLoadingScreen("Disconnecting and Returning to Lobby");
		DiscordController.dc.presence.partyMax = 0;
		DiscordController.dc.presence.partySize = 0;
		DiscordController.dc.presence.state = "In Lobby";
		DiscordRpc.UpdatePresence(ref DiscordController.dc.presence);
		player.RemoveAvatar();
		if (GameManager.gameManager.GetGameModeInfo().elimination && player.inGame)
		{
			GameManager.gameManager.OnPlayerDeath();
		}
		if (PhotonNetwork.InRoom)
		{
			PhotonNetwork.LeaveRoom();
			while (PhotonNetwork.InRoom)
			{
				yield return null;
			}
			PhotonNetwork.JoinLobby();
		}
	}

	public void SoftDisconnect()
	{
		defaultCamObj.SetActive(true);
		GameMenuManager.menuManager.CloseMenu("escape");
		LoadingScreenManager.Instance.RequestLoadingScreen("Disconnecting and Returning to Lobby");
		Object.Destroy(PhotonRoomCustomMatch.room.gameObject);
		player.RemoveAvatar();
		PhotonNetwork.Destroy(player.PV);
		if (GameManager.gameManager.GetGameModeInfo().elimination && player.inGame)
		{
			GameManager.gameManager.OnPlayerDeath();
		}
		SceneManager.LoadScene(MultiplayerSettings.multiplayerSettings.menuScene);
	}

	public void SendGameInfo(string info)
	{
		GameObject gameObject = Object.Instantiate(gameInfoPrefab, gameInfoContainer);
		gameObject.GetComponent<Text>().text = info;
		Object.Destroy(gameObject, 15f);
	}

	public void OnChatChannelButtonPressed()
	{
		GameMenuManager.menuManager.OnChatChannelButtonPressed();
	}
}
