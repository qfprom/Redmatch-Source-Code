using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Voice.DemoVoiceUI
{
	[RequireComponent(typeof(VoiceConnection))]
	public class DemoVoiceUI : MonoBehaviour
	{
		[SerializeField]
		private Text connectionStatusText;

		[SerializeField]
		private Text serverStatusText;

		[SerializeField]
		private Text roomStatusText;

		[SerializeField]
		private Text inputWarningText;

		[SerializeField]
		private Text packetLossWarningText;

		[SerializeField]
		private InputField localNicknameText;

		[SerializeField]
		private WebRtcAudioDsp voiceAudioPreprocessor;

		[SerializeField]
		private Toggle debugEchoToggle;

		[SerializeField]
		private Toggle reliableTransmissionToggle;

		[SerializeField]
		private GameObject webRtcDspGameObject;

		[SerializeField]
		private Toggle aecToggle;

		[SerializeField]
		private Toggle noiseSuppressionToggle;

		[SerializeField]
		private Toggle agcToggle;

		[SerializeField]
		private Toggle vadToggle;

		public Transform RemoteVoicesPanel;

		private VoiceConnection voiceConnection;

		protected internal const string MutePropKey = "mute";

		private Color warningColor = new Color(0.9f, 0.5f, 0f, 1f);

		private Color okColor = new Color(0f, 0.6f, 0.2f, 1f);

		private void Awake()
		{
			voiceConnection = GetComponent<VoiceConnection>();
			InitToggles();
		}

		private void OnEnable()
		{
			voiceConnection.SpeakerLinked += OnSpeakerCreated;
			if (localNicknameText != null)
			{
				string text = PlayerPrefs.GetString("vNick");
				if (!string.IsNullOrEmpty(text))
				{
					Debug.Log("Saved nick = " + text);
					localNicknameText.text = text;
					voiceConnection.Client.NickName = text;
				}
			}
		}

		private void OnDisable()
		{
			voiceConnection.SpeakerLinked -= OnSpeakerCreated;
		}

		private void OnSpeakerCreated(Speaker speaker)
		{
			speaker.gameObject.transform.SetParent(RemoteVoicesPanel, false);
		}

		private void OnRemoteVoiceRemove(Speaker speaker)
		{
			Object.Destroy(speaker.gameObject);
		}

		public void ToggleTransmit()
		{
			bool flag = !voiceConnection.PrimaryRecorder.TransmitEnabled;
			voiceConnection.PrimaryRecorder.TransmitEnabled = flag;
			voiceConnection.Client.LocalPlayer.SetCustomProperties(new Hashtable { 
			{
				"mute",
				!flag
			} });
		}

		public void ToggleDebugEcho()
		{
			bool debugEchoMode = !voiceConnection.PrimaryRecorder.DebugEchoMode;
			voiceConnection.PrimaryRecorder.DebugEchoMode = debugEchoMode;
		}

		public void ToggleReliable()
		{
			bool reliableMode = !voiceConnection.PrimaryRecorder.ReliableMode;
			voiceConnection.PrimaryRecorder.ReliableMode = reliableMode;
		}

		public void ToggleAEC()
		{
			voiceAudioPreprocessor.AEC = !voiceAudioPreprocessor.AEC;
		}

		public void ToggleNoiseSuppression()
		{
			voiceAudioPreprocessor.NoiseSuppression = !voiceAudioPreprocessor.NoiseSuppression;
		}

		public void ToggleAGC()
		{
			voiceAudioPreprocessor.AGC = !voiceAudioPreprocessor.AGC;
		}

		public void ToggleVAD()
		{
			voiceAudioPreprocessor.VAD = !voiceAudioPreprocessor.VAD;
		}

		public void UpdateSyncedNickname(string nickname)
		{
			nickname = nickname.Trim();
			if (!string.IsNullOrEmpty(nickname))
			{
				Debug.Log("UpdateSyncedNickname() name: " + nickname);
				voiceConnection.Client.LocalPlayer.NickName = nickname;
				PlayerPrefs.SetString("vNick", nickname);
			}
		}

		public void JoinOrCreateRoom(string roomname)
		{
			roomname = roomname.Trim();
			ConnectAndJoin component = GetComponent<ConnectAndJoin>();
			if (!(component == null))
			{
				Debug.Log("JoinOrCreateRoom() roomname: " + roomname);
				if (string.IsNullOrEmpty(roomname))
				{
					component.RoomName = string.Empty;
					component.RandomRoom = true;
				}
				else
				{
					component.RoomName = roomname;
					component.RandomRoom = false;
				}
				voiceConnection.Client.OpLeaveRoom(false);
			}
		}

		protected void Update()
		{
			connectionStatusText.text = voiceConnection.Client.State.ToString();
			serverStatusText.text = string.Format("{0}/{1}", voiceConnection.Client.CloudRegion, voiceConnection.Client.CurrentServerAddress);
			string text = string.Empty;
			if (voiceConnection.Client.InRoom)
			{
				Dictionary<int, Player>.ValueCollection values = voiceConnection.Client.CurrentRoom.Players.Values;
				if (values != null && values.Count > 1)
				{
					foreach (Player item in values)
					{
						text += item.ToStringFull();
					}
				}
			}
			roomStatusText.text = ((voiceConnection.Client.CurrentRoom != null) ? (voiceConnection.Client.CurrentRoom.Name + " " + text) : string.Empty);
			float num = voiceConnection.PrimaryRecorder.LevelMeter.CurrentAvgAmp;
			if (num > 1f)
			{
				num /= 32768f;
			}
			if ((double)num > 0.1)
			{
				inputWarningText.text = "Input too loud!";
				inputWarningText.color = warningColor;
			}
			else
			{
				inputWarningText.text = string.Empty;
			}
			if (voiceConnection.FramesReceivedPerSecond > 0f)
			{
				packetLossWarningText.text = string.Format("{0}% Packet Loss", voiceConnection.FramesLostPercent);
				packetLossWarningText.color = ((!(voiceConnection.FramesLostPercent > 1f)) ? okColor : warningColor);
			}
			else
			{
				packetLossWarningText.text = "(no data)";
			}
		}

		private void InitToggles()
		{
			if (voiceConnection != null && voiceConnection.PrimaryRecorder != null)
			{
				if (debugEchoToggle != null)
				{
					debugEchoToggle.isOn = voiceConnection.PrimaryRecorder.DebugEchoMode;
				}
				if (reliableTransmissionToggle != null)
				{
					reliableTransmissionToggle.isOn = voiceConnection.PrimaryRecorder.ReliableMode;
				}
			}
			if (!(webRtcDspGameObject != null))
			{
				return;
			}
			if (voiceAudioPreprocessor == null)
			{
				webRtcDspGameObject.SetActive(false);
				return;
			}
			webRtcDspGameObject.SetActive(true);
			if (aecToggle != null)
			{
				aecToggle.isOn = voiceAudioPreprocessor.AEC;
			}
			if (noiseSuppressionToggle != null)
			{
				noiseSuppressionToggle.isOn = voiceAudioPreprocessor.NoiseSuppression;
			}
			if (agcToggle != null)
			{
				agcToggle.isOn = voiceAudioPreprocessor.AGC;
			}
			if (vadToggle != null)
			{
				vadToggle.isOn = voiceAudioPreprocessor.VAD;
			}
		}
	}
}
