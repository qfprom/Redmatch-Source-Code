using System.Collections;
using DatabaseControl;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
	public GameObject loginParent;

	public GameObject registerParent;

	public InputField Login_UsernameField;

	public InputField Login_PasswordField;

	public InputField Register_UsernameField;

	public InputField Register_PasswordField;

	public InputField Register_ConfirmPasswordField;

	public Text Login_ErrorText;

	public Text Register_ErrorText;

	private string playerUsername = string.Empty;

	private string playerPassword = string.Empty;

	private void Awake()
	{
		ResetAllUIElements();
	}

	private void ResetAllUIElements()
	{
		Login_UsernameField.text = PlayerPrefs.GetString("username");
		Login_PasswordField.text = string.Empty;
		Register_UsernameField.text = string.Empty;
		Register_PasswordField.text = string.Empty;
		Register_ConfirmPasswordField.text = string.Empty;
		Login_ErrorText.text = string.Empty;
		Register_ErrorText.text = string.Empty;
	}

	private IEnumerator LoginUser()
	{
		IEnumerator e = DCF.Login(playerUsername, playerPassword);
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Success")
		{
			ResetAllUIElements();
			LoadingScreenManager.Instance.ResolveLoadingScreen();
			PlayerPrefs.SetString("username", playerUsername);
			UserDataManager.Instance.OnLogIn(playerUsername, playerPassword);
			PhotonLobbyCustomMatch.lobby.OnLoggedIn();
			yield break;
		}
		LoadingScreenManager.Instance.ResolveLoadingScreen();
		loginParent.gameObject.SetActive(true);
		if (response == "UserError")
		{
			Login_ErrorText.text = "Error: Username not Found";
		}
		else if (response == "PassError")
		{
			Login_ErrorText.text = "Error: Password Incorrect";
		}
		else
		{
			Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
		}
	}

	private IEnumerator RegisterUser()
	{
		IEnumerator e = DCF.RegisterUser(playerUsername, playerPassword, UserDataManager.Instance.GetDefaultUserData());
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Success")
		{
			ResetAllUIElements();
			PlayerPrefs.SetString("username", playerUsername);
			UserDataManager.Instance.OnLogIn(playerUsername, playerPassword);
			PhotonLobbyCustomMatch.lobby.OnLoggedIn();
			yield break;
		}
		LoadingScreenManager.Instance.ResolveLoadingScreen();
		registerParent.gameObject.SetActive(true);
		if (response == "UserError")
		{
			Register_ErrorText.text = "Error: Username Already Taken";
		}
		else
		{
			Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
		}
	}

	public void Login_LoginButtonPressed()
	{
		playerUsername = Login_UsernameField.text;
		playerPassword = Login_PasswordField.text;
		loginParent.gameObject.SetActive(false);
		LoadingScreenManager.Instance.RequestLoadingScreen("Logging in");
		StartCoroutine(LoginUser());
	}

	public void Login_RegisterButtonPressed()
	{
		ResetAllUIElements();
		loginParent.gameObject.SetActive(false);
		registerParent.gameObject.SetActive(true);
	}

	public void Register_RegisterButtonPressed()
	{
		playerUsername = Register_UsernameField.text;
		playerPassword = Register_PasswordField.text;
		registerParent.gameObject.SetActive(false);
		LoadingScreenManager.Instance.RequestLoadingScreen("Registering");
		StartCoroutine(RegisterUser());
	}

	public void Register_BackButtonPressed()
	{
		ResetAllUIElements();
		loginParent.gameObject.SetActive(true);
		registerParent.gameObject.SetActive(false);
	}

	public void LoggedIn_LogoutButtonPressed()
	{
		ResetAllUIElements();
		playerUsername = string.Empty;
		playerPassword = string.Empty;
		loginParent.gameObject.SetActive(true);
		PhotonLobbyCustomMatch.lobby.OnLoggedOut();
	}
}
