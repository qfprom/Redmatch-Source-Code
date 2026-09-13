using UnityEngine;
using UnityEngine.UI;

public class SkinButton : MonoBehaviour
{
	[SerializeField]
	private Text nameText;

	[SerializeField]
	private Image skinImage;

	[SerializeField]
	private GameObject lockImage;

	private Skin skin;

	public void SetSkin(Skin setSkin)
	{
		skin = setSkin;
		nameText.text = setSkin.name;
		if ((bool)setSkin.icon)
		{
			skinImage.sprite = setSkin.icon;
		}
		else
		{
			skinImage.color = setSkin.skinMat.color;
		}
		if (!string.IsNullOrEmpty(setSkin.property) && UserDataManager.Instance.GetIntProperty(setSkin.property) < setSkin.propertyMin)
		{
			lockImage.SetActive(true);
		}
	}

	public void SelectSkin()
	{
		SkinManager.skinManager.SetSkin(skin);
	}
}
