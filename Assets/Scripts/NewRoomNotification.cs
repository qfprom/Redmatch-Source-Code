using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NewRoomNotification : MonoBehaviour
{
	[SerializeField]
	private Text roomText;

	[SerializeField]
	private Image lifetimeImage;

	private float currentLifetime;

	[SerializeField]
	private float lifetime;

	private RoomInfo room;

	public void AssignRoom(RoomInfo room)
	{
		this.room = room;
		roomText.text = string.Format("Room \"{0}\" has been created. Join? ({1}/{2})", room.Name.Split('|')[1], room.PlayerCount, room.MaxPlayers);
	}

	public void Join()
	{
		PhotonNetwork.JoinRoom(room.Name);
		Object.Destroy(base.gameObject);
	}

	public void Ignore()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		lifetimeImage.fillAmount = 1f - currentLifetime / lifetime;
		currentLifetime += Time.deltaTime;
		if (currentLifetime >= lifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
