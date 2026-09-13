using System;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime
{
	public class ConnectionHandler : MonoBehaviour
	{
		private byte fallbackThreadId = byte.MaxValue;

		private bool didSendAcks;

		private int startedAckingTimestamp;

		private int deltaSinceStartedToAck;

		public int KeepAliveInBackground = 60000;

		public bool ApplyDontDestroyOnLoad = true;

		public LoadBalancingClient Client { get; set; }

		public int CountSendAcksOnly { get; private set; }

		public bool FallbackThreadRunning
		{
			get
			{
				return fallbackThreadId < byte.MaxValue;
			}
		}

		protected virtual void Awake()
		{
			if (ApplyDontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		protected virtual void OnDestroy()
		{
			StopFallbackSendAckThread();
		}

		protected virtual void OnApplicationQuit()
		{
			StopFallbackSendAckThread();
			if (Client != null)
			{
				Client.Disconnect();
				Client.LoadBalancingPeer.StopThread();
			}
			SupportClass.StopAllBackgroundCalls();
		}

		public void StartFallbackSendAckThread()
		{
			if (!FallbackThreadRunning)
			{
				fallbackThreadId = SupportClass.StartBackgroundCalls(RealtimeFallbackThread, 50, "RealtimeFallbackThread");
			}
		}

		public void StopFallbackSendAckThread()
		{
			if (FallbackThreadRunning)
			{
				SupportClass.StopBackgroundCalls(fallbackThreadId);
				fallbackThreadId = byte.MaxValue;
			}
		}

		public bool RealtimeFallbackThread()
		{
			if (Client != null)
			{
				if (!Client.IsConnected)
				{
					didSendAcks = false;
					return true;
				}
				if (Client.LoadBalancingPeer.ConnectionTime - Client.LoadBalancingPeer.LastSendOutgoingTime > 100)
				{
					if (didSendAcks)
					{
						deltaSinceStartedToAck = Environment.TickCount - startedAckingTimestamp;
						if (deltaSinceStartedToAck > KeepAliveInBackground)
						{
							return true;
						}
					}
					else
					{
						startedAckingTimestamp = Environment.TickCount;
					}
					didSendAcks = true;
					CountSendAcksOnly++;
					Client.LoadBalancingPeer.SendAcksOnly();
				}
				else
				{
					didSendAcks = false;
				}
			}
			return true;
		}
	}
}
