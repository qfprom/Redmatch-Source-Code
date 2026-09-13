using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
	private Transform cam;

	private void Start()
	{
		cam = Camera.main.transform;
	}

	private void LateUpdate()
	{
		if (cam == null || !cam.gameObject.activeSelf)
		{
			Camera main = Camera.main;
			if (!(main != null))
			{
				return;
			}
			cam = main.transform;
		}
		base.transform.LookAt(base.transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
	}
}
