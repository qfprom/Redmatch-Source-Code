using UnityEngine;

public class SpectatorController : MonoBehaviour
{
	[SerializeField]
	private Transform playerCameraHolder;

	[SerializeField]
	private Camera playerCamera;

	[SerializeField]
	private LayerMask rayMask;

	private float verticalLookRotation;

	private Vector3 moveAmount;

	private bool following;

	private Transform followingTransform;

	private bool interactPrompting;

	private void Start()
	{
		GameSetup.gameSetup.bodyIdentificationPanel.SetActive(false);
	}

	private void Update()
	{
		if (following)
		{
			if ((bool)followingTransform)
			{
				base.transform.position = followingTransform.position;
				base.transform.rotation = followingTransform.rotation;
			}
			else
			{
				StopFollowing();
			}
			if (InputManager.IM.GetButtonDown("Interact"))
			{
				StopFollowing();
			}
		}
		else
		{
			if (GameMenuManager.menuManager.lockControl)
			{
				return;
			}
			bool flag = false;
			Ray ray = playerCamera.ViewportPointToRay((Vector3.right + Vector3.up) * 0.5f);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, 2f, rayMask) && hitInfo.collider.CompareTag("Player"))
			{
				flag = true;
				if (!interactPrompting)
				{
					interactPrompting = true;
					GameSetup.gameSetup.canvasAnim.SetBool("interacting", interactPrompting);
				}
				GameSetup.gameSetup.interactText.text = string.Format("Press [{0}] to spectate", InputManager.IM.GetKeyNameForButton("Interact"));
				if (InputManager.IM.GetButtonDown("Interact"))
				{
					StartFollowing(hitInfo.collider.GetComponentInParent<PlayerController>().playerCameraHolder.transform);
				}
			}
			if ((!flag || following) && interactPrompting)
			{
				interactPrompting = false;
				GameSetup.gameSetup.canvasAnim.SetBool("interacting", interactPrompting);
			}
			base.transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * SettingsManager.settings.sensitivity * Time.deltaTime);
			verticalLookRotation += Input.GetAxisRaw("Mouse Y") * SettingsManager.settings.sensitivity * Time.deltaTime;
			verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
			playerCameraHolder.localEulerAngles = Vector3.left * verticalLookRotation;
			Vector3 normalized = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
			moveAmount = normalized * ((!InputManager.IM.GetButton("Sprint")) ? 10f : 16f);
		}
	}

	private void FixedUpdate()
	{
		if (!following)
		{
			base.transform.position += playerCameraHolder.TransformDirection(moveAmount * Time.fixedDeltaTime);
		}
	}

	private void StartFollowing(Transform transformToFollow)
	{
		following = true;
		verticalLookRotation = 0f;
		playerCameraHolder.localEulerAngles = Vector3.zero;
		followingTransform = transformToFollow;
		GameSetup.gameSetup.interactText.gameObject.SetActive(false);
	}

	private void StopFollowing()
	{
		following = false;
		followingTransform = null;
		base.transform.rotation = Quaternion.identity;
		verticalLookRotation = 0f;
	}
}
