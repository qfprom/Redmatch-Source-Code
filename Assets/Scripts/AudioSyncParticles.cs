using System.Collections;
using UnityEngine;

public class AudioSyncParticles : AudioSyncer
{
	[SerializeField]
	private float restSpeed;

	[SerializeField]
	private float beatSpeed;

	private ParticleSystem ps;

	private void Awake()
	{
		ps = GetComponent<ParticleSystem>();
	}

	private IEnumerator MoveToSpeed(float target)
	{
		float current = ps.noise.strengthMultiplier;
		float initial = current;
		float timer = 0f;
		while (current != target)
		{
			current = Mathf.Lerp(initial, target, timer / timeToBeat);
			timer += Time.deltaTime;
			ParticleSystem.NoiseModule noise = ps.noise;
			noise.strengthMultiplier = current;
			yield return null;
		}
		isBeat = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!isBeat)
		{
			ParticleSystem.NoiseModule noise = ps.noise;
			noise.strengthMultiplier = Mathf.Lerp(ps.noise.strengthMultiplier, restSpeed, Time.deltaTime * restSmoothTime);
		}
	}

	public override void OnBeat()
	{
		base.OnBeat();
		StopCoroutine("MoveToSpeed");
		StartCoroutine("MoveToSpeed", beatSpeed);
	}
}
