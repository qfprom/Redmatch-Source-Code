using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun
{
	[Serializable]
	[HelpURL("https://doc.photonengine.com/en-us/pun/v2/getting-started/initial-setup")]
	public class ServerSettings : ScriptableObject
	{
		[Tooltip("Core Photon Server/Cloud settings.")]
		public AppSettings AppSettings;

		[Tooltip("Simulates an online connection.\nPUN can be used as usual.")]
		public bool StartInOfflineMode;

		[Tooltip("Log output by PUN.")]
		public PunLogLevel PunLogging;

		[Tooltip("Logs additional info for debugging.")]
		public bool EnableSupportLogger;

		[Tooltip("Enables apps to keep the connection without focus.")]
		public bool RunInBackground = true;

		[Tooltip("RPC name list.\nUsed as shortcut when sending calls.")]
		public List<string> RpcList = new List<string>();

		public static string BestRegionSummaryInPreferences
		{
			get
			{
				return PhotonNetwork.BestRegionSummaryInPreferences;
			}
		}

		public void UseCloud(string cloudAppid, string code = "")
		{
			AppSettings.AppIdRealtime = cloudAppid;
			AppSettings.Server = null;
			AppSettings.FixedRegion = ((!string.IsNullOrEmpty(code)) ? code : null);
			Debug.Log("this.AppSettings.IsBestRegion: " + AppSettings.IsBestRegion);
		}

		public static bool IsAppId(string val)
		{
			try
			{
				new Guid(val);
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static void ResetBestRegionCodeInPreferences()
		{
			PhotonNetwork.BestRegionSummaryInPreferences = null;
		}

		public override string ToString()
		{
			return "ServerSettings: " + AppSettings.ToStringFull();
		}
	}
}
