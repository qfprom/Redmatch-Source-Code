using System.Collections;
using DatabaseControl;
using UnityEngine;
using UnityEngine.UI;

public class DCF_DemoScene_ManagerScript_CSharp : MonoBehaviour
{
	public GameObject loginParent;

	public GameObject registerParent;

	public GameObject loggedInParent;

	public GameObject loadingParent;

	public InputField Login_UsernameField;

	public InputField Login_PasswordField;

	public InputField Register_UsernameField;

	public InputField Register_PasswordField;

	public InputField Register_ConfirmPasswordField;

	public InputField LoggedIn_DataInputField;

	public InputField LoggedIn_DataOutputField;

	public Text Login_ErrorText;

	public Text Register_ErrorText;

	public Text LoggedIn_DisplayUsernameText;

	private string playerUsername = string.Empty;

	private string playerPassword = string.Empty;

	private void Awake()
	{
		ResetAllUIElements();
	}

	private void ResetAllUIElements()
	{
		Login_UsernameField.text = string.Empty;
		Login_PasswordField.text = string.Empty;
		Register_UsernameField.text = string.Empty;
		Register_PasswordField.text = string.Empty;
		Register_ConfirmPasswordField.text = string.Empty;
		LoggedIn_DataInputField.text = string.Empty;
		LoggedIn_DataOutputField.text = string.Empty;
		Login_ErrorText.text = string.Empty;
		Register_ErrorText.text = string.Empty;
		LoggedIn_DisplayUsernameText.text = string.Empty;
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
			loadingParent.gameObject.SetActive(false);
			loggedInParent.gameObject.SetActive(true);
			LoggedIn_DisplayUsernameText.text = "Logged In As: " + playerUsername;
			yield break;
		}
		loadingParent.gameObject.SetActive(false);
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
		IEnumerator e = DCF.RegisterUser(playerUsername, playerPassword, "Hello World");
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Success")
		{
			ResetAllUIElements();
			loadingParent.gameObject.SetActive(false);
			loggedInParent.gameObject.SetActive(true);
			LoggedIn_DisplayUsernameText.text = "Logged In As: " + playerUsername;
			yield break;
		}
		loadingParent.gameObject.SetActive(false);
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

	private IEnumerator GetData()
	{
		IEnumerator e = DCF.GetUserData(playerUsername, playerPassword);
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Error")
		{
			ResetAllUIElements();
			playerUsername = string.Empty;
			playerPassword = string.Empty;
			loginParent.gameObject.SetActive(true);
			loadingParent.gameObject.SetActive(false);
			Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
		}
		else
		{
			loadingParent.gameObject.SetActive(false);
			loggedInParent.gameObject.SetActive(true);
			LoggedIn_DataOutputField.text = response;
		}
	}

	private IEnumerator SetData(string data)
	{
		IEnumerator e = DCF.SetUserData(playerUsername, playerPassword, data);
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Success")
		{
			loadingParent.gameObject.SetActive(false);
			loggedInParent.gameObject.SetActive(true);
			yield break;
		}
		ResetAllUIElements();
		playerUsername = string.Empty;
		playerPassword = string.Empty;
		loginParent.gameObject.SetActive(true);
		loadingParent.gameObject.SetActive(false);
		Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
	}

	public void Login_LoginButtonPressed()
	{
		playerUsername = Login_UsernameField.text;
		playerPassword = Login_PasswordField.text;
		if (playerUsername.Length > 3)
		{
			if (playerPassword.Length > 5)
			{
				loginParent.gameObject.SetActive(false);
				loadingParent.gameObject.SetActive(true);
				StartCoroutine(LoginUser());
			}
			else
			{
				Login_ErrorText.text = "Error: Password Incorrect";
			}
		}
		else
		{
			Login_ErrorText.text = "Error: Username Incorrect";
		}
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
		string text = Register_ConfirmPasswordField.text;
		if (playerUsername.Length > 3)
		{
			if (playerPassword.Length > 5)
			{
				if (playerPassword == text)
				{
					registerParent.gameObject.SetActive(false);
					loadingParent.gameObject.SetActive(true);
					StartCoroutine(RegisterUser());
				}
				else
				{
					Register_ErrorText.text = "Error: Password's don't Match";
				}
			}
			else
			{
				Register_ErrorText.text = "Error: Password too Short";
			}
		}
		else
		{
			Register_ErrorText.text = "Error: Username too Short";
		}
	}

	public void Register_BackButtonPressed()
	{
		ResetAllUIElements();
		loginParent.gameObject.SetActive(true);
		registerParent.gameObject.SetActive(false);
	}

	public void LoggedIn_SaveDataButtonPressed()
	{
		loadingParent.gameObject.SetActive(true);
		loggedInParent.gameObject.SetActive(false);
		StartCoroutine(SetData(LoggedIn_DataInputField.text));
	}

	public void LoggedIn_LoadDataButtonPressed()
	{
		loadingParent.gameObject.SetActive(true);
		loggedInParent.gameObject.SetActive(false);
		StartCoroutine(GetData());
	}

	public void LoggedIn_LogoutButtonPressed()
	{
		ResetAllUIElements();
		playerUsername = string.Empty;
		playerPassword = string.Empty;
		loginParent.gameObject.SetActive(true);
		loggedInParent.gameObject.SetActive(false);
	}
}
