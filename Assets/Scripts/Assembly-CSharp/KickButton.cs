using Photon.Realtime;
using UnityEngine;

public class KickButton : MonoBehaviour
{
	public Player targetPlayer;

	public void KickPlayer()
	{
		GameManager.gameManager.KickPlayer(targetPlayer);
	}
}
