using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime
{
	public class Player
	{
		private int actorNumber = -1;

		public readonly bool IsLocal;

		private string nickName = string.Empty;

		public object TagObject;

		protected internal Room RoomReference { get; set; }

		public int ActorNumber
		{
			get
			{
				return actorNumber;
			}
		}

		public string NickName
		{
			get
			{
				return nickName;
			}
			set
			{
				if (string.IsNullOrEmpty(nickName) || !nickName.Equals(value))
				{
					nickName = value;
					if (IsLocal && RoomReference != null)
					{
						SetPlayerNameProperty();
					}
				}
			}
		}

		public string UserId { get; internal set; }

		public bool IsMasterClient
		{
			get
			{
				if (RoomReference == null)
				{
					return false;
				}
				return ActorNumber == RoomReference.MasterClientId;
			}
		}

		public bool IsInactive { get; protected internal set; }

		public Hashtable CustomProperties { get; set; }

		protected internal Player(string nickName, int actorNumber, bool isLocal)
			: this(nickName, actorNumber, isLocal, null)
		{
		}

		protected internal Player(string nickName, int actorNumber, bool isLocal, Hashtable playerProperties)
		{
			IsLocal = isLocal;
			this.actorNumber = actorNumber;
			NickName = nickName;
			CustomProperties = new Hashtable();
			InternalCacheProperties(playerProperties);
		}

		public Player Get(int id)
		{
			if (RoomReference == null)
			{
				return null;
			}
			return RoomReference.GetPlayer(id);
		}

		public Player GetNext()
		{
			return GetNextFor(ActorNumber);
		}

		public Player GetNextFor(Player currentPlayer)
		{
			if (currentPlayer == null)
			{
				return null;
			}
			return GetNextFor(currentPlayer.ActorNumber);
		}

		public Player GetNextFor(int currentPlayerId)
		{
			if (RoomReference == null || RoomReference.Players == null || RoomReference.Players.Count < 2)
			{
				return null;
			}
			Dictionary<int, Player> players = RoomReference.Players;
			int num = int.MaxValue;
			int num2 = currentPlayerId;
			foreach (int key in players.Keys)
			{
				if (key < num2)
				{
					num2 = key;
				}
				else if (key > currentPlayerId && key < num)
				{
					num = key;
				}
			}
			return (num == int.MaxValue) ? players[num2] : players[num];
		}

		public virtual void InternalCacheProperties(Hashtable properties)
		{
			if (properties == null || properties.Count == 0 || CustomProperties.Equals(properties))
			{
				return;
			}
			if (properties.ContainsKey(byte.MaxValue))
			{
				string text = (string)properties[byte.MaxValue];
				if (text != null)
				{
					if (IsLocal)
					{
						if (!text.Equals(nickName))
						{
							SetPlayerNameProperty();
						}
					}
					else
					{
						NickName = text;
					}
				}
			}
			if (properties.ContainsKey((byte)253))
			{
				UserId = (string)properties[(byte)253];
			}
			if (properties.ContainsKey((byte)254))
			{
				IsInactive = (bool)properties[(byte)254];
			}
			CustomProperties.MergeStringKeys(properties);
			CustomProperties.StripKeysWithNullValues();
		}

		public override string ToString()
		{
			return ((!string.IsNullOrEmpty(NickName)) ? nickName : ActorNumber.ToString()) + " " + SupportClass.DictionaryToString(CustomProperties);
		}

		public string ToStringFull()
		{
			return string.Format("#{0:00} '{1}'{2} {3}", ActorNumber, NickName, (!IsInactive) ? string.Empty : " (inactive)", CustomProperties.ToStringFull());
		}

		public override bool Equals(object p)
		{
			Player player = p as Player;
			return player != null && GetHashCode() == player.GetHashCode();
		}

		public override int GetHashCode()
		{
			return ActorNumber;
		}

		protected internal void ChangeLocalID(int newID)
		{
			if (IsLocal)
			{
				actorNumber = newID;
			}
		}

		public void SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedValues = null, WebFlags webFlags = null)
		{
			if (propertiesToSet == null)
			{
				return;
			}
			Hashtable hashtable = propertiesToSet.StripToStringKeys();
			Hashtable hashtable2 = expectedValues.StripToStringKeys();
			if (hashtable2 == null || hashtable2.Count == 0)
			{
				CustomProperties.Merge(hashtable);
				CustomProperties.StripKeysWithNullValues();
			}
			if (RoomReference != null)
			{
				if (RoomReference.IsOffline)
				{
					RoomReference.LoadBalancingClient.InRoomCallbackTargets.OnPlayerPropertiesUpdate(this, hashtable);
				}
				else
				{
					RoomReference.LoadBalancingClient.LoadBalancingPeer.OpSetPropertiesOfActor(actorNumber, hashtable, hashtable2, webFlags);
				}
			}
		}

		private void SetPlayerNameProperty()
		{
			if (RoomReference != null)
			{
				Hashtable hashtable = new Hashtable();
				hashtable[byte.MaxValue] = nickName;
				RoomReference.LoadBalancingClient.LoadBalancingPeer.OpSetPropertiesOfActor(ActorNumber, hashtable);
			}
		}
	}
}
