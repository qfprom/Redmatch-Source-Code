using UnityEngine;
using UnityEngine.UI;

public class AudioSyncBars : AudioSyncer
{
	private Image[] bars;

	private void Awake()
	{
		bars = base.transform.GetComponentsInChildren<Image>();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		int num = 0;
		Image[] array = bars;
		foreach (Image image in array)
		{
			image.fillAmount = Mathf.Lerp(image.fillAmount, AudioSpectrum.audioSpectrum[num] * 20f, Time.deltaTime * 5f);
			num++;
		}
	}
}
