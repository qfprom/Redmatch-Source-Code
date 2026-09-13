using System;

[Serializable]
public struct GameModeInfo
{
	public GameMode gameMode;

	public bool elimination;

	public bool onPlayerDeath;

	public bool canJoinAfterStart;

	public bool floorItems;

	public bool startWithAllItems;

	public bool dropItemsOnDeath;

	public bool bodyOnDeath;

	public bool sendGameInfo;

	public bool naturalHealthRegen;

	public bool noDamageOutOfTime;

	public bool infiniteAmmo;

	public float respawnTime;
}
