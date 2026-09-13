using Photon.Pun;
using UnityEngine;

public class NetworkedRigidbody : MonoBehaviour
{
	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			Object.Destroy(rb);
		}
	}
}
