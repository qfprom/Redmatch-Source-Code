using System;
using UnityEngine;

[Serializable]
public class Skin
{
	public string name;

	public Sprite icon;

	public Material skinMat;

	public bool onlyEditor;

	public string property;

	public int propertyMin;

	public float chance = 1f;

	public int CompareTo(Skin other)
	{
		if (string.IsNullOrEmpty(property) && string.IsNullOrEmpty(other.property))
		{
			return name.CompareTo(other.name);
		}
		if (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(other.property))
		{
			if (UserDataManager.Instance.GetIntProperty(property) >= propertyMin && UserDataManager.Instance.GetIntProperty(other.property) <= other.propertyMin)
			{
				return -1;
			}
			return propertyMin.CompareTo(other.propertyMin);
		}
		if (string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(other.property))
		{
			return -1;
		}
		if (!string.IsNullOrEmpty(property) && string.IsNullOrEmpty(other.property))
		{
			return 1;
		}
		return 0;
	}
}
