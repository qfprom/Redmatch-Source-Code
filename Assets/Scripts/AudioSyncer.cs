using UnityEngine;

public class AudioSyncer : MonoBehaviour
{
	public float bias;

	public float timeStep;

	public float timeToBeat;

	public float restSmoothTime;

	private float previousAudioValue;

	private float audioValue;

	private float timer;

	protected bool isBeat;

	public virtual void OnBeat()
	{
		timer = 0f;
		isBeat = true;
	}

	public virtual void OnUpdate()
	{
		previousAudioValue = audioValue;
		audioValue = AudioSpectrum.spectrumValue;
		if (previousAudioValue > bias && audioValue < bias && timer > timeStep)
		{
			OnBeat();
		}
		if (previousAudioValue <= bias && audioValue >= bias && timer > timeStep)
		{
			OnBeat();
		}
		timer += Time.deltaTime;
	}

	private void Update()
	{
		OnUpdate();
	}
}
