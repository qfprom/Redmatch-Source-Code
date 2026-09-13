using System;
using UnityEngine;

[Serializable]
public class CustomKey
{
	public string altName;

	public KeyCode keyCode;

	public string axis;

	public float axisMin;

	public float axisMax;

	public bool downLastFrame;

	public CustomKey(KeyCode code)
	{
		keyCode = code;
	}
}
