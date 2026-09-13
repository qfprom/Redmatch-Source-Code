using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Photon.Pun
{
	public static class PunExtensions
	{
		public static Dictionary<MethodInfo, ParameterInfo[]> ParametersOfMethods = new Dictionary<MethodInfo, ParameterInfo[]>();

		public static ParameterInfo[] GetCachedParemeters(this MethodInfo mo)
		{
			ParameterInfo[] value;
			if (!ParametersOfMethods.TryGetValue(mo, out value))
			{
				value = mo.GetParameters();
				ParametersOfMethods[mo] = value;
			}
			return value;
		}

		public static PhotonView[] GetPhotonViewsInChildren(this GameObject go)
		{
			return go.GetComponentsInChildren<PhotonView>(true);
		}

		public static PhotonView GetPhotonView(this GameObject go)
		{
			return go.GetComponent<PhotonView>();
		}

		public static bool AlmostEquals(this Vector3 target, Vector3 second, float sqrMagnitudePrecision)
		{
			return (target - second).sqrMagnitude < sqrMagnitudePrecision;
		}

		public static bool AlmostEquals(this Vector2 target, Vector2 second, float sqrMagnitudePrecision)
		{
			return (target - second).sqrMagnitude < sqrMagnitudePrecision;
		}

		public static bool AlmostEquals(this Quaternion target, Quaternion second, float maxAngle)
		{
			return Quaternion.Angle(target, second) < maxAngle;
		}

		public static bool AlmostEquals(this float target, float second, float floatDiff)
		{
			return Mathf.Abs(target - second) < floatDiff;
		}
	}
}
