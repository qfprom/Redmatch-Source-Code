using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
	public static PhotonLobby lobby;

	[SerializeField]
	private Button playButton;

	[SerializeField]
	private GameObject cancelButtonGO;

	[SerializeField]
	private GameObject loadingScreen;

	[SerializeField]
	private Text infoText;

	private void Awake()
	{
		lobby = this;
	}

	private void Start()
	{
		LoadingScreenManager.Instance.RequestLoadingScreen("Connecting to Server");
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		ShowInfoText("Connected to master server");
		LoadingScreenManager.Instance.ResolveLoadingScreen();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public void OnPlayButtonClicked()
	{
		playButton.gameObject.SetActive(false);
		cancelButtonGO.SetActive(true);
		PhotonNetwork.JoinRandomRoom();
	}

	public void OnCancelButtonClicked()
	{
		playButton.gameObject.SetActive(true);
		cancelButtonGO.SetActive(false);
		PhotonNetwork.LeaveRoom();
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		ShowInfoText("Room failed to join. There must be no open rooms available!");
		CreateRoom();
	}

	private void CreateRoom()
	{
		int num = Random.Range(0, 10000);
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = true;
		roomOptions.IsOpen = true;
		roomOptions.MaxPlayers = (byte)MultiplayerSettings.multiplayerSettings.maxPlayers;
		RoomOptions roomOptions2 = roomOptions;
		PhotonNetwork.CreateRoom("Room" + num, roomOptions2);
		ShowInfoText("Room created with name Room" + num);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		ShowInfoText("Room failed to create, there must be another room with the same name. Retrying room creation.");
		CreateRoom();
	}

	public void ShowInfoText(string text)
	{
		StopCoroutine(HideInfoText());
		infoText.gameObject.SetActive(true);
		infoText.text = text;
		StartCoroutine(HideInfoText());
	}

	private IEnumerator HideInfoText()
	{
		yield return new WaitForSecondsRealtime(2f);
		infoText.gameObject.SetActive(false);
	}
}
