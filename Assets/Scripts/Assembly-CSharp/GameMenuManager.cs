using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
	public static GameMenuManager menuManager;

	[HideInInspector]
	public bool paused;

	[HideInInspector]
	public bool lockControl;

	[SerializeField]
	private bool leaveControlUnlocked;

	[HideInInspector]
	public GameChatChannel currentChatChannel;

	private bool canChatThisFrame;

	[SerializeField]
	private Menu[] menues;

	private void Awake()
	{
		menuManager = this;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ToggleMenu("escape");
		}
		if (!GameSetup.gameSetup)
		{
			return;
		}
		if (InputManager.IM.GetButtonDown("Chat"))
		{
			if (GameSetup.gameSetup.chatInputField.gameObject.activeSelf)
			{
				StopChatting();
			}
			else if (canChatThisFrame)
			{
				StartChatting();
			}
		}
		canChatThisFrame = true;
	}

	private void StopChatting()
	{
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.chatInputField.gameObject.SetActive(false);
			GameSetup.gameSetup.chatInputField.text = string.Empty;
			if (!leaveControlUnlocked)
			{
				Cursor.visible = false;
			}
			if (GetMenuCount() >= 0)
			{
				lockControl = false;
			}
		}
	}

	private void StartChatting()
	{
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.chatInputField.gameObject.SetActive(true);
			GameSetup.gameSetup.chatInputField.Select();
			GameSetup.gameSetup.chatInputField.ActivateInputField();
			CloseAllMenues();
			if (paused)
			{
				Resume();
			}
			lockControl = true;
		}
	}

	public void OnChatChannelButtonPressed()
	{
		if (currentChatChannel == GameChatChannel.Team)
		{
			currentChatChannel = GameChatChannel.All;
		}
		else
		{
			currentChatChannel = GameChatChannel.Team;
		}
		GameSetup.gameSetup.chatChannelButton.transform.GetChild(0).GetComponent<Text>().text = currentChatChannel.ToString();
		Image component = GameSetup.gameSetup.chatChannelButton.GetComponent<Image>();
		component.color = Color.white;
		if (currentChatChannel == GameChatChannel.Team)
		{
			if (GameSetup.gameSetup.player.tttteam == TTTTeam.Traitor)
			{
				GameSetup.gameSetup.chatChannelButton.GetComponent<Image>().color = Color.red;
			}
			else if (GameSetup.gameSetup.player.tttteam == TTTTeam.Detective)
			{
				GameSetup.gameSetup.chatChannelButton.GetComponent<Image>().color = Color.blue;
			}
		}
	}

	private void CloseAllMenues()
	{
		Menu[] array = menues;
		for (int i = 0; i < array.Length; i++)
		{
			Menu menu = array[i];
			menu.menuObj.SetActive(false);
		}
		lockControl = false;
		if (!leaveControlUnlocked)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void RequestMenu(string menuName)
	{
		bool flag = false;
		bool flag2 = false;
		Menu[] array = menues;
		for (int i = 0; i < array.Length; i++)
		{
			Menu menu = array[i];
			if (menu.menuName == menuName)
			{
				if (menu.pause)
				{
					Pause();
				}
				StopChatting();
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				lockControl = true;
				flag2 = true;
				flag = menu.exclusive;
				if ((bool)menu.menuObj)
				{
					menu.menuObj.SetActive(true);
				}
				else
				{
					Debug.LogError("Menu object missing with menu name " + menu.menuName);
				}
				break;
			}
		}
		if (flag2)
		{
			if (!flag)
			{
				return;
			}
			Menu[] array2 = menues;
			for (int j = 0; j < array2.Length; j++)
			{
				Menu menu2 = array2[j];
				if (menu2.menuName != menuName)
				{
					menu2.menuObj.SetActive(false);
				}
			}
		}
		else
		{
			Debug.LogError("Menu with name " + menuName + " not found");
		}
	}

	public void CloseMenu(string menuName)
	{
		bool flag = false;
		Menu[] array = menues;
		for (int i = 0; i < array.Length; i++)
		{
			Menu menu = array[i];
			if (!(menu.menuName == menuName))
			{
				continue;
			}
			if (menu.pause)
			{
				Resume();
			}
			menu.menuObj.SetActive(false);
			flag = true;
			if (GetMenuCount() <= 0)
			{
				lockControl = false;
				if (!leaveControlUnlocked)
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}
		}
		if (!flag)
		{
			Debug.LogError("Menu with name " + menuName + " not found");
		}
	}

	public void ToggleMenu(string menuName)
	{
		Menu[] array = menues;
		for (int i = 0; i < array.Length; i++)
		{
			Menu menu = array[i];
			if (menu.menuName == menuName)
			{
				if (menu.menuObj.activeSelf)
				{
					CloseMenu(menuName);
				}
				else
				{
					RequestMenu(menuName);
				}
				break;
			}
		}
	}

	public void Pause()
	{
		paused = true;
	}

	public void Resume()
	{
		paused = false;
	}

	public void OnFinishChatEntry(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (GameManager.gameManager.gameMode == GameMode.TTT)
			{
				GameManager.gameManager.PV.RPC("RPC_TTTGlobalChat", RpcTarget.All, currentChatChannel, "<b>" + PhotonNetwork.NickName + "</b>: " + text, GameSetup.gameSetup.player.tttteam);
			}
			else
			{
				GameManager.gameManager.PV.RPC("RPC_GlobalChat", RpcTarget.All, GameChatChannel.All, "<b>" + PhotonNetwork.NickName + "</b>: " + text);
			}
		}
		canChatThisFrame = false;
		StopChatting();
	}

	public int GetMenuCount()
	{
		int num = 0;
		Menu[] array = menues;
		for (int i = 0; i < array.Length; i++)
		{
			Menu menu = array[i];
			if (menu.menuObj.activeSelf)
			{
				num++;
			}
		}
		return num;
	}
}
