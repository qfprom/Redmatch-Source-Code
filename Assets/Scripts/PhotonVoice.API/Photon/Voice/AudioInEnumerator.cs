using System;
using System.Runtime.InteropServices;

namespace Photon.Voice
{
	public class AudioInEnumerator : IDisposable
	{
		private const string lib_name = "AudioIn";

		private IntPtr handle;

		public readonly bool IsSupported = true;

		public string Error { get; private set; }

		public int Count
		{
			get
			{
				return (Error == null) ? Photon_Audio_In_MicEnumerator_Count(handle) : 0;
			}
		}

		public AudioInEnumerator(ILogger logger)
		{
			Refresh();
			if (Error != null)
			{
				logger.LogError("[PV] AudioInEnumerator: " + Error);
			}
		}

		[DllImport("AudioIn")]
		private static extern IntPtr Photon_Audio_In_CreateMicEnumerator();

		[DllImport("AudioIn")]
		private static extern void Photon_Audio_In_DestroyMicEnumerator(IntPtr handle);

		[DllImport("AudioIn")]
		private static extern int Photon_Audio_In_MicEnumerator_Count(IntPtr handle);

		[DllImport("AudioIn")]
		private static extern IntPtr Photon_Audio_In_MicEnumerator_NameAtIndex(IntPtr handle, int idx);

		[DllImport("AudioIn")]
		private static extern int Photon_Audio_In_MicEnumerator_IDAtIndex(IntPtr handle, int idx);

		public void Refresh()
		{
			Dispose();
			try
			{
				handle = Photon_Audio_In_CreateMicEnumerator();
				Error = null;
			}
			catch (Exception ex)
			{
				Error = ex.ToString();
				if (Error == null)
				{
					Error = "Exception in AudioInEnumerator.Refresh()";
				}
			}
		}

		public string NameAtIndex(int idx)
		{
			return (Error != null) ? string.Empty : Marshal.PtrToStringAuto(Photon_Audio_In_MicEnumerator_NameAtIndex(handle, idx));
		}

		public int IDAtIndex(int idx)
		{
			return (Error != null) ? (-2) : Photon_Audio_In_MicEnumerator_IDAtIndex(handle, idx);
		}

		public bool IDIsValid(int id)
		{
			return id >= -1;
		}

		public void Dispose()
		{
			if (handle != IntPtr.Zero && Error == null)
			{
				Photon_Audio_In_DestroyMicEnumerator(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
