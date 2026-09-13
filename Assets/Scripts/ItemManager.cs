using UnityEngine;

public class ItemManager : MonoBehaviour
{
	public static ItemManager itemManager;

	public ItemInfo[] allItems;

	private void Awake()
	{
		itemManager = this;
	}

	public ItemInfo GetItemByID(int itemID)
	{
		ItemInfo[] array = allItems;
		foreach (ItemInfo itemInfo in array)
		{
			if (itemInfo.itemID == itemID)
			{
				return itemInfo;
			}
		}
		Debug.LogError("Item with ID " + itemID + " not found.");
		return allItems[0];
	}
}
