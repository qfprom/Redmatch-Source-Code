using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class CustomMultiNetworkedAnimator : MonoBehaviour
{
	[SerializeField]
	private Animator[] anims;

	private PhotonView PV;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		if (!PV)
		{
			Debug.LogError("PhotonView missing on CMNA " + base.name);
		}
	}

	public void ToggleBool(string boolName)
	{
		PV.RPC("RPC_ToggleBool", RpcTarget.MasterClient, boolName);
	}

	[PunRPC]
	private void RPC_ToggleBool(string boolName)
	{
		PV.RPC("RPC_SetBool", RpcTarget.AllBuffered, boolName, !anims[0].GetBool(boolName));
	}

	[PunRPC]
	private void RPC_SetBool(string boolName, bool value)
	{
		Animator[] array = anims;
		foreach (Animator animator in array)
		{
			animator.SetBool(boolName, value);
		}
	}
}
