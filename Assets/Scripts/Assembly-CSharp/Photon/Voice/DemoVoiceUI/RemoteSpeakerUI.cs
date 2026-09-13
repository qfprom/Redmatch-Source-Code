using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Voice.DemoVoiceUI
{
	[RequireComponent(typeof(Speaker))]
	public class RemoteSpeakerUI : MonoBehaviour
	{
		public Text nameText;

		public Image remoteIsMuting;

		public Image remoteIsTalking;

		private void Update()
		{
			Speaker component = GetComponent<Speaker>();
			if (component.Actor != null)
			{
				string text = component.Actor.NickName;
				if (string.IsNullOrEmpty(text))
				{
					text = "user " + component.Actor.ActorNumber;
				}
				nameText.text = text;
				if (remoteIsMuting != null)
				{
					bool? flag = component.Actor.CustomProperties["mute"] as bool?;
					if (flag.HasValue)
					{
						remoteIsMuting.enabled = flag.Value;
					}
				}
				if (remoteIsTalking != null)
				{
					remoteIsTalking.enabled = component.IsPlaying;
				}
			}
			else
			{
				nameText.text = component.name;
			}
		}
	}
}
