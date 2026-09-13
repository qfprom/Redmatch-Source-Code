using UnityEngine;

public class MusicManager : MonoBehaviour
{
	[SerializeField]
	private AudioSource mainMusic;

	[SerializeField]
	private AudioSource lowPassMusic;

	private float mainMusicStartingVolume;

	private float lowPassMusicStartingVolume;

	private void Start()
	{
		mainMusicStartingVolume = mainMusic.volume;
		lowPassMusicStartingVolume = lowPassMusic.volume;
		SwitchToLowPass();
	}

	public void SwitchToMain()
	{
		mainMusic.volume = mainMusicStartingVolume;
		lowPassMusic.volume = 0f;
	}

	public void SwitchToLowPass()
	{
		lowPassMusic.volume = lowPassMusicStartingVolume;
		mainMusic.volume = 0f;
	}
}
