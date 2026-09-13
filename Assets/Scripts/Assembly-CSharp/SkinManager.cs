using System;
using UnityEngine;
using UnityEngine.UI;

public class SkinManager : MonoBehaviour
{
	public static SkinManager skinManager;

	[SerializeField]
	private Button skinButton;

	[SerializeField]
	private Text skinText;

	[SerializeField]
	private Text skinTaskText;

	[SerializeField]
	private Transform skinListingContainer;

	[SerializeField]
	private GameObject skinListingPrefab;

	[SerializeField]
	private Renderer skinPreviewRenderer;

	[SerializeField]
	private Skin[] skins;

	[SerializeField]
	private Skin missingSkin;

	private Skin selectedSkin;

	private void Awake()
	{
		if ((bool)skinManager)
		{
			UnityEngine.Object.Destroy(skinManager.gameObject);
		}
		skinManager = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Setup()
	{
		skinButton.interactable = true;
		Array.Sort(skins, (Skin skin1, Skin skin2) => skin1.CompareTo(skin2));
		if (PlayerPrefs.HasKey("SkinManager.selectedSkin"))
		{
			SetSkin(GetSkin(PlayerPrefs.GetString("SkinManager.selectedSkin")));
		}
		else
		{
			SetSkin(skins[0]);
		}
		RenderSkinListings();
	}

	public void SetSkin(Skin skin)
	{
		skinPreviewRenderer.material = skin.skinMat;
		if (string.IsNullOrEmpty(skin.property))
		{
			selectedSkin = skin;
			skinText.text = "Skin: " + skin.name;
			skinTaskText.text = string.Empty;
			Debug.Log("Chose skin: " + skin.name);
		}
		else if (UserDataManager.Instance.GetIntProperty(skin.property) >= skin.propertyMin)
		{
			selectedSkin = skin;
			skinText.text = "Skin: " + skin.name;
			skinTaskText.text = string.Format("You had {0}/{1} of the {2} needed to unlock this skin.", UserDataManager.Instance.GetIntProperty(skin.property), skin.propertyMin, skin.property.Replace('_', ' '));
			Debug.Log("Chose skin: " + skin.name);
		}
		else
		{
			skinText.text = "Locked Skin: " + skin.name;
			skinTaskText.text = string.Format("You have {0}/{1} of the {2} needed to unlock this skin and use it in-game.", UserDataManager.Instance.GetIntProperty(skin.property), skin.propertyMin, skin.property.Replace('_', ' '));
		}
		PlayerPrefs.SetString("SkinManager.selectedSkin", skin.name);
	}

	public Skin GetSkin(string skinName)
	{
		Skin result = missingSkin;
		Skin[] array = skins;
		foreach (Skin skin in array)
		{
			if (skin.name == skinName)
			{
				result = skin;
				break;
			}
		}
		return result;
	}

	public Skin GetCurrentSkin()
	{
		return (selectedSkin != null) ? selectedSkin : missingSkin;
	}

	private void RemoveSkinListings()
	{
		foreach (Transform item in skinListingContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private void RenderSkinListings()
	{
		Skin[] array = skins;
		foreach (Skin skin in array)
		{
			if (!(UnityEngine.Random.Range(0f, 1f) < skin.chance))
			{
				continue;
			}
			if (skin.onlyEditor)
			{
				if (Application.isEditor)
				{
					ListSkin(skin);
				}
			}
			else
			{
				ListSkin(skin);
			}
		}
	}

	private void ListSkin(Skin skin)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(skinListingPrefab, skinListingContainer);
		SkinButton component = gameObject.GetComponent<SkinButton>();
		component.SetSkin(skin);
	}
}
