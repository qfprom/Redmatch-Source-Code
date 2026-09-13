using Photon.Pun;
using UnityEngine;

public class PlayerBody : MonoBehaviour, IInteractable
{
	[HideInInspector]
	public PhotonView PV;

	public string playerName;

	public TTTTeam tttteam;

	public bool identified;

	private Rigidbody rb;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		playerName = (string)PV.InstantiationData[0];
		tttteam = (TTTTeam)PV.InstantiationData[1];
		GetComponent<Renderer>().material = SkinManager.skinManager.GetSkin((string)PV.InstantiationData[2]).skinMat;
		if (PhotonNetwork.IsMasterClient)
		{
			rb = GetComponent<Rigidbody>();
			rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
		}
	}

	public void Interact()
	{
		if (identified || GameSetup.gameSetup.player.tttteam == TTTTeam.Detective)
		{
			if (!identified)
			{
				PV.RPC("RPC_IdentifyBody", RpcTarget.AllBuffered);
			}
			string text = ((tttteam == TTTTeam.Traitor) ? "<color=red>Traitor</color>" : ((tttteam != TTTTeam.Detective) ? "<color=lime>Innocent</color>" : "<color=blue>Detective</color>"));
			GameSetup.gameSetup.bodyInfoText.text = string.Format("{0}\n{1}\nKilled with: {2}\nFrom: {3} meters away", playerName, text, PV.InstantiationData[3], PV.InstantiationData[4]);
			GameMenuManager.menuManager.RequestMenu("bodyIdentification");
		}
	}

	public string GetInteractMessage()
	{
		if (identified)
		{
			return playerName + "'s body.";
		}
		if (GameSetup.gameSetup.player.tttteam == TTTTeam.Detective)
		{
			return "Unidentified body";
		}
		return "Unidentified body. Ask a detective to identify.";
	}

	[PunRPC]
	public void RPC_IdentifyBody()
	{
		identified = true;
	}
}
