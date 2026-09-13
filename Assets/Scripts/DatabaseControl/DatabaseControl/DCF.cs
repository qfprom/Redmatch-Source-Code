using System;
using System.Collections;
using UnityEngine;

namespace DatabaseControl
{
	public class DCF
	{
		public static IEnumerator RegisterUser(string username, string password, string data)
		{
			string text = "Error";
			string s = "";
			string s2 = "";
			TextAsset textAsset = Resources.Load("DCF_RuntimeData") as TextAsset;
			if (textAsset != null)
			{
				string text2 = textAsset.text;
				if (!string.IsNullOrEmpty(text2) && text2 != "0")
				{
					string[] array = text2.Split(new string[1] { "-" }, StringSplitOptions.None);
					if (array.Length == 2 && !string.IsNullOrEmpty(array[0]) && !string.IsNullOrEmpty(array[1]))
					{
						s = array[0];
						s2 = array[1];
						text = "";
					}
				}
			}
			if (text == "")
			{
				WWWForm wWWForm = new WWWForm();
				wWWForm.AddField("databaseId", WWW.EscapeURL(s));
				wWWForm.AddField("databasePass", WWW.EscapeURL(s2));
				wWWForm.AddField("playerUser", WWW.EscapeURL(username));
				wWWForm.AddField("playerPass", WWW.EscapeURL(password));
				wWWForm.AddField("playerData", WWW.EscapeURL(data));
				wWWForm.AddField("version", "1.1.0");
				string url = "https://databasecontrolfree.azurewebsites.net/RegisterUser";
				WWW www = new WWW(url, wWWForm);
				while (!www.isDone)
				{
					yield return null;
				}
				yield return WWW.UnEscapeURL(www.text);
			}
			else
			{
				yield return "Error";
			}
		}

		public static IEnumerator Login(string username, string password)
		{
			string text = "Error";
			string s = "";
			string s2 = "";
			TextAsset textAsset = Resources.Load("DCF_RuntimeData") as TextAsset;
			if (textAsset != null)
			{
				string text2 = textAsset.text;
				if (!string.IsNullOrEmpty(text2) && text2 != "0")
				{
					string[] array = text2.Split(new string[1] { "-" }, StringSplitOptions.None);
					if (array.Length == 2 && !string.IsNullOrEmpty(array[0]) && !string.IsNullOrEmpty(array[1]))
					{
						s = array[0];
						s2 = array[1];
						text = "";
					}
				}
			}
			if (text == "")
			{
				WWWForm wWWForm = new WWWForm();
				wWWForm.AddField("databaseId", WWW.EscapeURL(s));
				wWWForm.AddField("databasePass", WWW.EscapeURL(s2));
				wWWForm.AddField("playerUser", WWW.EscapeURL(username));
				wWWForm.AddField("playerPass", WWW.EscapeURL(password));
				wWWForm.AddField("version", "1.1.0");
				string url = "https://databasecontrolfree.azurewebsites.net/LoginUser";
				WWW www = new WWW(url, wWWForm);
				while (!www.isDone)
				{
					yield return null;
				}
				yield return WWW.UnEscapeURL(www.text);
			}
			else
			{
				yield return "Error";
			}
		}

		public static IEnumerator GetUserData(string username, string password)
		{
			string text = "Error";
			string s = "";
			string s2 = "";
			TextAsset textAsset = Resources.Load("DCF_RuntimeData") as TextAsset;
			if (textAsset != null)
			{
				string text2 = textAsset.text;
				if (!string.IsNullOrEmpty(text2) && text2 != "0")
				{
					string[] array = text2.Split(new string[1] { "-" }, StringSplitOptions.None);
					if (array.Length == 2 && !string.IsNullOrEmpty(array[0]) && !string.IsNullOrEmpty(array[1]))
					{
						s = array[0];
						s2 = array[1];
						text = "";
					}
				}
			}
			if (text == "")
			{
				WWWForm wWWForm = new WWWForm();
				wWWForm.AddField("databaseId", WWW.EscapeURL(s));
				wWWForm.AddField("databasePass", WWW.EscapeURL(s2));
				wWWForm.AddField("playerUser", WWW.EscapeURL(username));
				wWWForm.AddField("playerPass", WWW.EscapeURL(password));
				wWWForm.AddField("version", "1.1.0");
				string url = "https://databasecontrolfree.azurewebsites.net/GetUserData";
				WWW www = new WWW(url, wWWForm);
				while (!www.isDone)
				{
					yield return null;
				}
				yield return WWW.UnEscapeURL(www.text);
			}
			else
			{
				yield return "Error";
			}
		}

		public static IEnumerator SetUserData(string username, string password, string data)
		{
			string text = "Error";
			string s = "";
			string s2 = "";
			TextAsset textAsset = Resources.Load("DCF_RuntimeData") as TextAsset;
			if (textAsset != null)
			{
				string text2 = textAsset.text;
				if (!string.IsNullOrEmpty(text2) && text2 != "0")
				{
					string[] array = text2.Split(new string[1] { "-" }, StringSplitOptions.None);
					if (array.Length == 2 && !string.IsNullOrEmpty(array[0]) && !string.IsNullOrEmpty(array[1]))
					{
						s = array[0];
						s2 = array[1];
						text = "";
					}
				}
			}
			if (text == "")
			{
				WWWForm wWWForm = new WWWForm();
				wWWForm.AddField("databaseId", WWW.EscapeURL(s));
				wWWForm.AddField("databasePass", WWW.EscapeURL(s2));
				wWWForm.AddField("playerUser", WWW.EscapeURL(username));
				wWWForm.AddField("playerPass", WWW.EscapeURL(password));
				wWWForm.AddField("playerData", WWW.EscapeURL(data));
				wWWForm.AddField("version", "1.1.0");
				string url = "https://databasecontrolfree.azurewebsites.net/SetUserData";
				WWW www = new WWW(url, wWWForm);
				while (!www.isDone)
				{
					yield return null;
				}
				yield return WWW.UnEscapeURL(www.text);
			}
			else
			{
				yield return "Error";
			}
		}
	}
}
