using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoPunVoice
{
	[RequireComponent(typeof(Canvas))]
	public class Highlighter : MonoBehaviour
	{
		private Canvas canvas;

		private PhotonVoiceView photonVoiceView;

		[SerializeField]
		private Image recorderSprite;

		[SerializeField]
		private Image speakerSprite;

		[SerializeField]
		private Text bufferLagText;

		private bool showSpeakerLag;

		private void OnEnable()
		{
			ChangePOV.CameraChanged += ChangePOV_CameraChanged;
			VoiceDemoUI.DebugToggled += VoiceDemoUI_DebugToggled;
		}

		private void OnDisable()
		{
			ChangePOV.CameraChanged -= ChangePOV_CameraChanged;
			VoiceDemoUI.DebugToggled -= VoiceDemoUI_DebugToggled;
		}

		private void VoiceDemoUI_DebugToggled(bool debugMode)
		{
			showSpeakerLag = debugMode;
		}

		private void ChangePOV_CameraChanged(Camera camera)
		{
			canvas.worldCamera = camera;
		}

		private void Awake()
		{
			canvas = GetComponent<Canvas>();
			if (canvas != null && canvas.worldCamera == null)
			{
				canvas.worldCamera = Camera.main;
			}
			photonVoiceView = GetComponentInParent<PhotonVoiceView>();
		}

		private void Update()
		{
			recorderSprite.enabled = photonVoiceView.IsRecording;
			speakerSprite.enabled = photonVoiceView.IsSpeaking;
			bufferLagText.enabled = showSpeakerLag && photonVoiceView.IsSpeaking;
			if (bufferLagText.enabled)
			{
				bufferLagText.text = string.Format("{0}", photonVoiceView.SpeakerInUse.Lag);
			}
		}

		private void LateUpdate()
		{
			if (!(canvas == null) && !(canvas.worldCamera == null))
			{
				base.transform.rotation = Quaternion.Euler(0f, canvas.worldCamera.transform.eulerAngles.y, 0f);
			}
		}
	}
}
