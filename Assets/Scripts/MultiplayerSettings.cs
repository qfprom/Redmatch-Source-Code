using UnityEngine;

public class MultiplayerSettings : MonoBehaviour
{
	public static MultiplayerSettings multiplayerSettings;

	public Map[] maps;

	public GameModeInfo[] gameModeInfo;

	public ItemInfo[] gunGameWeapons;

	public bool delayStart;

	public int maxPlayers;

	public int menuScene;

	private void Awake()
	{
		if (multiplayerSettings == null)
		{
			multiplayerSettings = this;
		}
		else if (multiplayerSettings != this)
		{
			Object.Destroy(base.gameObject);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
