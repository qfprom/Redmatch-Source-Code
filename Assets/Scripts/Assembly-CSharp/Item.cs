using UnityEngine;

public class Item : MonoBehaviour
{
	[Header("-= Universal =-")]
	public GameObject itemObj;

	public ItemInfo info;

	[Header("Gun")]
	public ParticleSystem[] tracerParticles;

	public ParticleSystem muzzleFlassParticles;

	[HideInInspector]
	public int fireModeIndex;

	[HideInInspector]
	public float currentRecoil;

	[HideInInspector]
	public int ammo;

	[HideInInspector]
	public float nextTimeToFire;

	[HideInInspector]
	public float nextTimeToBurstFire;

	private void Awake()
	{
		if (info.itemType == ItemType.Gun)
		{
			ammo = info.magazineSize;
		}
	}
}
