using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class CustomNetworkedAnimator : MonoBehaviour
{
	[SerializeField]
	private Animator anim;

	private PhotonView PV;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		if (!PV)
		{
			Debug.LogError("PhotonView missing on CNA " + base.name);
		}
	}

	public void ToggleBool(string boolName)
	{
		PV.RPC("RPC_ToggleBool", RpcTarget.MasterClient, boolName);
	}

	public void SetBoolTrue(string boolName)
	{
		PV.RPC("RPC_SetBoolTrue", RpcTarget.MasterClient, boolName);
	}

	public void SetBoolFalse(string boolName)
	{
		PV.RPC("RPC_SetBoolFalse", RpcTarget.MasterClient, boolName);
	}

	[PunRPC]
	private void RPC_ToggleBool(string boolName)
	{
		PV.RPC("RPC_SetBool", RpcTarget.AllBuffered, boolName, !anim.GetBool(boolName));
	}

	[PunRPC]
	private void RPC_SetBoolTrue(string boolName)
	{
		PV.RPC("RPC_SetBool", RpcTarget.AllBuffered, boolName, true);
	}

	[PunRPC]
	private void RPC_SetBoolFalse(string boolName)
	{
		PV.RPC("RPC_SetBool", RpcTarget.AllBuffered, boolName, false);
	}

	[PunRPC]
	private void RPC_SetBool(string boolName, bool value)
	{
		anim.SetBool(boolName, value);
	}
}
