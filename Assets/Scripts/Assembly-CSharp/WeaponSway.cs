using Photon.Pun;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
	private PhotonView PV;

	private Vector3 initialPosition;

	private PlayerController player;

	private void Awake()
	{
		player = GetComponentInParent<PlayerController>();
		PV = GetComponentInParent<PhotonView>();
	}

	private void Start()
	{
		initialPosition = base.transform.localPosition;
	}

	private void Update()
	{
		if (!GameMenuManager.menuManager.lockControl && PV.IsMine && player.ownedItems.Count != 0)
		{
			float x = Input.GetAxisRaw("Mouse X") * Time.deltaTime * SettingsManager.settings.sensitivity * ((!player.aiming) ? player.ownedItems[player.itemIndex].info.swayAmount : player.ownedItems[player.itemIndex].info.aimSwayAmount);
			float y = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * SettingsManager.settings.sensitivity * ((!player.aiming) ? player.ownedItems[player.itemIndex].info.swayAmount : player.ownedItems[player.itemIndex].info.aimSwayAmount);
			Vector3 vector = Vector3.ClampMagnitude(new Vector3(x, y), (!player.aiming) ? player.ownedItems[player.itemIndex].info.maxSwayAmount : player.ownedItems[player.itemIndex].info.maxAimSwayAmount);
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, vector + initialPosition, Time.deltaTime * player.ownedItems[player.itemIndex].info.swaySmoothAmount);
		}
	}
}
