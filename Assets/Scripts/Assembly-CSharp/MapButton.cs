using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
	[SerializeField]
	private Text nameText;

	[SerializeField]
	private Image image;

	private int mapIndex;

	public void SetMap(string name, Sprite mapImage, int buildIndex)
	{
		mapIndex = buildIndex;
		nameText.text = name;
		image.sprite = mapImage;
	}

	public void SelectMap()
	{
		PhotonLobbyCustomMatch.lobby.SetMap(mapIndex);
	}
}
