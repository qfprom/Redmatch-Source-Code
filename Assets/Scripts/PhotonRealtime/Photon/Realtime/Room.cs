using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	public class Room : RoomInfo
	{
		private bool isOffline;

		private Dictionary<int, Player> players = new Dictionary<int, Player>();

		public LoadBalancingClient LoadBalancingClient { get; set; }

		public new string Name
		{
			get
			{
				return name;
			}
			internal set
			{
				name = value;
			}
		}

		public bool IsOffline
		{
			get
			{
				return isOffline;
			}
			private set
			{
				isOffline = value;
			}
		}

		public new bool IsOpen
		{
			get
			{
				return isOpen;
			}
			set
			{
				if (value != isOpen && !isOffline)
				{
					LoadBalancingClient.OpSetPropertiesOfRoom(new Hashtable { 
					{
						(byte)253,
						value
					} });
				}
				isOpen = value;
			}
		}

		public new bool IsVisible
		{
			get
			{
				return isVisible;
			}
			set
			{
				if (value != isVisible && !isOffline)
				{
					LoadBalancingClient.OpSetPropertiesOfRoom(new Hashtable { 
					{
						(byte)254,
						value
					} });
				}
				isVisible = value;
			}
		}

		public new byte MaxPlayers
		{
			get
			{
				return maxPlayers;
			}
			set
			{
				if (value != maxPlayers && !isOffline)
				{
					LoadBalancingClient.OpSetPropertiesOfRoom(new Hashtable { 
					{
						byte.MaxValue,
						value
					} });
				}
				maxPlayers = value;
			}
		}

		public new byte PlayerCount
		{
			get
			{
				if (Players == null)
				{
					return 0;
				}
				return (byte)Players.Count;
			}
		}

		public Dictionary<int, Player> Players
		{
			get
			{
				return players;
			}
			private set
			{
				players = value;
			}
		}

		public string[] ExpectedUsers
		{
			get
			{
				return expectedUsers;
			}
		}

		public int PlayerTtl
		{
			get
			{
				return playerTtl;
			}
			set
			{
				if (value != playerTtl && !isOffline)
				{
					LoadBalancingClient.OpSetPropertyOfRoom(246, value);
				}
				playerTtl = value;
			}
		}

		public int EmptyRoomTtl
		{
			get
			{
				return emptyRoomTtl;
			}
			set
			{
				if (value != emptyRoomTtl && !isOffline)
				{
					LoadBalancingClient.OpSetPropertyOfRoom(245, value);
				}
				emptyRoomTtl = value;
			}
		}

		public int MasterClientId
		{
			get
			{
				return masterClientId;
			}
		}

		public string[] PropertiesListedInLobby
		{
			get
			{
				return propertiesListedInLobby;
			}
			private set
			{
				propertiesListedInLobby = value;
			}
		}

		public bool AutoCleanUp
		{
			get
			{
				return autoCleanUp;
			}
		}

		public Room(string roomName, RoomOptions options, bool isOffline = false)
			: base(roomName, (options == null) ? null : options.CustomRoomProperties)
		{
			if (options != null)
			{
				isVisible = options.IsVisible;
				isOpen = options.IsOpen;
				maxPlayers = options.MaxPlayers;
				propertiesListedInLobby = options.CustomRoomPropertiesForLobby;
			}
			this.isOffline = isOffline;
		}

		protected internal override void InternalCacheProperties(Hashtable propertiesToCache)
		{
			int num = masterClientId;
			base.InternalCacheProperties(propertiesToCache);
			if (num != 0 && masterClientId != num)
			{
				LoadBalancingClient.InRoomCallbackTargets.OnMasterClientSwitched(GetPlayer(masterClientId));
			}
		}

		public virtual void SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
		{
			Hashtable hashtable = propertiesToSet.StripToStringKeys();
			if (isOffline)
			{
				base.CustomProperties.Merge(hashtable);
				base.CustomProperties.StripKeysWithNullValues();
				LoadBalancingClient.InRoomCallbackTargets.OnRoomPropertiesUpdate(propertiesToSet);
				return;
			}
			if (expectedProperties == null || expectedProperties.Count == 0)
			{
				base.CustomProperties.Merge(hashtable);
				base.CustomProperties.StripKeysWithNullValues();
			}
			LoadBalancingClient.LoadBalancingPeer.OpSetPropertiesOfRoom(hashtable, expectedProperties, webFlags);
		}

		public void SetPropertiesListedInLobby(string[] propertiesListedInLobby)
		{
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)250] = propertiesListedInLobby;
			if (LoadBalancingClient.OpSetPropertiesOfRoom(hashtable))
			{
				base.propertiesListedInLobby = propertiesListedInLobby;
			}
		}

		protected internal virtual void RemovePlayer(Player player)
		{
			Players.Remove(player.ActorNumber);
			player.RoomReference = null;
		}

		protected internal virtual void RemovePlayer(int id)
		{
			RemovePlayer(GetPlayer(id));
		}

		public bool SetMasterClient(Player masterClientPlayer)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add((byte)248, masterClientPlayer.ActorNumber);
			Hashtable gameProperties = hashtable;
			hashtable = new Hashtable();
			hashtable.Add((byte)248, MasterClientId);
			Hashtable expectedProperties = hashtable;
			return LoadBalancingClient.OpSetPropertiesOfRoom(gameProperties, expectedProperties);
		}

		public virtual bool AddPlayer(Player player)
		{
			if (!Players.ContainsKey(player.ActorNumber))
			{
				StorePlayer(player);
				return true;
			}
			return false;
		}

		public virtual Player StorePlayer(Player player)
		{
			Players[player.ActorNumber] = player;
			player.RoomReference = this;
			if (MasterClientId == 0 || player.ActorNumber < MasterClientId)
			{
				masterClientId = player.ActorNumber;
			}
			return player;
		}

		public virtual Player GetPlayer(int id)
		{
			Player value = null;
			Players.TryGetValue(id, out value);
			return value;
		}

		public void ClearExpectedUsers()
		{
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)247] = new string[0];
			Hashtable hashtable2 = new Hashtable();
			hashtable2[(byte)247] = ExpectedUsers;
			LoadBalancingClient.OpSetPropertiesOfRoom(hashtable, hashtable2);
		}

		public override string ToString()
		{
			return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", name, (!isVisible) ? "hidden" : "visible", (!isOpen) ? "closed" : "open", maxPlayers, PlayerCount);
		}

		public new string ToStringFull()
		{
			return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", name, (!isVisible) ? "hidden" : "visible", (!isOpen) ? "closed" : "open", maxPlayers, PlayerCount, base.CustomProperties.ToStringFull());
		}
	}
}
