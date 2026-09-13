using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStandings : MonoBehaviour
{
	[SerializeField]
	private Text standingsText;

	[SerializeField]
	private VerticalLayoutGroup vertLayoutGroup;

	private int updateDelay;

	public void RenderStandings()
	{
		updateDelay = 2;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<b><color=#ff5e5e>" + UserDataManager.PlayerUsername + "'s Standings</color></b>\n");
		foreach (string key in UserDataManager.Instance.dataProperties.Keys)
		{
			if (!(key == "UNFRIENDLY"))
			{
				stringBuilder.Append("<b>" + key.Replace('_', ' ') + "</b>: " + UserDataManager.Instance.GetProperty(key) + "\n");
			}
		}
		standingsText.text = stringBuilder.ToString();
	}

	private void Update()
	{
		updateDelay--;
		if (updateDelay == 0)
		{
			Canvas.ForceUpdateCanvases();
			vertLayoutGroup.enabled = false;
			vertLayoutGroup.enabled = true;
		}
	}
}
