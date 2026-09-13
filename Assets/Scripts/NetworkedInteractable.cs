using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class NetworkedInteractable : MonoBehaviour, IInteractable
{
	[SerializeField]
	private string interactMessage = " interact";

	[SerializeField]
	private UnityEvent onInteract;

	private PhotonView PV;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	public void Interact()
	{
		PV.RPC("RPC_NetworkedInteract", RpcTarget.MasterClient);
	}

	[PunRPC]
	private void RPC_NetworkedInteract()
	{
		onInteract.Invoke();
	}

	public string GetInteractMessage()
	{
		return string.Format("Use [{0}] to {1}", InputManager.IM.GetKeyNameForButton("Interact"), interactMessage);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(base.transform.position, 0.5f);
	}
}
