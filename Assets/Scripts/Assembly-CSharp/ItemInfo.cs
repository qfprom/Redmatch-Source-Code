using UnityEngine;

[CreateAssetMenu(fileName = "New ItemInfo", menuName = "ItemInfo")]
public class ItemInfo : ScriptableObject
{
	public ItemType itemType;

	public AmmoType ammoType;

	public int itemID;

	public Sprite icon;

	public string itemName;

	public string floorItemName;

	public int[] animationLayers;

	public float motionSpreadMultiplier;

	public float cameraShakeMultiplier = 1.5f;

	public float cameraShakeRoughness = 1f;

	public bool canThrowAway = true;

	public bool breaksOnUse;

	public bool hideCrosshair;

	public bool hideMiddleCrosshair;

	public string enterTriggerName;

	public string soundName;

	public float damageFalloffStart;

	public float damageFalloff;

	public float damage;

	public float aimedBulletSpread;

	public float aimedSensitivityMultiplier;

	public float hipBulletSpread;

	public float fireRate;

	public int magazineSize;

	public float reloadTime;

	public float recoil;

	public float maxRecoil;

	public float recoilDecrease;

	public FireMode[] fireModes;

	public int shotCount = 1;

	public bool individualReload;

	public float moveSpeedAdditive;

	public float swayAmount = 0.015f;

	public float maxSwayAmount = 0.06f;

	public float aimSwayAmount = 0.005f;

	public float maxAimSwayAmount = 0.015f;

	public float swaySmoothAmount = 8f;
}
