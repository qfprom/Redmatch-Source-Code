using UnityEngine;

public class Gun : MonoBehaviour
{
	public new string name;

	public float aimedBulletSpread;

	public float aimedSensitivityMultiplier;

	public float hipBulletSpread;

	public float damage;

	public float fireRate;

	public int magazineSize;

	public float reloadTime;

	public float recoil;

	public float maxRecoil;

	public float recoilDecrease;

	public int[] animationLayers;

	public bool semiAutomatic;

	public int shotCount = 1;

	public string enterTriggerName;

	public string soundName;

	public bool individualReload;

	public bool holstered;

	public bool canThrowAway = true;

	public bool hideCrosshair;

	public int gunID;

	public string pickupName;

	public float damageFalloffStart;

	public float damageFalloff;

	[HideInInspector]
	public float currentRecoil;

	public ParticleSystem[] tracerParticles;

	public ParticleSystem muzzleFlassParticles;

	[HideInInspector]
	public int ammo;

	[HideInInspector]
	public float nextTimeToFire;

	public GameObject gunObj;

	private void Awake()
	{
		ammo = magazineSize;
	}
}
