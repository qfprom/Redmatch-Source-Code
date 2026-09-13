using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts
{
	public class PhotonVoiceStatsGui : MonoBehaviour
	{
		private bool statsWindowOn = true;

		private bool statsOn = true;

		private bool healthStatsVisible;

		private bool trafficStatsOn;

		private bool buttonsOn;

		private Rect statsRect = new Rect(0f, 100f, 300f, 50f);

		private int windowId = 200;

		private PhotonPeer peer;

		private void OnEnable()
		{
			VoiceConnection[] components = GetComponents<VoiceConnection>();
			if (components == null || components.Length == 0)
			{
				Debug.LogError("No VoiceConnection component found, PhotonVoiceStatsGui disabled", this);
				base.enabled = false;
			}
			if (components.Length > 1)
			{
				Debug.LogWarningFormat(this, "Multiple VoiceConnection components found, using first occurrence attached to GameObject {0}", components[0].name);
			}
			peer = components[0].Client.LoadBalancingPeer;
			if (statsRect.x <= 0f)
			{
				statsRect.x = (float)Screen.width - statsRect.width;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
			{
				statsWindowOn = !statsWindowOn;
				statsOn = true;
			}
		}

		private void OnGUI()
		{
			if (peer.TrafficStatsEnabled != statsOn)
			{
				peer.TrafficStatsEnabled = statsOn;
			}
			if (statsWindowOn)
			{
				statsRect = GUILayout.Window(windowId, statsRect, TrafficStatsWindow, "Voice Client Messages (shift+tab)");
			}
		}

		private void TrafficStatsWindow(int windowId)
		{
			bool flag = false;
			TrafficStatsGameLevel trafficStatsGameLevel = peer.TrafficStatsGameLevel;
			long num = peer.TrafficStatsElapsedMs / 1000;
			if (num == 0)
			{
				num = 1L;
			}
			GUILayout.BeginHorizontal();
			buttonsOn = GUILayout.Toggle(buttonsOn, "buttons");
			healthStatsVisible = GUILayout.Toggle(healthStatsVisible, "health");
			trafficStatsOn = GUILayout.Toggle(trafficStatsOn, "traffic");
			GUILayout.EndHorizontal();
			string text = string.Format("Out {0,4} | In {1,4} | Sum {2,4}", trafficStatsGameLevel.TotalOutgoingMessageCount, trafficStatsGameLevel.TotalIncomingMessageCount, trafficStatsGameLevel.TotalMessageCount);
			string text2 = string.Format("{0}sec average:", num);
			string text3 = string.Format("Out {0,4} | In {1,4} | Sum {2,4}", trafficStatsGameLevel.TotalOutgoingMessageCount / num, trafficStatsGameLevel.TotalIncomingMessageCount / num, trafficStatsGameLevel.TotalMessageCount / num);
			GUILayout.Label(text);
			GUILayout.Label(text2);
			GUILayout.Label(text3);
			if (buttonsOn)
			{
				GUILayout.BeginHorizontal();
				statsOn = GUILayout.Toggle(statsOn, "stats on");
				if (GUILayout.Button("Reset"))
				{
					peer.TrafficStatsReset();
					peer.TrafficStatsEnabled = true;
				}
				flag = GUILayout.Button("To Log");
				GUILayout.EndHorizontal();
			}
			string text4 = string.Empty;
			string text5 = string.Empty;
			if (trafficStatsOn)
			{
				GUILayout.Box("Voice Client Traffic Stats");
				text4 = "Incoming: \n" + peer.TrafficStatsIncoming;
				text5 = "Outgoing: \n" + peer.TrafficStatsOutgoing;
				GUILayout.Label(text4);
				GUILayout.Label(text5);
			}
			string text6 = string.Empty;
			if (healthStatsVisible)
			{
				GUILayout.Box("Voice Client Health Stats");
				text6 = string.Format("ping: {6}[+/-{7}]ms resent:{8} \n\nmax ms between\nsend: {0,4} \ndispatch: {1,4} \n\nlongest dispatch for: \nev({3}):{2,3}ms \nop({5}):{4,3}ms", trafficStatsGameLevel.LongestDeltaBetweenSending, trafficStatsGameLevel.LongestDeltaBetweenDispatching, trafficStatsGameLevel.LongestEventCallback, trafficStatsGameLevel.LongestEventCallbackCode, trafficStatsGameLevel.LongestOpResponseCallback, trafficStatsGameLevel.LongestOpResponseCallbackOpCode, peer.RoundTripTime, peer.RoundTripTimeVariance, peer.ResentReliableCommands);
				GUILayout.Label(text6);
			}
			if (flag)
			{
				string message = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}", text, text2, text3, text4, text5, text6);
				Debug.Log(message);
			}
			if (GUI.changed)
			{
				statsRect.height = 100f;
			}
			GUI.DragWindow();
		}
	}
}
