using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
	[SerializeField]
	private Text nameText;

	[SerializeField]
	private Text sizeText;

	public RoomInfo selectedRoom;

	public void SetRoom(RoomInfo room, int currentSize, int maxSize)
	{
		selectedRoom = room;
		string[] array = selectedRoom.Name.Split('|');
		nameText.text = array[array.Length - 1];
		sizeText.text = currentSize + "/" + maxSize;
	}

	public void JoinRoom()
	{
		PhotonRoomCustomMatch.room.selectedRoom = selectedRoom;
		string[] array = selectedRoom.Name.Split('|');
		if (string.IsNullOrEmpty(array[0]))
		{
			if (PhotonNetwork.InRoom)
			{
				PhotonNetwork.LeaveRoom();
			}
			PhotonNetwork.JoinRoom(selectedRoom.Name);
		}
		else
		{
			PhotonRoomCustomMatch.room.StartPasswordEntry();
		}
	}
}
