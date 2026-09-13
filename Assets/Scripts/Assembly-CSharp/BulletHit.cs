using UnityEngine;

public class BulletHit : MonoBehaviour
{
	[SerializeField]
	private LayerMask colliderMask;

	[SerializeField]
	private GameObject bulletDecal;

	[SerializeField]
	private ParticleSystem[] defaultParticleSystems;

	[SerializeField]
	private ParticleSystem[] playerParticleSystems;

	private void Start()
	{
		Collider[] array = Physics.OverlapBox(base.transform.position, Vector3.one * 0.05f, base.transform.rotation, colliderMask);
		if (array.Length > 0)
		{
			if (array[0].CompareTag("Player"))
			{
				bulletDecal.SetActive(false);
				if (SettingsManager.settings.showGore)
				{
					ParticleSystem[] array2 = playerParticleSystems;
					foreach (ParticleSystem particleSystem in array2)
					{
						particleSystem.Play();
					}
				}
				else
				{
					ParticleSystem[] array3 = defaultParticleSystems;
					foreach (ParticleSystem particleSystem2 in array3)
					{
						particleSystem2.Play();
					}
				}
			}
			else
			{
				base.transform.SetParent(array[0].transform);
				ParticleSystem[] array4 = defaultParticleSystems;
				foreach (ParticleSystem particleSystem3 in array4)
				{
					particleSystem3.Play();
				}
			}
		}
		else
		{
			bulletDecal.SetActive(false);
		}
	}
}
