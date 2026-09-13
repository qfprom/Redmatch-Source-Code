using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemShop : MonoBehaviour
{
	public static ItemShop shop;

	[SerializeField]
	private ItemShopItem[] shopItems;

	private int itemIndex;

	private int points = 2;

	private void Awake()
	{
		shop = this;
	}

	private void Start()
	{
		UpdateUI();
	}

	public void AddPoints(int _points)
	{
		points += _points;
		UpdateUI();
	}

	public void UpdateUI()
	{
		GameSetup.gameSetup.shopPointCountText.text = "Points: <color=orange>" + points + "</color>";
		GameSetup.gameSetup.shopTitleText.text = shopItems[itemIndex].item.itemName;
		GameSetup.gameSetup.shopDescriptionText.text = shopItems[itemIndex].description;
		GameSetup.gameSetup.shopCostText.text = "<color=orange>Cost: " + shopItems[itemIndex].pointCost + "</color>";
		RemoveShopListings();
		RenderShopListings();
	}

	private void RemoveShopListings()
	{
		foreach (Transform item in GameSetup.gameSetup.shopContent)
		{
			Object.Destroy(item.gameObject);
		}
	}

	private void RenderShopListings()
	{
		if (!GameSetup.gameSetup.player)
		{
			return;
		}
		for (int i = 0; i < shopItems.Length; i++)
		{
			if (shopItems[i].availableTeams.Contains(GameSetup.gameSetup.player.tttteam))
			{
				int s = i;
				GameObject gameObject = Object.Instantiate(GameSetup.gameSetup.shopListingPrefab, GameSetup.gameSetup.shopContent);
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					SelectShopItem(s);
				});
				Image component = gameObject.GetComponent<Image>();
				component.sprite = shopItems[i].item.icon;
				component.color = ((points < shopItems[i].pointCost) ? ((Color.red + Color.white) / 2f) : Color.white);
			}
		}
	}

	public void BuySelectedItem()
	{
		BuyItem(shopItems[itemIndex]);
	}

	public void SelectShopItem(int index)
	{
		itemIndex = index;
		UpdateUI();
	}

	private void BuyItem(ItemShopItem item)
	{
		if (points >= item.pointCost)
		{
			if ((bool)GameSetup.gameSetup.player.avatarPlayerController)
			{
				if (GameSetup.gameSetup.player.avatarPlayerController.OwnsItem(item.item.itemID))
				{
					GameSetup.gameSetup.SendGameInfo("<color=red>You already own this item.</color>");
				}
				else
				{
					GameSetup.gameSetup.player.avatarPlayerController.PickupItem(item.item.itemID);
					points -= item.pointCost;
				}
			}
		}
		else
		{
			GameSetup.gameSetup.SendGameInfo("<color=red>Not enough points.</color>");
		}
		UpdateUI();
	}
}
