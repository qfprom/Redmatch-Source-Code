using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public class PunTurnManager : MonoBehaviourPunCallbacks, IOnEventCallback
	{
		public float TurnDuration = 20f;

		public IPunTurnManagerCallbacks TurnManagerListener;

		private readonly HashSet<Player> finishedPlayers = new HashSet<Player>();

		public const byte TurnManagerEventOffset = 0;

		public const byte EvMove = 1;

		public const byte EvFinalMove = 2;

		private bool _isOverCallProcessed;

		public int Turn
		{
			get
			{
				return PhotonNetwork.CurrentRoom.GetTurn();
			}
			private set
			{
				_isOverCallProcessed = false;
				PhotonNetwork.CurrentRoom.SetTurn(value, true);
			}
		}

		public float ElapsedTimeInTurn
		{
			get
			{
				return (float)(PhotonNetwork.ServerTimestamp - PhotonNetwork.CurrentRoom.GetTurnStart()) / 1000f;
			}
		}

		public float RemainingSecondsInTurn
		{
			get
			{
				return Mathf.Max(0f, TurnDuration - ElapsedTimeInTurn);
			}
		}

		public bool IsCompletedByAll
		{
			get
			{
				return PhotonNetwork.CurrentRoom != null && Turn > 0 && finishedPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount;
			}
		}

		public bool IsFinishedByMe
		{
			get
			{
				return finishedPlayers.Contains(PhotonNetwork.LocalPlayer);
			}
		}

		public bool IsOver
		{
			get
			{
				return RemainingSecondsInTurn <= 0f;
			}
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (Turn > 0 && IsOver && !_isOverCallProcessed)
			{
				_isOverCallProcessed = true;
				TurnManagerListener.OnTurnTimeEnds(Turn);
			}
		}

		public void BeginTurn()
		{
			Turn++;
		}

		public void SendMove(object move, bool finished)
		{
			if (IsFinishedByMe)
			{
				Debug.LogWarning("Can't SendMove. Turn is finished by this player.");
				return;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("turn", Turn);
			hashtable.Add("move", move);
			byte eventCode = (byte)((!finished) ? 1 : 2);
			PhotonNetwork.RaiseEvent(eventCode, hashtable, new RaiseEventOptions
			{
				CachingOption = EventCaching.AddToRoomCache
			}, SendOptions.SendReliable);
			if (finished)
			{
				PhotonNetwork.LocalPlayer.SetFinishedTurn(Turn);
			}
			ProcessOnEvent(eventCode, hashtable, PhotonNetwork.LocalPlayer.ActorNumber);
		}

		public bool GetPlayerFinishedTurn(Player player)
		{
			if (player != null && finishedPlayers != null && finishedPlayers.Contains(player))
			{
				return true;
			}
			return false;
		}

		private void ProcessOnEvent(byte eventCode, object content, int senderId)
		{
			Player player = PhotonNetwork.CurrentRoom.GetPlayer(senderId);
			switch (eventCode)
			{
			case 1:
			{
				Hashtable hashtable2 = content as Hashtable;
				int turn = (int)hashtable2["turn"];
				object move2 = hashtable2["move"];
				TurnManagerListener.OnPlayerMove(player, turn, move2);
				break;
			}
			case 2:
			{
				Hashtable hashtable = content as Hashtable;
				int num = (int)hashtable["turn"];
				object move = hashtable["move"];
				if (num == Turn)
				{
					finishedPlayers.Add(player);
					TurnManagerListener.OnPlayerFinished(player, num, move);
				}
				if (IsCompletedByAll)
				{
					TurnManagerListener.OnTurnCompleted(Turn);
				}
				break;
			}
			}
		}

		public void OnEvent(EventData photonEvent)
		{
			ProcessOnEvent(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
		}

		public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			if (propertiesThatChanged.ContainsKey("Turn"))
			{
				_isOverCallProcessed = false;
				finishedPlayers.Clear();
				TurnManagerListener.OnTurnBegins(Turn);
			}
		}
	}
}
