using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
	[SerializeField]
	private GameObject loadingScreen;

	[SerializeField]
	private Text loadingTitleText;

	[SerializeField]
	private Text loadingFactTitleText;

	[SerializeField]
	private Text loadingFactContentText;

	private TextAsset funfacts;

	private string[] facts;

	[SerializeField]
	private bool menu;

	public static LoadingScreenManager Instance;

	private void Awake()
	{
		Instance = this;
		funfacts = Resources.Load("funfacts") as TextAsset;
		facts = funfacts.text.Split("\n"[0]);
		if (!menu)
		{
			RequestLoadingScreen("Waiting for Players");
		}
	}

	public void RequestLoadingScreen(string loadingTitle)
	{
		string text = facts[Random.Range(0, facts.Length)];
		loadingScreen.SetActive(true);
		loadingTitleText.text = loadingTitle;
		if (text.Split('|').Length == 1)
		{
			loadingFactTitleText.text = text;
			loadingFactContentText.text = string.Empty;
			return;
		}
		loadingFactTitleText.text = text.Split('|')[0];
		loadingFactContentText.text = text.Split('|')[1];
	}

	public void ResolveLoadingScreen()
	{
		loadingScreen.SetActive(false);
	}
}
