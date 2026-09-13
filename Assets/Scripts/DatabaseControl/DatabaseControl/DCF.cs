using System.Collections;
using UnityEngine;

namespace DatabaseControl
{
	public class DCF
	{
		private static string PwKey(string username)
		{
			return "DCF_LOCAL_PW_" + username;
		}

		private static string DataKey(string username)
		{
			return "DCF_LOCAL_DATA_" + username;
		}

		public static IEnumerator RegisterUser(string username, string password, string data)
		{
			PlayerPrefs.SetString(PwKey(username), password);
			if (!PlayerPrefs.HasKey(DataKey(username)))
			{
				PlayerPrefs.SetString(DataKey(username), data);
			}
			PlayerPrefs.Save();
			yield return "Success";
		}

		public static IEnumerator Login(string username, string password)
		{
			if (!PlayerPrefs.HasKey(PwKey(username)))
			{
				PlayerPrefs.SetString(PwKey(username), password);
				PlayerPrefs.Save();
			}
			yield return "Success";
		}

		public static IEnumerator GetUserData(string username, string password)
		{
			yield return PlayerPrefs.GetString(DataKey(username));
		}

		public static IEnumerator SetUserData(string username, string password, string data)
		{
			PlayerPrefs.SetString(DataKey(username), data);
			PlayerPrefs.Save();
			yield return "Success";
		}
	}
}
