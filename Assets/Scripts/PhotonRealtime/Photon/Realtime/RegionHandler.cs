using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	public class RegionHandler
	{
		private string availableRegionCodes;

		private Region bestRegionCache;

		private List<RegionPinger> pingerList;

		private Action<RegionHandler> onCompleteCall;

		private int previousPing;

		public List<Region> EnabledRegions { get; protected internal set; }

		public Region BestRegion
		{
			get
			{
				if (EnabledRegions == null)
				{
					return null;
				}
				if (bestRegionCache != null)
				{
					return bestRegionCache;
				}
				Region result = null;
				int num = int.MaxValue;
				foreach (Region enabledRegion in EnabledRegions)
				{
					if (enabledRegion.Ping != 0 && enabledRegion.Ping < num)
					{
						num = enabledRegion.Ping;
						result = enabledRegion;
					}
				}
				bestRegionCache = result;
				return result;
			}
		}

		public string SummaryToCache
		{
			get
			{
				if (BestRegion != null)
				{
					return BestRegion.Code + ";" + BestRegion.Ping + ";" + availableRegionCodes;
				}
				return availableRegionCodes;
			}
		}

		public bool IsPinging { get; private set; }

		public void SetRegions(OperationResponse opGetRegions)
		{
			if (opGetRegions.OperationCode != 220 || opGetRegions.ReturnCode != 0)
			{
				return;
			}
			string[] array = opGetRegions[210] as string[];
			string[] array2 = opGetRegions[230] as string[];
			if (array == null || array2 == null || array.Length != array2.Length)
			{
				return;
			}
			bestRegionCache = null;
			EnabledRegions = new List<Region>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				Region region = new Region(array[i], array2[i]);
				if (!string.IsNullOrEmpty(region.Code))
				{
					EnabledRegions.Add(region);
				}
			}
			Array.Sort(array);
			availableRegionCodes = string.Join(",", array);
		}

		public bool PingMinimumOfRegions(Action<RegionHandler> onCompleteCallback, string previousSummary)
		{
			if (EnabledRegions == null || EnabledRegions.Count == 0)
			{
				return false;
			}
			if (IsPinging)
			{
				return false;
			}
			IsPinging = true;
			onCompleteCall = onCompleteCallback;
			if (string.IsNullOrEmpty(previousSummary))
			{
				return PingEnabledRegions();
			}
			string[] array = previousSummary.Split(';');
			if (array.Length < 3)
			{
				return PingEnabledRegions();
			}
			int result;
			if (!int.TryParse(array[1], out result))
			{
				return PingEnabledRegions();
			}
			string prevBestRegionCode = array[0];
			string value = array[2];
			if (string.IsNullOrEmpty(prevBestRegionCode))
			{
				return PingEnabledRegions();
			}
			if (string.IsNullOrEmpty(value))
			{
				return PingEnabledRegions();
			}
			if (!availableRegionCodes.Equals(value) || !availableRegionCodes.Contains(prevBestRegionCode))
			{
				return PingEnabledRegions();
			}
			if (result >= RegionPinger.PingWhenFailed)
			{
				return PingEnabledRegions();
			}
			previousPing = result;
			Region region = EnabledRegions.Find((Region r) => r.Code.Equals(prevBestRegionCode));
			RegionPinger regionPinger = new RegionPinger(region, OnPreferredRegionPinged);
			regionPinger.Start();
			return true;
		}

		private void OnPreferredRegionPinged(Region preferredRegion)
		{
			if ((float)preferredRegion.Ping > (float)previousPing * 1.5f)
			{
				PingEnabledRegions();
				return;
			}
			IsPinging = false;
			onCompleteCall(this);
		}

		private bool PingEnabledRegions()
		{
			if (EnabledRegions == null || EnabledRegions.Count == 0)
			{
				return false;
			}
			pingerList = new List<RegionPinger>();
			foreach (Region enabledRegion in EnabledRegions)
			{
				RegionPinger regionPinger = new RegionPinger(enabledRegion, OnRegionDone);
				pingerList.Add(regionPinger);
				regionPinger.Start();
			}
			return true;
		}

		private void OnRegionDone(Region region)
		{
			foreach (RegionPinger pinger in pingerList)
			{
				if (!pinger.Done)
				{
					return;
				}
			}
			IsPinging = false;
			onCompleteCall(this);
		}
	}
}
