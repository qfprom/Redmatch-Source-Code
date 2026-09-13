using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;

	private void Awake()
	{
		AudioMixer audioMixer = Resources.Load("MasterMixer") as AudioMixer;
		Sound[] array = sounds;
		foreach (Sound sound in array)
		{
			sound.source = base.gameObject.AddComponent<AudioSource>();
			sound.source.outputAudioMixerGroup = ((!sound.group) ? audioMixer.FindMatchingGroups("Master")[0] : sound.group);
			sound.source.volume = sound.volume;
			sound.source.maxDistance = sound.maxDistance;
			sound.source.minDistance = sound.minDistance;
			sound.source.spatialBlend = 1f;
			sound.source.rolloffMode = AudioRolloffMode.Linear;
			sound.source.dopplerLevel = 0f;
			sound.source.loop = sound.loop;
			sound.source.clip = sound.clips[UnityEngine.Random.Range(0, sound.clips.Length)];
			if (sound.playOnAwake)
			{
				sound.source.Play();
			}
		}
	}

	public void Play(string name)
	{
		Sound sound = Array.Find(sounds, (Sound sound2) => sound2.name == name);
		if (sound == null)
		{
			Debug.LogError("AudioManager: Sound with name " + name + "not found");
			return;
		}
		sound.source.clip = sound.clips[UnityEngine.Random.Range(0, sound.clips.Length)];
		sound.source.pitch = UnityEngine.Random.Range(sound.minPitch, sound.maxPitch);
		sound.source.Play();
	}

	public void Stop(string name)
	{
		Sound sound = Array.Find(sounds, (Sound sound2) => sound2.name == name);
		if (sound == null)
		{
			Debug.LogError("AudioManager: Sound with name " + name + "not found");
		}
		else
		{
			sound.source.Stop();
		}
	}
}
