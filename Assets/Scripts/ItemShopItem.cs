using System;

[Serializable]
public class ItemShopItem
{
	public string description;

	public TTTTeam[] availableTeams;

	public int pointCost = 1;

	public ItemInfo item;
}
