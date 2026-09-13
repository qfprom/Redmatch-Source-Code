using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace Photon.Voice.Unity
{
	public class AudioOutCapture : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<float[], int> OnAudioFrame__BackingField;

		public event Action<float[], int> OnAudioFrame
		{
			add
			{
				Action<float[], int> action = OnAudioFrame__BackingField;
				Action<float[], int> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnAudioFrame__BackingField, (Action<float[], int>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<float[], int> action = OnAudioFrame__BackingField;
				Action<float[], int> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnAudioFrame__BackingField, (Action<float[], int>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		private void OnAudioFilterRead(float[] frame, int channels)
		{
			if (OnAudioFrame__BackingField != null)
			{
				OnAudioFrame__BackingField(frame, channels);
			}
		}
	}
}
