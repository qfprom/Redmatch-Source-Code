using UnityEngine;
using UnityEngine.UI;

public class KeybindListItem : MonoBehaviour
{
	[SerializeField]
	private Text buttonKeyNameText;

	[SerializeField]
	private Image buttonImage;

	public void AssignKey(CustomKey ck)
	{
		CustomKeyImage[] specialKeyImages = InputManager.IM.specialKeyImages;
		foreach (CustomKeyImage customKeyImage in specialKeyImages)
		{
			if (customKeyImage.keyCode == KeyCode.None)
			{
				if (customKeyImage.altName == ck.altName)
				{
					buttonKeyNameText.gameObject.SetActive(false);
					buttonImage.gameObject.SetActive(true);
					buttonImage.sprite = customKeyImage.keySprite;
					return;
				}
			}
			else if (customKeyImage.keyCode == ck.keyCode)
			{
				buttonKeyNameText.gameObject.SetActive(false);
				buttonImage.gameObject.SetActive(true);
				buttonImage.sprite = customKeyImage.keySprite;
				return;
			}
		}
		buttonImage.gameObject.SetActive(false);
		buttonKeyNameText.gameObject.SetActive(true);
		buttonKeyNameText.text = ck.keyCode.ToString();
		buttonKeyNameText.color = ((ck.keyCode != KeyCode.None) ? new Color(0.196f, 0.196f, 0.196f) : Color.red);
	}
}
