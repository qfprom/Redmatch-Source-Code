using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	public static class Extensions
	{
		public static void Merge(this IDictionary target, IDictionary addHash)
		{
			if (addHash == null || target.Equals(addHash))
			{
				return;
			}
			foreach (object key in addHash.Keys)
			{
				target[key] = addHash[key];
			}
		}

		public static void MergeStringKeys(this IDictionary target, IDictionary addHash)
		{
			if (addHash == null || target.Equals(addHash))
			{
				return;
			}
			foreach (object key in addHash.Keys)
			{
				if (key is string)
				{
					target[key] = addHash[key];
				}
			}
		}

		public static string ToStringFull(this IDictionary origin)
		{
			return SupportClass.DictionaryToString(origin, false);
		}

		public static string ToStringFull<T>(this List<T> data)
		{
			if (data == null)
			{
				return "null";
			}
			string[] array = new string[data.Count];
			for (int i = 0; i < data.Count; i++)
			{
				object obj = data[i];
				array[i] = ((obj == null) ? "null" : obj.ToString());
			}
			return string.Join(", ", array);
		}

		public static string ToStringFull(this object[] data)
		{
			if (data == null)
			{
				return "null";
			}
			string[] array = new string[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				object obj = data[i];
				array[i] = ((obj == null) ? "null" : obj.ToString());
			}
			return string.Join(", ", array);
		}

		public static ExitGames.Client.Photon.Hashtable StripToStringKeys(this IDictionary original)
		{
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			if (original != null)
			{
				foreach (object key in original.Keys)
				{
					if (key is string)
					{
						hashtable[key] = original[key];
					}
				}
			}
			return hashtable;
		}

		public static void StripKeysWithNullValues(this IDictionary original)
		{
			object[] array = new object[original.Count];
			original.Keys.CopyTo(array, 0);
			foreach (object key in array)
			{
				if (original[key] == null)
				{
					original.Remove(key);
				}
			}
		}

		public static bool Contains(this int[] target, int nr)
		{
			if (target == null)
			{
				return false;
			}
			for (int i = 0; i < target.Length; i++)
			{
				if (target[i] == nr)
				{
					return true;
				}
			}
			return false;
		}
	}
}
