using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DatabaseControl;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
	public delegate void OnDataSavedCallback();

	public static UserDataManager Instance;

	private float nextTimeToSaveData = 100000f;

	private float dataSaveInterval = 20f;

	private string userData = string.Empty;

	public Dictionary<string, string> dataProperties = new Dictionary<string, string>
	{
		{ "SKIRMISH_GAMES_PLAYED", "0" },
		{ "SKIRMISH_HIGHEST_KILLSTREAK", "0" },
		{ "SKIRMISH_KILLS", "0" },
		{ "SKIRMISH_DEATHS", "0" },
		{ "TTT_GAMES_PLAYED", "0" },
		{ "TTT_TRAITOR_GOOD_KILLS", "0" },
		{ "TTT_TRAITOR_BAD_KILLS", "0" },
		{ "TTT_TRAITOR_DEATHS", "0" },
		{ "TTT_INNOCENT_GOOD_KILLS", "0" },
		{ "TTT_INNOCENT_BAD_KILLS", "0" },
		{ "TTT_INNOCENT_DEATHS", "0" },
		{ "GUNGAME_GAMES_PLAYED", "0" },
		{ "GUNGAME_WINS", "0" },
		{ "GUNGAME_HIGHEST_KILLSTREAK", "0" },
		{ "GUNGAME_KILLS", "0" },
		{ "GUNGAME_DEATHS", "0" },
		{ "INFECTION_GAMES_PLAYED", "0" },
		{ "INFECTION_AS_INFECTED_KILLS", "0" },
		{ "INFECTION_AS_INFECTED_DEATHS", "0" },
		{ "INFECTION_AS_CLEAN_KILLS", "0" },
		{ "INFECTION_AS_CLEAN_DEATHS", "0" },
		{ "BATTLEROYALE_GAMES_PLAYED", "0" },
		{ "BATTLEROYALE_WINS", "0" },
		{ "BATTLEROYALE_HIGHEST_KILLSTREAK", "0" },
		{ "BATTLEROYALE_KILLS", "0" },
		{ "BATTLEROYALE_DEATHS", "0" },
		{ "JUMPS", "0" },
		{ "SHOTS_FIRED", "0" },
		{ "ENVIRONMENT_DEATHS", "0" },
		{ "UNFRIENDLY", "0" }
	};

	public static string PlayerUsername { get; protected set; }

	public static string PlayerPassword { get; protected set; }

	public static bool IsLoggedIn { get; protected set; }

	public static bool IsDataReady { get; protected set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public string GetDefaultUserData()
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, string> dataProperty in dataProperties)
		{
			string text2 = text;
			text = text2 + "[" + dataProperty.Key + "]" + dataProperty.Value + "/";
		}
		return text;
	}

	public string[] GetDataPropertyNames()
	{
		return dataProperties.Keys.ToArray();
	}

	public void IncrementProperty(string propertyName)
	{
		if (!IsDataReady)
		{
			Debug.LogWarning("Data save requested when not ready");
		}
		else
		{
			SetProperty(propertyName, int.Parse(GetProperty(propertyName)) + 1);
		}
	}

	public void ChangeProperty(string propertyName, int amount)
	{
		if (!IsDataReady)
		{
			Debug.LogWarning("Data save requested when not ready");
		}
		else
		{
			SetProperty(propertyName, int.Parse(GetProperty(propertyName)) + amount);
		}
	}

	public void SetProperty(string propertyName, string propertyValue)
	{
		if (!IsDataReady)
		{
			Debug.LogWarning("Data save requested when not ready");
			return;
		}
		string[] array = userData.Split('/');
		string text = string.Format("[{0}]", propertyName);
		bool flag = false;
		int num = 0;
		string text2 = string.Empty;
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (text3.StartsWith(text))
			{
				text2 = string.Format("[{0}]{1}", propertyName, propertyValue);
				flag = true;
				break;
			}
			num++;
		}
		if (!flag)
		{
			Debug.LogWarning("No property found with identifier " + text);
			userData += string.Format("/[{0}]{1}", propertyName, propertyValue);
		}
		else
		{
			array[num] = text2;
			string text4 = string.Empty;
			string[] array3 = array;
			foreach (string text5 in array3)
			{
				if (!string.IsNullOrEmpty(text5))
				{
					text4 = text4 + text5 + "/";
				}
			}
			userData = text4;
		}
		if (LeaderboardManager.Instance.leaderboardProperties.Contains(propertyName))
		{
			LeaderboardManager.Instance.AddNewHighscore(propertyName, int.Parse(propertyValue));
		}
	}

	public void SetProperty(string propertyName, int propertyValue)
	{
		SetProperty(propertyName, propertyValue.ToString());
	}

	public void SetData(OnDataSavedCallback onDataSaved)
	{
		if (!IsDataReady)
		{
			Debug.LogWarning("Data save requested when not ready");
		}
		else
		{
			StartCoroutine(SaveData(onDataSaved));
		}
	}

	public void SetData()
	{
		if (!IsDataReady)
		{
			Debug.LogWarning("Data save requested when not ready");
		}
		else
		{
			StartCoroutine(SaveData());
		}
	}

	private IEnumerator SaveData(OnDataSavedCallback onDataSaved)
	{
		IEnumerator e = DCF.SetUserData(PlayerUsername, PlayerPassword, userData);
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Success")
		{
			Debug.Log("Successfully set data " + userData);
			onDataSaved();
		}
		else
		{
			Debug.LogError("Error with sending data");
		}
	}

	private IEnumerator SaveData()
	{
		IEnumerator e = DCF.SetUserData(PlayerUsername, PlayerPassword, userData);
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response != "Success")
		{
			Debug.LogError("Error with sending data");
		}
	}

	public string GetProperty(string propertyName)
	{
		if (!IsDataReady)
		{
			Debug.LogWarning("Data requested when not ready");
			return string.Empty;
		}
		string[] array = userData.Split('/');
		string text = string.Format("[{0}]", propertyName);
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (text2.StartsWith(text))
			{
				return text2.Substring(text.Length);
			}
		}
		Debug.LogWarning("No property found with identifier " + text);
		userData += string.Format("/[{0}]{1}", propertyName, dataProperties[propertyName]);
		return dataProperties[propertyName];
	}

	public int GetIntProperty(string propertyName)
	{
		int result = 0;
		if (int.TryParse(GetProperty(propertyName), out result))
		{
			return result;
		}
		Debug.LogError("Was unable to parse the property value of " + propertyName + " with value " + GetProperty(propertyName));
		return int.Parse(dataProperties[propertyName]);
	}

	private IEnumerator LoadData()
	{
		IEnumerator e = DCF.GetUserData(PlayerUsername, PlayerPassword);
		while (e.MoveNext())
		{
			yield return e.Current;
		}
		string response = e.Current as string;
		if (response == "Error")
		{
			Debug.Log("Error with getting data");
		}
		else
		{
			userData = response;
			IsDataReady = true;
		}
		if ((bool)PhotonLobbyCustomMatch.lobby)
		{
			PhotonLobbyCustomMatch.lobby.OnDataLoaded();
		}
		nextTimeToSaveData = Time.unscaledTime + dataSaveInterval;
	}

	public void OnLogIn(string username, string password)
	{
		PlayerUsername = username;
		PlayerPassword = password;
		IsLoggedIn = true;
		StartCoroutine(LoadData());
	}

	public void OnLogOut()
	{
		PlayerUsername = string.Empty;
		PlayerPassword = string.Empty;
		IsLoggedIn = false;
	}

	private void Update()
	{
		if (Time.unscaledTime > nextTimeToSaveData && IsDataReady)
		{
			nextTimeToSaveData = Time.unscaledTime + dataSaveInterval;
			SetData();
		}
	}

	private void OnApplicationQuit()
	{
		if (IsLoggedIn)
		{
			SetData();
		}
	}
}
