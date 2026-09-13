using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChannelSelector : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public string Channel;

	public void SetChannel(string channel)
	{
		Channel = channel;
		Text componentInChildren = GetComponentInChildren<Text>();
		componentInChildren.text = Channel;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		ChatGui chatGui = Object.FindObjectOfType<ChatGui>();
		chatGui.ShowChannel(Channel);
	}
}
