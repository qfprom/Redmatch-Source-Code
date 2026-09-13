using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class AutoMoveAndRotate : MonoBehaviour
	{
		[Serializable]
		public class Vector3andSpace
		{
			public Vector3 value;

			public Space space = Space.Self;
		}

		public Vector3andSpace moveUnitsPerSecond;

		public Vector3andSpace rotateDegreesPerSecond;

		public bool ignoreTimescale;

		private float m_LastRealTime;

		public bool startRandomRot;

		private void Start()
		{
			if (startRandomRot)
			{
				base.transform.Rotate(rotateDegreesPerSecond.value * UnityEngine.Random.Range(0f, 360f));
			}
			m_LastRealTime = Time.realtimeSinceStartup;
		}

		private void LateUpdate()
		{
			float num = Time.deltaTime;
			if (ignoreTimescale)
			{
				num = Time.realtimeSinceStartup - m_LastRealTime;
				m_LastRealTime = Time.realtimeSinceStartup;
			}
			base.transform.Translate(moveUnitsPerSecond.value * num, moveUnitsPerSecond.space);
			base.transform.Rotate(rotateDegreesPerSecond.value * num, moveUnitsPerSecond.space);
		}
	}
}
