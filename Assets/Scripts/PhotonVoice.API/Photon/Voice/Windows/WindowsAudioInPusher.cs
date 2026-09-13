using System;
using System.Runtime.InteropServices;

namespace Photon.Voice.Windows
{
	public class WindowsAudioInPusher : IAudioPusher<short>, IAudioDesc, IDisposable
	{
		private enum SystemMode
		{
			SINGLE_CHANNEL_AEC = 0,
			OPTIBEAM_ARRAY_ONLY = 2,
			OPTIBEAM_ARRAY_AND_AEC = 4,
			SINGLE_CHANNEL_NSAGC = 5
		}

		private IntPtr handle;

		private Action<short[]> pushCallback;

		private ObjectFactory<short[], int> bufferFactory;

		private Action<IntPtr, int> pushRef;

		public int Channels
		{
			get
			{
				return 1;
			}
		}

		public int SamplingRate
		{
			get
			{
				return 16000;
			}
		}

		public string Error { get; private set; }

		public WindowsAudioInPusher(int deviceID, ILogger logger)
		{
			pushRef = push;
			try
			{
				handle = Photon_Audio_In_Create(SystemMode.SINGLE_CHANNEL_AEC, deviceID, -1, pushRef, true, true, true, true);
			}
			catch (Exception ex)
			{
				Error = ex.ToString();
				if (Error == null)
				{
					Error = "Exception in WindowsAudioInPusher constructor";
				}
				logger.LogError("[PV] WindowsAudioInPusher: " + Error);
			}
		}

		[DllImport("AudioIn")]
		private static extern IntPtr Photon_Audio_In_Create(SystemMode systemMode, int micDevIdx, int spkDevIdx, Action<IntPtr, int> callback, bool featrModeOn, bool noiseSup, bool agc, bool cntrClip);

		[DllImport("AudioIn")]
		private static extern void Photon_Audio_In_Destroy(IntPtr handler);

		public void SetCallback(Action<short[]> callback, ObjectFactory<short[], int> bufferFactory)
		{
			pushCallback = callback;
			this.bufferFactory = bufferFactory;
		}

		private void push(IntPtr buf, int lenBytes)
		{
			if (pushCallback != null)
			{
				int num = lenBytes / 2;
				short[] array = bufferFactory.New(num);
				Marshal.Copy(buf, array, 0, num);
				pushCallback(array);
			}
		}

		public void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Photon_Audio_In_Destroy(handle);
			}
		}
	}
}
