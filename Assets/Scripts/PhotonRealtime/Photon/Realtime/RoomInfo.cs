using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	public class RoomInfo
	{
		public bool RemovedFromList;

		private Hashtable customProperties = new Hashtable();

		protected byte maxPlayers;

		protected int emptyRoomTtl;

		protected int playerTtl;

		protected string[] expectedUsers;

		protected bool isOpen = true;

		protected bool isVisible = true;

		protected bool autoCleanUp = true;

		protected string name;

		public int masterClientId;

		protected string[] propertiesListedInLobby;

		public Hashtable CustomProperties
		{
			get
			{
				return customProperties;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

		public int PlayerCount { get; private set; }

		public byte MaxPlayers
		{
			get
			{
				return maxPlayers;
			}
		}

		public bool IsOpen
		{
			get
			{
				return isOpen;
			}
		}

		public bool IsVisible
		{
			get
			{
				return isVisible;
			}
		}

		protected internal RoomInfo(string roomName, Hashtable roomProperties)
		{
			InternalCacheProperties(roomProperties);
			name = roomName;
		}

		public override bool Equals(object other)
		{
			RoomInfo roomInfo = other as RoomInfo;
			return roomInfo != null && Name.Equals(roomInfo.name);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", name, (!isVisible) ? "hidden" : "visible", (!isOpen) ? "closed" : "open", maxPlayers, PlayerCount);
		}

		public string ToStringFull()
		{
			return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", name, (!isVisible) ? "hidden" : "visible", (!isOpen) ? "closed" : "open", maxPlayers, PlayerCount, customProperties.ToStringFull());
		}

		protected internal virtual void InternalCacheProperties(Hashtable propertiesToCache)
		{
			if (propertiesToCache == null || propertiesToCache.Count == 0 || customProperties.Equals(propertiesToCache))
			{
				return;
			}
			if (propertiesToCache.ContainsKey((byte)251))
			{
				RemovedFromList = (bool)propertiesToCache[(byte)251];
				if (RemovedFromList)
				{
					return;
				}
			}
			if (propertiesToCache.ContainsKey(byte.MaxValue))
			{
				maxPlayers = (byte)propertiesToCache[byte.MaxValue];
			}
			if (propertiesToCache.ContainsKey((byte)253))
			{
				isOpen = (bool)propertiesToCache[(byte)253];
			}
			if (propertiesToCache.ContainsKey((byte)254))
			{
				isVisible = (bool)propertiesToCache[(byte)254];
			}
			if (propertiesToCache.ContainsKey((byte)252))
			{
				PlayerCount = (byte)propertiesToCache[(byte)252];
			}
			if (propertiesToCache.ContainsKey((byte)249))
			{
				autoCleanUp = (bool)propertiesToCache[(byte)249];
			}
			if (propertiesToCache.ContainsKey((byte)248))
			{
				masterClientId = (int)propertiesToCache[(byte)248];
			}
			if (propertiesToCache.ContainsKey((byte)250))
			{
				propertiesListedInLobby = propertiesToCache[(byte)250] as string[];
			}
			if (propertiesToCache.ContainsKey((byte)247))
			{
				expectedUsers = (string[])propertiesToCache[(byte)247];
			}
			if (propertiesToCache.ContainsKey((byte)245))
			{
				emptyRoomTtl = (int)propertiesToCache[(byte)245];
			}
			if (propertiesToCache.ContainsKey((byte)246))
			{
				playerTtl = (int)propertiesToCache[(byte)246];
			}
			customProperties.MergeStringKeys(propertiesToCache);
			customProperties.StripKeysWithNullValues();
		}
	}
}
