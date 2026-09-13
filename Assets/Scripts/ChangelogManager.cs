using UnityEngine;
using UnityEngine.UI;

public class ChangelogManager : MonoBehaviour
{
	[SerializeField]
	private Transform changelogContent;

	[SerializeField]
	private GameObject changelogEntryPrefab;

	private TextAsset changelogTxt;

	private void Start()
	{
		changelogTxt = Resources.Load("changelog") as TextAsset;
		RenderChangelog();
	}

	private void RenderChangelog()
	{
		string[] array = changelogTxt.text.Split("\n"[0]);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text[0] == '|')
			{
				Object.Instantiate(changelogEntryPrefab, changelogContent).GetComponent<Text>().text = "<color=#ff5e5e>" + text.Split('|')[1] + "</color>";
			}
			else
			{
				Object.Instantiate(changelogEntryPrefab, changelogContent).GetComponent<Text>().text = "- " + text;
			}
		}
	}
}
