using Photon.Pun;
using UnityEngine;

public class FloorItem : MonoBehaviour, IInteractable
{
	public int itemID;

	[Header("Ammo")]
	public bool ammo;

	public AmmoType ammoType;

	public int ammoAmount;

	[HideInInspector]
	public PhotonView PV;

	private bool pickedUp;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		if (PV.InstantiationData != null && ammo)
		{
			ammoAmount = (int)PV.InstantiationData[0];
		}
	}

	public string GetInteractMessage()
	{
		if (GameManager.gameManager.gameMode == GameMode.Infection && GameSetup.gameSetup.player.infected)
		{
			return "You can't pick up items while infected.";
		}
		return string.Format("Press {0} to Pick Up", InputManager.IM.GetKeyNameForButton("Interact"));
	}

	public void Interact()
	{
		if (!pickedUp && (!GameSetup.gameSetup.player.avatarPlayerController.OwnsItem(itemID) || ammo) && (GameManager.gameManager.gameMode != GameMode.Infection || !GameSetup.gameSetup.player.infected))
		{
			pickedUp = true;
			if (ammo)
			{
				GameSetup.gameSetup.player.avatarPlayerController.GainReserveAmmo(ammoType, ammoAmount);
			}
			else
			{
				GameSetup.gameSetup.player.avatarPlayerController.PickupItem(itemID, PV.InstantiationData);
			}
			GameSetup.gameSetup.player.avatarPlayerController.PV.RPC("RPC_DestroyObject", RpcTarget.MasterClient, PV.ViewID);
		}
	}
}
