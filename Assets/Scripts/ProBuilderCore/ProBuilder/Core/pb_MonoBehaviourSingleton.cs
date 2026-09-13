using UnityEngine;

namespace ProBuilder.Core
{
	internal class pb_MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static MonoBehaviour m_Instance;

		public static T instance
		{
			get
			{
				if (nullableInstance == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.name = typeof(T).ToString();
					m_Instance = gameObject.AddComponent<T>();
				}
				return (T)m_Instance;
			}
		}

		public static T nullableInstance
		{
			get
			{
				if (m_Instance == null)
				{
					T[] array = Resources.FindObjectsOfTypeAll<T>();
					if (array != null && array.Length > 0)
					{
						m_Instance = array[0];
						for (int i = 1; i < array.Length; i++)
						{
							Object.DestroyImmediate(array[i]);
						}
					}
				}
				return (T)m_Instance;
			}
		}

		public static bool Valid()
		{
			return nullableInstance != null;
		}

		public virtual void Awake()
		{
			if (m_Instance == null)
			{
				m_Instance = this;
			}
			else
			{
				Object.Destroy(this);
			}
		}

		public virtual void OnEnable()
		{
			m_Instance = this;
		}
	}
}
