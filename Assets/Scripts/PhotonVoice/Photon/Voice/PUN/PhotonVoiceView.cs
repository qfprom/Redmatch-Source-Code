using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.PUN
{
	[AddComponentMenu("Photon Voice/Photon Voice View")]
	[RequireComponent(typeof(PhotonView))]
	public class PhotonVoiceView : VoiceComponent
	{
		private PhotonView photonView;

		[SerializeField]
		private Recorder recorderInUse;

		[SerializeField]
		private Speaker speakerInUse;

		public bool AutoCreateRecorderIfNotFound;

		public bool UsePrimaryRecorder;

		public bool SetupDebugSpeaker;

		public Recorder RecorderInUse
		{
			get
			{
				return recorderInUse;
			}
			set
			{
				if (photonView.IsMine)
				{
					if (recorderInUse != value || !IsRecorder)
					{
						recorderInUse = value;
						IsRecorder = SetupRecorder(recorderInUse);
					}
				}
				else if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("No need to set Recorder as the PhotonView does not belong to local player");
				}
			}
		}

		public Speaker SpeakerInUse
		{
			get
			{
				return speakerInUse;
			}
			set
			{
				if (SetupDebugSpeaker || !photonView.IsMine)
				{
					if (speakerInUse != value || !IsSpeaker)
					{
						speakerInUse = value;
						IsSpeaker = SetupSpeaker(speakerInUse);
					}
				}
				else if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Speaker not set because the PhotonView does not belong to a remote player or SetupDebugSpeaker is disabled");
				}
			}
		}

		public bool IsSetup { get; protected set; }

		public bool IsSpeaker { get; protected set; }

		public bool IsSpeaking
		{
			get
			{
				return IsSpeaker && SpeakerInUse.IsPlaying;
			}
		}

		public bool IsRecorder { get; protected set; }

		public bool IsRecording
		{
			get
			{
				return IsRecorder && RecorderInUse.IsCurrentlyTransmitting;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			photonView = GetComponent<PhotonView>();
		}

		private void OnEnable()
		{
			if (photonView.ViewID > 0)
			{
				Setup();
				if (IsSpeaker && !SpeakerInUse.IsLinked)
				{
					PhotonVoiceNetwork.Instance.CheckLateLinking(this, photonView.ViewID);
				}
			}
		}

		private void Setup()
		{
			if (!IsSetup)
			{
				if (photonView.IsMine)
				{
					IsRecorder = SetupRecorder();
				}
				if (SetupDebugSpeaker || !photonView.IsMine)
				{
					IsSpeaker = SetupSpeaker();
				}
				IsSetup = true;
			}
		}

		private bool SetupRecorder()
		{
			if (recorderInUse == null)
			{
				if (UsePrimaryRecorder)
				{
					recorderInUse = PhotonVoiceNetwork.Instance.PrimaryRecorder;
				}
				else
				{
					Recorder[] componentsInChildren = GetComponentsInChildren<Recorder>();
					if (componentsInChildren.Length > 0)
					{
						recorderInUse = componentsInChildren[0];
						if (componentsInChildren.Length > 1 && base.Logger.IsWarningEnabled)
						{
							base.Logger.LogWarning("Multiple Recorder components found attached to the GameObject or its children");
						}
					}
				}
				if (recorderInUse == null)
				{
					if (!AutoCreateRecorderIfNotFound)
					{
						return false;
					}
					recorderInUse = base.gameObject.AddComponent<Recorder>();
				}
			}
			return SetupRecorder(recorderInUse);
		}

		private bool SetupRecorder(Recorder recorder)
		{
			if (recorder == null)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Cannot setup a null Recorder");
				}
				return false;
			}
			if (photonView.ViewID <= 0)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Recorder setup cannot be done before assigning a valid ViewID to the PhotonView attached to the same GameObject as the PhotonVoiceView");
				}
				return false;
			}
			if (recorder.IsInitialized)
			{
				if (recorder.UserData is int && photonView.ViewID == (int)recorder.UserData)
				{
					return true;
				}
				recorder.UserData = photonView.ViewID;
				recorder.ReInit();
				return true;
			}
			RecorderInUse.Init(PhotonVoiceNetwork.Instance.VoiceClient, photonView.ViewID);
			return true;
		}

		private bool SetupSpeaker()
		{
			if (speakerInUse == null)
			{
				Speaker[] componentsInChildren = GetComponentsInChildren<Speaker>();
				if (componentsInChildren.Length > 0)
				{
					speakerInUse = componentsInChildren[0];
					if (componentsInChildren.Length > 1 && base.Logger.IsWarningEnabled)
					{
						base.Logger.LogWarning("Multiple Speaker components found attached to the GameObject or its children");
					}
				}
				if (speakerInUse == null)
				{
					if (!PhotonVoiceNetwork.Instance.AutoCreateSpeakerIfNotFound)
					{
						return false;
					}
					if (!(PhotonVoiceNetwork.Instance.SpeakerPrefab != null))
					{
						speakerInUse = base.gameObject.AddComponent<Speaker>();
						AudioSource componentInChildren = speakerInUse.GetComponentInChildren<AudioSource>();
						componentInChildren.spatialBlend = 1f;
						return true;
					}
					GameObject gameObject = Object.Instantiate(PhotonVoiceNetwork.Instance.SpeakerPrefab, base.transform, false);
					speakerInUse = gameObject.GetComponentInChildren<Speaker>();
					if (speakerInUse == null)
					{
						if (base.Logger.IsErrorEnabled)
						{
							base.Logger.LogError("SpeakerPrefab does not have a component of type Speaker in its hierarchy.");
						}
						Object.Destroy(gameObject);
						return false;
					}
				}
			}
			return SetupSpeaker(speakerInUse);
		}

		private bool SetupSpeaker(Speaker speaker)
		{
			if (speaker == null)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Cannot setup a null Speaker");
				}
				return false;
			}
			AudioSource componentInChildren = speaker.GetComponentInChildren<AudioSource>();
			if (componentInChildren == null)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Unexpected: no AudioSource found attached to the same GameObject as the Speaker component");
				}
				return false;
			}
			return true;
		}
	}
}
