using System;
using System.Diagnostics;
using System.Threading;

namespace Photon.Voice.Unity
{
	public class RemoteVoiceLink
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<float[]> FloatFrameDecoded__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action RemoteVoiceRemoved__BackingField;

		public VoiceInfo Info { get; private set; }

		public int PlayerId { get; private set; }

		public int VoiceId { get; private set; }

		public int ChannelId { get; private set; }

		public event Action<float[]> FloatFrameDecoded
		{
			add
			{
				Action<float[]> action = FloatFrameDecoded__BackingField;
				Action<float[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref FloatFrameDecoded__BackingField, (Action<float[]>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<float[]> action = FloatFrameDecoded__BackingField;
				Action<float[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref FloatFrameDecoded__BackingField, (Action<float[]>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public event Action RemoteVoiceRemoved
		{
			add
			{
				Action action = RemoteVoiceRemoved__BackingField;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref RemoteVoiceRemoved__BackingField, (Action)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action action = RemoteVoiceRemoved__BackingField;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref RemoteVoiceRemoved__BackingField, (Action)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public RemoteVoiceLink(VoiceInfo info, int playerId, int voiceId, int channelId, ref RemoteVoiceOptions options)
		{
			Info = info;
			PlayerId = playerId;
			VoiceId = voiceId;
			ChannelId = channelId;
			options.OnDecodedFrameFloatAction = OnDecodedFrameFloatAction;
			options.OnRemoteVoiceRemoveAction = OnRemoteVoiceRemoveAction;
		}

		private void OnRemoteVoiceRemoveAction()
		{
			if (RemoteVoiceRemoved__BackingField != null)
			{
				RemoteVoiceRemoved__BackingField();
			}
		}

		private void OnDecodedFrameFloatAction(float[] floats)
		{
			if (FloatFrameDecoded__BackingField != null)
			{
				FloatFrameDecoded__BackingField(floats);
			}
		}
	}
}
