using UnityEngine;

public class C4 : MonoBehaviour, IInteractable
{
	public string GetInteractMessage()
	{
		if (GameSetup.gameSetup.player.tttteam == TTTTeam.Traitor)
		{
			return "Press \"Interact\" to arm";
		}
		return "Press \"Interact\" to disarm";
	}

	public void Interact()
	{
		if (GameSetup.gameSetup.player.tttteam == TTTTeam.Traitor)
		{
			Arm();
		}
		else
		{
			Disarm();
		}
	}

	private void Arm()
	{
	}

	private void Disarm()
	{
	}
}
