using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindDialogue : MonoBehaviour
{
	[SerializeField]
	private GameObject keyItemPrefab;

	[SerializeField]
	private GameObject keyList;

	[SerializeField]
	private GameObject waitingForKeyScreen;

	[SerializeField]
	private Text waitingForKeyBindText;

	[SerializeField]
	private Dropdown layoutDropdown;

	private string buttonToRebind;

	private Dictionary<string, KeybindListItem> buttonToKeybindListItem;

	private void Start()
	{
		string[] buttonNames = InputManager.IM.GetButtonNames();
		buttonToKeybindListItem = new Dictionary<string, KeybindListItem>();
		foreach (string bn in buttonNames)
		{
			GameObject gameObject = Object.Instantiate(keyItemPrefab);
			gameObject.transform.SetParent(keyList.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.Find("ButtonNameText").GetComponent<Text>().text = bn;
			Text component = gameObject.transform.Find("Button/ButtonKeyNameText").GetComponent<Text>();
			component.text = InputManager.IM.GetKeyNameForButton(bn);
			buttonToKeybindListItem[bn] = gameObject.GetComponent<KeybindListItem>();
			buttonToKeybindListItem[bn].AssignKey(InputManager.IM.GetCustomKeyForButton(bn));
			gameObject.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate
			{
				StartRebind(bn);
			});
		}
		layoutDropdown.value = InputManager.IM.GetLayoutNum();
	}

	public void SwitchLayout(int layout)
	{
		InputManager.IM.SetLayout(layout);
		RenderKeybinds();
	}

	public void ResetKeybinds()
	{
		InputManager.IM.ResetButtons();
		RenderKeybinds();
	}

	private void RenderKeybinds()
	{
		string[] buttonNames = InputManager.IM.GetButtonNames();
		for (int i = 0; i < buttonNames.Length; i++)
		{
			buttonToKeybindListItem[buttonNames[i]].AssignKey(InputManager.IM.GetCustomKeyForButton(buttonNames[i]));
		}
	}

	public void ResetKeybind()
	{
		if (buttonToRebind != null)
		{
			InputManager.IM.ResetButton(buttonToRebind);
			buttonToKeybindListItem[buttonToRebind].AssignKey(InputManager.IM.GetCustomKeyForButton(buttonToRebind));
		}
		buttonToRebind = null;
		InputManager.IM.EnableBoundInput();
		waitingForKeyScreen.SetActive(false);
	}

	public void RemoveBind()
	{
		if (buttonToRebind != null)
		{
			InputManager.IM.SetButton(buttonToRebind, new CustomKey(KeyCode.None));
			buttonToKeybindListItem[buttonToRebind].AssignKey(new CustomKey(KeyCode.None));
		}
		buttonToRebind = null;
		InputManager.IM.EnableBoundInput();
		waitingForKeyScreen.SetActive(false);
	}

	public void CancelBind()
	{
		buttonToRebind = null;
		InputManager.IM.EnableBoundInput();
		waitingForKeyScreen.SetActive(false);
	}

	private void Update()
	{
		if (buttonToRebind != null && InputManager.IM.CurrentKey() != InputManager.IM.noKey)
		{
			InputManager.IM.SetButton(buttonToRebind, InputManager.IM.CurrentKey());
			InputManager.IM.EnableBoundInput();
			buttonToKeybindListItem[buttonToRebind].AssignKey(InputManager.IM.CurrentKey());
			buttonToRebind = null;
			waitingForKeyScreen.SetActive(false);
		}
	}

	private void StartRebind(string buttonName)
	{
		InputManager.IM.DisableBoundInput();
		waitingForKeyBindText.text = string.Format("Binding \"{0}\"", buttonName);
		waitingForKeyScreen.SetActive(true);
		buttonToRebind = buttonName;
	}

	public void DeleteKeybindFiles()
	{
		InputManager.IM.DeleteKeybindFiles();
	}
}
