using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SoundsForJoinAndLeave : MonoBehaviourPunCallbacks
{
	public AudioClip JoinClip;

	public AudioClip LeaveClip;

	private AudioSource source;

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (JoinClip != null)
		{
			if (source == null)
			{
				source = Object.FindObjectOfType<AudioSource>();
			}
			source.PlayOneShot(JoinClip);
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (LeaveClip != null)
		{
			if (source == null)
			{
				source = Object.FindObjectOfType<AudioSource>();
			}
			source.PlayOneShot(LeaveClip);
		}
	}
}
