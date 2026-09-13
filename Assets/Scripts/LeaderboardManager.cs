using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
	private const string webURL = "http://dreamlo.com/lb/";

	public string[] leaderboardProperties = new string[3] { "SKIRMISH_KILLS", "GUNGAME_WINS", "JUMPS" };

	private string[] leaderboardPrivateCodes = new string[3] { "", "", "" };

	private string[] leaderboardPublicCodes = new string[3] { "5cae783e3eba5e041cd6f76b", "5cae79a33eba5e041cd6fb15", "5cae7a063eba5e041cd6fc1f" };

	[SerializeField]
	private Text[] highscoreTexts;

	public static LeaderboardManager Instance;

	private void Awake()
	{
		if ((bool)Instance)
		{
			UnityEngine.Object.Destroy(Instance.gameObject);
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
		StartCoroutine("RefreshHighscores");
	}

	public void SubmitHighscore(string propertyName, int value)
	{
		AddNewHighscore(propertyName.Replace('*', '_'), value);
	}

	private void OnHighscoresDownloaded(string downloadedHighscores, string propertyName)
	{
		if (!GameSetup.gameSetup)
		{
			ClearDisplayedHighscores(propertyName);
			GetHighscoreText(propertyName).text = "<b><color=#ff5e5e>Global Standings: " + propertyName.Replace('_', ' ') + "</color></b>\n" + downloadedHighscores;
		}
	}

	private void ClearDisplayedHighscores(string propertyName)
	{
		GetHighscoreText(propertyName).text = "Loading...";
	}

	private IEnumerator RefreshHighscores()
	{
		while (true)
		{
			if (!GameSetup.gameSetup)
			{
				string[] array = leaderboardProperties;
				foreach (string property in array)
				{
					DownloadHighscores(property);
				}
			}
			yield return new WaitForSecondsRealtime(30f);
		}
	}

	public void AddNewHighscore(string property, int score)
	{
	}

	private IEnumerator UploadNewHighscore(string property, int score)
	{
		WWW www = new WWW("http://dreamlo.com/lb/" + GetPrivateCode(property) + "/add/" + WWW.EscapeURL(UserDataManager.PlayerUsername) + "/" + score);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("Error Uploading: " + www.error + ". Check to make sure you are online!");
		}
	}

	private string GetPrivateCode(string property)
	{
		return string.Empty;
	}

	private string GetPublicCode(string property)
	{
		int num = 0;
		string[] array = leaderboardProperties;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == property)
			{
				return leaderboardPublicCodes[num];
			}
			num++;
		}
		Debug.LogError("No leaderboard public code found with property " + property);
		return string.Empty;
	}

	private Text GetHighscoreText(string property)
	{
		int num = 0;
		string[] array = leaderboardProperties;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == property)
			{
				return highscoreTexts[num];
			}
			num++;
		}
		Debug.LogError("No leaderboard container found with property " + property);
		return highscoreTexts[0];
	}

	public void DownloadHighscores(string property)
	{
		StartCoroutine("DownloadHighscoresFromDatabase", property);
	}

	private IEnumerator DownloadHighscoresFromDatabase(string propertyName)
	{
		WWW www = new WWW("http://dreamlo.com/lb/" + GetPublicCode(propertyName) + "/pipe/");
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			OnHighscoresDownloaded(FormatHighscores(www.text), propertyName);
		}
		else
		{
			MonoBehaviour.print("Error Downloading: " + www.error);
		}
	}

	private string FormatHighscores(string textStream)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = textStream.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('|');
			string text = array2[0];
			int num = int.Parse(array2[1]);
			if (text == UserDataManager.PlayerUsername)
			{
				stringBuilder.Append("<color=yellow>" + (i + 1) + ". <b>" + text + "</b>: " + num + "</color>\n");
			}
			else
			{
				stringBuilder.Append(i + 1 + ". <b>" + text + "</b>: " + num + "\n");
			}
		}
		return stringBuilder.ToString();
	}
}
