using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
	public PlayerController playerController;

	private void OnTriggerEnter(Collider col)
	{
		playerController.grounded = true;
	}

	private void OnTriggerExit(Collider col)
	{
		playerController.grounded = false;
	}

	private void OnTriggerStay(Collider col)
	{
		playerController.grounded = true;
	}

	private void OnCollisionEnter(Collision col)
	{
		playerController.grounded = true;
	}

	private void OnCollisionExit(Collision col)
	{
		playerController.grounded = false;
	}

	private void OnCollisionStay(Collision col)
	{
		playerController.grounded = true;
	}
}
