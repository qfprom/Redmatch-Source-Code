using UnityEngine;

public class AudioSpectrum : MonoBehaviour
{
	public static float spectrumValue;

	public static float[] audioSpectrum;

	private void Start()
	{
		audioSpectrum = new float[128];
	}

	private void Update()
	{
		AudioListener.GetSpectrumData(audioSpectrum, 0, FFTWindow.Hamming);
		if (audioSpectrum != null && audioSpectrum.Length > 0)
		{
			spectrumValue = audioSpectrum[2] * 100f;
		}
	}
}
