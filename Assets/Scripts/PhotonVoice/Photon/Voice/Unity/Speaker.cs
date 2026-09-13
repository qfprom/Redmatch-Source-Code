using System;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Voice.Unity
{
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("Photon Voice/Speaker")]
	public class Speaker : VoiceComponent
	{
		private IAudioOut audioOutput;

		private RemoteVoiceLink remoteVoiceLink;

		private bool started;

		public int PlayDelayMs = 200;

		public bool IsPlaying
		{
			get
			{
				return audioOutput != null && audioOutput.IsPlaying;
			}
		}

		public int Lag
		{
			get
			{
				return (audioOutput == null) ? (-1) : audioOutput.Lag;
			}
		}

		public Action<Speaker> OnRemoteVoiceRemoveAction { get; set; }

		public Player Actor { get; protected internal set; }

		public bool IsLinked
		{
			get
			{
				return remoteVoiceLink != null;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			Func<IAudioOut> func = () => new AudioStreamPlayer(new VoiceLogger(this, "AudioStreamPlayer", base.LogLevel), new UnityAudioOut(GetComponent<AudioSource>()), "PhotonVoiceSpeaker:", base.Logger.IsInfoEnabled);
			audioOutput = func();
			StartPlaying();
		}

		internal void OnRemoteVoiceInfo(RemoteVoiceLink stream)
		{
			if (stream == null)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("RemoteVoiceLink is null, cancelled linking");
				}
			}
			else if (IsLinked)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Speaker already linked to {0}/{1}, cancelled linking to {2}/{3}", remoteVoiceLink.PlayerId, remoteVoiceLink.VoiceId, stream.PlayerId, stream.PlayerId);
				}
			}
			else
			{
				remoteVoiceLink = stream;
				remoteVoiceLink.RemoteVoiceRemoved += OnRemoteVoiceRemove;
				StartPlaying();
			}
		}

		internal void OnRemoteVoiceRemove()
		{
			bool flag = started;
			if (audioOutput != null)
			{
				audioOutput.Stop();
				started = false;
			}
			Actor = null;
			if (OnRemoteVoiceRemoveAction != null)
			{
				OnRemoteVoiceRemoveAction(this);
			}
			if (remoteVoiceLink != null)
			{
				remoteVoiceLink.RemoteVoiceRemoved -= OnRemoteVoiceRemove;
				if (flag)
				{
					remoteVoiceLink.FloatFrameDecoded -= OnAudioFrame;
				}
				remoteVoiceLink = null;
			}
		}

		internal void OnAudioFrame(float[] frame)
		{
			audioOutput.Push(frame);
		}

		private void Update()
		{
			audioOutput.Service();
		}

		private void StartPlaying()
		{
			if (!started && audioOutput != null && IsLinked)
			{
				VoiceInfo info = remoteVoiceLink.Info;
				audioOutput.Start(info.SamplingRate, info.Channels, info.FrameDurationSamples, PlayDelayMs);
				remoteVoiceLink.FloatFrameDecoded += OnAudioFrame;
				started = true;
			}
		}
	}
}
