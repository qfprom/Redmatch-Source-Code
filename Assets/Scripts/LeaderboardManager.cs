using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
	public string[] leaderboardProperties = new string[3] { "SKIRMISH_KILLS", "GUNGAME_WINS", "JUMPS" };

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
		foreach (string property in leaderboardProperties)
		{
			ClearDisplayedHighscores(property);
		}
	}

	public void SubmitHighscore(string propertyName, int value)
	{
		AddNewHighscore(propertyName.Replace('*', '_'), value);
	}

	private void ClearDisplayedHighscores(string propertyName)
	{
		GetHighscoreText(propertyName).text = "Leaderboards Disabled";
	}

	public void AddNewHighscore(string property, int score)
	{
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
	}
}
