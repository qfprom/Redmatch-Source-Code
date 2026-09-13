using System;
using UnityEngine;

namespace Photon.Voice.Unity
{
	public class UnityAndroidAudioInAEC : IAudioPusher<short>, IAudioDesc, IDisposable
	{
		private class DataCallback : AndroidJavaProxy
		{
			private Action<short[]> callback;

			private IntPtr javaBuf;

			private int cntFrame;

			private int cntShort;

			public DataCallback()
				: base("com.exitgames.photon.audioinaec.AudioInAEC$DataCallback")
			{
			}

			public void SetCallback(Action<short[]> callback, IntPtr javaBuf)
			{
				this.callback = callback;
				this.javaBuf = javaBuf;
			}

			public void OnData()
			{
				if (callback != null)
				{
					short[] array = AndroidJNI.FromShortArray(javaBuf);
					cntFrame++;
					cntShort += array.Length;
					callback(array);
				}
			}

			public void OnStop()
			{
				AndroidJNI.DeleteGlobalRef(javaBuf);
			}
		}

		private AndroidJavaObject audioIn;

		private IntPtr javaBuf;

		private ILogger logger;

		private DataCallback callback;

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
				return 44100;
			}
		}

		public string Error { get; private set; }

		public UnityAndroidAudioInAEC(ILogger logger)
		{
			this.logger = logger;
			try
			{
				callback = new DataCallback();
				audioIn = new AndroidJavaObject("com.exitgames.photon.audioinaec.AudioInAEC");
				bool flag = audioIn.Call<bool>("AECIsAvailable", new object[0]);
				int num = audioIn.Call<int>("GetMinBufferSize", new object[2] { SamplingRate, Channels });
				logger.LogInfo("[PV] UnityAndroidAudioInAEC: AndroidJavaObject created: aecAvailable: {0}, minBufSize: {1}", flag, num);
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				bool flag2 = audioIn.Call<bool>("Start", new object[6]
				{
					androidJavaObject,
					callback,
					SamplingRate,
					Channels,
					num * 4,
					flag
				});
				if (flag2)
				{
					logger.LogInfo("[PV] UnityAndroidAudioInAEC: AndroidJavaObject started: {0}, sampling rate: {1}, channels: {2}, record buffer size: {3}, aec: {4}", flag2, SamplingRate, Channels, num * 4, flag);
				}
				else
				{
					Error = "[PV] UnityAndroidAudioInAEC constructor: calling Start java method failure";
					logger.LogError("[PV] UnityAndroidAudioInAEC: {0}", Error);
				}
			}
			catch (Exception ex)
			{
				Error = ex.ToString();
				if (Error == null)
				{
					Error = "Exception in WindowsAudioInPusher constructor";
				}
				logger.LogError("[PV] UnityAndroidAudioInAEC: {0}", Error);
			}
		}

		public void SetCallback(Action<short[]> callback, ObjectFactory<short[], int> bufferFactory)
		{
			if (Error == null)
			{
				int info = bufferFactory.Info;
				javaBuf = AndroidJNI.NewGlobalRef(AndroidJNI.NewShortArray(info));
				this.callback.SetCallback(callback, javaBuf);
				IntPtr methodID = AndroidJNI.GetMethodID(audioIn.GetRawClass(), "SetBuffer", "([S)Z");
				if (!AndroidJNI.CallBooleanMethod(audioIn.GetRawObject(), methodID, new jvalue[1]
				{
					new jvalue
					{
						l = javaBuf
					}
				}))
				{
					Error = "UnityAndroidAudioInAEC.SetCallback(): calling SetBuffer java method failure";
				}
			}
			if (Error != null)
			{
				logger.LogError("[PV] UnityAndroidAudioInAEC: {0}", Error);
			}
		}

		public void Dispose()
		{
			if (audioIn != null)
			{
				audioIn.Call<bool>("Stop", new object[0]);
			}
		}
	}
}
