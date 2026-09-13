using System;
using System.Linq;
using UnityEngine;

namespace Photon.Voice.Unity
{
	public class MicWrapper : IAudioReader<float>, IDataReader<float>, IAudioDesc, IDisposable
	{
		private AudioClip mic;

		private string device;

		private int micPrevPos;

		private int micLoopCnt;

		private int readAbsPos;

		public int SamplingRate
		{
			get
			{
				return (Error == null) ? mic.frequency : 0;
			}
		}

		public int Channels
		{
			get
			{
				return (Error == null) ? mic.channels : 0;
			}
		}

		public string Error { get; private set; }

		public MicWrapper(string device, int suggestedFrequency, ILogger logger)
		{
			try
			{
				this.device = device;
				if (Microphone.devices.Length < 1)
				{
					Error = "No microphones found (Microphone.devices is empty)";
					logger.LogError("[PV] MicWrapper: " + Error);
					return;
				}
				if (!string.IsNullOrEmpty(device) && !Microphone.devices.Contains(device))
				{
					logger.LogError(string.Format("[PV] MicWrapper: \"{0}\" is not a valid Unity microphone device, falling back to default one", device));
					device = null;
				}
				logger.LogInfo("[PV] MicWrapper: initializing microphone '{0}', suggested frequency = {1}).", device, suggestedFrequency);
				int minFreq;
				int maxFreq;
				Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
				int frequency = suggestedFrequency;
				if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
				{
					logger.LogWarning("[PV] MicWrapper does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}", suggestedFrequency, minFreq, maxFreq);
					frequency = maxFreq;
				}
				mic = Microphone.Start(device, true, 1, frequency);
				logger.LogInfo("[PV] MicWrapper: microphone '{0}' initialized, frequency = {1}, channels = {2}.", device, mic.frequency, mic.channels);
			}
			catch (Exception ex)
			{
				Error = ex.ToString();
				if (Error == null)
				{
					Error = "Exception in MicWrapper constructor";
				}
				logger.LogError("[PV] MicWrapper: " + Error);
			}
		}

		public void Dispose()
		{
			Microphone.End(device);
		}

		public bool Read(float[] buffer)
		{
			if (Error != null)
			{
				return false;
			}
			int position = Microphone.GetPosition(device);
			if (position < micPrevPos)
			{
				micLoopCnt++;
			}
			micPrevPos = position;
			int num = micLoopCnt * mic.samples + position;
			int num2 = buffer.Length / mic.channels;
			int num3 = readAbsPos + num2;
			if (num3 < num)
			{
				mic.GetData(buffer, readAbsPos % mic.samples);
				readAbsPos = num3;
				return true;
			}
			return false;
		}
	}
}
