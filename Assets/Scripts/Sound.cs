using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Sound
{
	public string name;

	public AudioClip[] clips;

	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(0f, 2f)]
	public float minPitch = 1f;

	[Range(0f, 2f)]
	public float maxPitch = 1f;

	public float minDistance = 5f;

	public float maxDistance = 40f;

	public bool playOnAwake;

	public bool loop;

	public AudioMixerGroup group;

	[HideInInspector]
	public AudioSource source;
}
