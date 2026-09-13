using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun
{
	public class DefaultPool : IPunPrefabPool
	{
		public readonly Dictionary<string, GameObject> ResourceCache = new Dictionary<string, GameObject>();

		public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
		{
			GameObject value = null;
			if (!ResourceCache.TryGetValue(prefabId, out value))
			{
				value = (GameObject)Resources.Load(prefabId, typeof(GameObject));
				if (value == null)
				{
					Debug.LogError("DefaultPool failed to load \"" + prefabId + "\" . Make sure it's in a \"Resources\" folder.");
				}
				else
				{
					ResourceCache.Add(prefabId, value);
				}
			}
			bool activeSelf = value.activeSelf;
			if (activeSelf)
			{
				value.SetActive(false);
			}
			GameObject result = Object.Instantiate(value, position, rotation);
			if (activeSelf)
			{
				value.SetActive(true);
			}
			return result;
		}

		public void Destroy(GameObject gameObject)
		{
			Object.Destroy(gameObject);
		}
	}
}
