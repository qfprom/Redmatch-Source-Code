using System;
using System.Diagnostics;
using System.Threading;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts
{
	public class CountdownTimer : MonoBehaviourPunCallbacks
	{
		public delegate void CountdownTimerHasExpired();

		public const string CountdownStartTime = "StartTime";

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static CountdownTimerHasExpired OnCountdownTimerHasExpired__BackingField;

		private bool isTimerRunning;

		private float startTime;

		[Header("Reference to a Text component for visualizing the countdown")]
		public Text Text;

		[Header("Countdown time in seconds")]
		public float Countdown = 5f;

		public static event CountdownTimerHasExpired OnCountdownTimerHasExpired
		{
			add
			{
				CountdownTimerHasExpired countdownTimerHasExpired = OnCountdownTimerHasExpired__BackingField;
				CountdownTimerHasExpired countdownTimerHasExpired2;
				do
				{
					countdownTimerHasExpired2 = countdownTimerHasExpired;
					countdownTimerHasExpired = Interlocked.CompareExchange(ref OnCountdownTimerHasExpired__BackingField, (CountdownTimerHasExpired)Delegate.Combine(countdownTimerHasExpired2, value), countdownTimerHasExpired);
				}
				while ((object)countdownTimerHasExpired != countdownTimerHasExpired2);
			}
			remove
			{
				CountdownTimerHasExpired countdownTimerHasExpired = OnCountdownTimerHasExpired__BackingField;
				CountdownTimerHasExpired countdownTimerHasExpired2;
				do
				{
					countdownTimerHasExpired2 = countdownTimerHasExpired;
					countdownTimerHasExpired = Interlocked.CompareExchange(ref OnCountdownTimerHasExpired__BackingField, (CountdownTimerHasExpired)Delegate.Remove(countdownTimerHasExpired2, value), countdownTimerHasExpired);
				}
				while ((object)countdownTimerHasExpired != countdownTimerHasExpired2);
			}
		}

		public void Start()
		{
			if (Text == null)
			{
				Debug.LogError("Reference to 'Text' is not set. Please set a valid reference.", this);
			}
		}

		public void Update()
		{
			if (!isTimerRunning)
			{
				return;
			}
			float num = (float)PhotonNetwork.Time - startTime;
			float num2 = Countdown - num;
			Text.text = string.Format("Game starts in {0} seconds", num2.ToString("n2"));
			if (!(num2 > 0f))
			{
				isTimerRunning = false;
				Text.text = string.Empty;
				if (OnCountdownTimerHasExpired__BackingField != null)
				{
					OnCountdownTimerHasExpired__BackingField();
				}
			}
		}

		public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			object value;
			if (propertiesThatChanged.TryGetValue("StartTime", out value))
			{
				isTimerRunning = true;
				startTime = (float)value;
			}
		}
	}
}
