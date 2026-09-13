using System.Collections.Generic;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Voice.DemoVoiceUI
{
	public class MicrophoneDropdownFiller : MonoBehaviour
	{
		public Recorder recorder;

		public Dropdown micDropdown;

		private List<MicRef> micOptions;

		[SerializeField]
		private GameObject RefreshButton;

		[SerializeField]
		private GameObject ToggleButton;

		private void Start()
		{
			RefreshMicrophones();
		}

		private void SetupMicDropdown()
		{
			micDropdown.ClearOptions();
			micOptions = new List<MicRef>();
			List<string> list = new List<string>();
			string[] devices = Microphone.devices;
			foreach (string arg in devices)
			{
				micOptions.Add(new MicRef(arg));
				list.Add(string.Format("[Unity] {0}", arg));
			}
			if (Recorder.PhotonMicrophoneEnumerator.IsSupported)
			{
				RefreshButton.SetActive(true);
				ToggleButton.SetActive(false);
				for (int j = 0; j < Recorder.PhotonMicrophoneEnumerator.Count; j++)
				{
					string arg2 = Recorder.PhotonMicrophoneEnumerator.NameAtIndex(j);
					micOptions.Add(new MicRef(arg2, Recorder.PhotonMicrophoneEnumerator.IDAtIndex(j)));
					list.Add(string.Format("[Photon] {0}", arg2));
				}
			}
			else
			{
				ToggleButton.SetActive(true);
				RefreshButton.SetActive(!ToggleButton.GetComponentInChildren<Toggle>().isOn);
			}
			micDropdown.AddOptions(list);
			micDropdown.onValueChanged.RemoveAllListeners();
			micDropdown.onValueChanged.AddListener(delegate
			{
				MicDropdownValueChanged(micOptions[micDropdown.value]);
			});
			SetCurrentValue();
		}

		private void MicDropdownValueChanged(MicRef mic)
		{
			recorder.MicrophoneType = mic.MicType;
			switch (mic.MicType)
			{
			case Recorder.MicType.Unity:
				recorder.UnityMicrophoneDevice = mic.Name;
				break;
			case Recorder.MicType.Photon:
				recorder.PhotonMicrophoneDeviceId = mic.PhotonId;
				break;
			}
			if (recorder.IsInitialized && recorder.RequiresInit)
			{
				recorder.ReInit();
			}
		}

		private void SetCurrentValue()
		{
			for (int i = 0; i < micOptions.Count; i++)
			{
				MicRef micRef = micOptions[i];
				if (recorder.MicrophoneType == micRef.MicType)
				{
					if (recorder.MicrophoneType == Recorder.MicType.Unity && micRef.Name.Equals(recorder.UnityMicrophoneDevice))
					{
						micDropdown.value = i;
						break;
					}
					if (recorder.MicrophoneType == Recorder.MicType.Photon && micRef.PhotonId == recorder.PhotonMicrophoneDeviceId)
					{
						micDropdown.value = i;
						break;
					}
				}
			}
		}

		public void PhotonMicToggled(bool on)
		{
			micDropdown.gameObject.SetActive(!on);
			RefreshButton.SetActive(!on);
			if (on)
			{
				recorder.MicrophoneType = Recorder.MicType.Photon;
				recorder.ReInit();
			}
			else
			{
				RefreshMicrophones();
				MicDropdownValueChanged(micOptions[micDropdown.value]);
			}
		}

		public void RefreshMicrophones()
		{
			Debug.Log("Refresh Mics");
			Recorder.PhotonMicrophoneEnumerator.Refresh();
			SetupMicDropdown();
		}
	}
}
