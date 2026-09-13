using UnityEngine;

public class RoomData : MonoBehaviour
{
	public static RoomData data;

	public GameMode gameMode;

	public string roomPassword = string.Empty;

	public string roomName = "Unnamed";

	public int roomSize = 10;

	public int map = 1;

	private void Awake()
	{
		if ((bool)data)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		data = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
