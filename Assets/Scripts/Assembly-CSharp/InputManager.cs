using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	private Dictionary<string, CustomKey> defaultButtonKeys = new Dictionary<string, CustomKey>();

	private Dictionary<string, CustomKey> buttonKeys = new Dictionary<string, CustomKey>();

	[SerializeField]
	private CustomKey[] customKeys;

	public CustomKeyImage[] specialKeyImages;

	private List<KeyCode> keyCodes = new List<KeyCode>();

	private int slotCount = 10;

	private int layoutNum;

	private bool acceptBoundInput = true;

	private string keybindFileName = "keys";

	[HideInInspector]
	public CustomKey noKey = new CustomKey(KeyCode.None);

	public CustomKey[] kaeaetaetaetaett;

	public static InputManager IM { get; private set; }

	private void Awake()
	{
		if (IM == null)
		{
			IM = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			keyCodes.Add(value);
		}
		defaultButtonKeys["Forward"] = new CustomKey(KeyCode.W);
		defaultButtonKeys["Backward"] = new CustomKey(KeyCode.S);
		defaultButtonKeys["Left"] = new CustomKey(KeyCode.A);
		defaultButtonKeys["Right"] = new CustomKey(KeyCode.D);
		defaultButtonKeys["Jump"] = new CustomKey(KeyCode.Space);
		defaultButtonKeys["Sprint"] = new CustomKey(KeyCode.LeftShift);
		defaultButtonKeys["Crouch"] = new CustomKey(KeyCode.LeftControl);
		defaultButtonKeys["Interact"] = new CustomKey(KeyCode.E);
		defaultButtonKeys["Attack"] = new CustomKey(KeyCode.Mouse0);
		defaultButtonKeys["Aim"] = new CustomKey(KeyCode.Mouse1);
		defaultButtonKeys["Reload"] = new CustomKey(KeyCode.R);
		defaultButtonKeys["ChangeFireMode"] = new CustomKey(KeyCode.B);
		defaultButtonKeys["Melee"] = new CustomKey(KeyCode.V);
		defaultButtonKeys["Drop"] = new CustomKey(KeyCode.Q);
		defaultButtonKeys["DropAmmo"] = new CustomKey(KeyCode.T);
		defaultButtonKeys["Inspect"] = new CustomKey(KeyCode.F);
		defaultButtonKeys["Shop"] = new CustomKey(KeyCode.C);
		defaultButtonKeys["Chat"] = new CustomKey(KeyCode.Return);
		defaultButtonKeys["Scoreboard"] = new CustomKey(KeyCode.Tab);
		defaultButtonKeys["NextSlot"] = GetCustomKeyByName("MouseUp");
		defaultButtonKeys["PreviousSlot"] = GetCustomKeyByName("MouseDown");
		defaultButtonKeys["ToggleUI"] = new CustomKey(KeyCode.None);
		for (int i = 1; i <= slotCount; i++)
		{
			defaultButtonKeys["Slot " + i] = ((i <= 9) ? new CustomKey((KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + i)) : new CustomKey(KeyCode.None));
		}
		layoutNum = PlayerPrefs.GetInt("InputManager.Layout");
		if (File.Exists(Application.persistentDataPath + string.Format("/{0}{1}.save", keybindFileName, layoutNum)))
		{
			LoadPreferences();
			return;
		}
		AssignDefaultKeys();
		SavePreferences();
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		CustomKey[] array = customKeys;
		foreach (CustomKey customKey in array)
		{
			customKey.downLastFrame = GetKey(customKey);
		}
	}

	private void AssignDefaultKeys()
	{
		buttonKeys = new Dictionary<string, CustomKey>(defaultButtonKeys);
	}

	public void ResetButtons()
	{
		AssignDefaultKeys();
		SavePreferences();
	}

	public void ResetButton(string buttonName)
	{
		buttonKeys[buttonName] = defaultButtonKeys[buttonName];
		SavePreferences();
	}

	public string[] GetButtonNames()
	{
		return buttonKeys.Keys.ToArray();
	}

	public void SetButton(string buttonName, CustomKey ck)
	{
		buttonKeys[buttonName] = ck;
		SavePreferences();
	}

	public void DisableBoundInput()
	{
		acceptBoundInput = false;
	}

	public void EnableBoundInput()
	{
		acceptBoundInput = true;
	}

	public void SetLayout(int newLayout)
	{
		layoutNum = newLayout;
		PlayerPrefs.SetInt("InputManager.Layout", layoutNum);
		if (File.Exists(Application.persistentDataPath + string.Format("/{0}{1}.save", keybindFileName, layoutNum)))
		{
			LoadPreferences();
			return;
		}
		AssignDefaultKeys();
		SavePreferences();
	}

	public void SavePreferences()
	{
		PreferenceSave preferenceSave = new PreferenceSave();
		preferenceSave.buttonKeys = buttonKeys;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Application.persistentDataPath + string.Format("/{0}{1}.save", keybindFileName, layoutNum));
		binaryFormatter.Serialize(fileStream, preferenceSave);
		fileStream.Close();
	}

	private void LoadPreferences()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(Application.persistentDataPath + string.Format("/{0}{1}.save", keybindFileName, layoutNum), FileMode.Open);
		PreferenceSave preferenceSave = (PreferenceSave)binaryFormatter.Deserialize(fileStream);
		fileStream.Close();
		foreach (string key in defaultButtonKeys.Keys)
		{
			if (preferenceSave.buttonKeys.ContainsKey(key))
			{
				buttonKeys[key] = preferenceSave.buttonKeys[key];
			}
			else
			{
				buttonKeys[key] = defaultButtonKeys[key];
			}
		}
	}

	public void DeleteKeybindFiles()
	{
		for (int i = 0; i < 5; i++)
		{
			if (File.Exists(Application.persistentDataPath + string.Format("/{0}{1}.save", keybindFileName, i)))
			{
				File.Delete(Application.persistentDataPath + string.Format("/{0}{1}.save", keybindFileName, i));
			}
		}
		layoutNum = 0;
		PlayerPrefs.SetInt("InputManager.Layout", 0);
		AssignDefaultKeys();
		SavePreferences();
	}

	public string GetKeyNameForButton(string buttonName)
	{
		if (buttonKeys[buttonName].keyCode == KeyCode.None)
		{
			return buttonKeys[buttonName].altName;
		}
		return buttonKeys[buttonName].keyCode.ToString();
	}

	public string GetFilteredKeyString(string buttonName)
	{
		return "NOT FINISHED... USE <sprite> with TEXTMESHPRO TO GET BUTTON ICON";
	}

	public int GetLayoutNum()
	{
		return layoutNum;
	}

	public CustomKey CurrentKey()
	{
		CustomKey[] array = customKeys;
		foreach (CustomKey customKey in array)
		{
			if (GetKeyDown(customKey))
			{
				return customKey;
			}
		}
		foreach (KeyCode keyCode in keyCodes)
		{
			if (Input.GetKeyDown(keyCode))
			{
				return new CustomKey(keyCode);
			}
		}
		return noKey;
	}

	public CustomKey GetCustomKeyForButton(string buttonName)
	{
		return buttonKeys[buttonName];
	}

	public CustomKey GetCustomKeyByName(string ckName)
	{
		CustomKey[] array = customKeys;
		foreach (CustomKey customKey in array)
		{
			if (customKey.altName == ckName)
			{
				return customKey;
			}
		}
		return noKey;
	}

	public bool GetButton(string buttonName)
	{
		if (!acceptBoundInput)
		{
			return false;
		}
		if (!buttonKeys.ContainsKey(buttonName))
		{
			Debug.LogError("No button found with name " + buttonName);
			return false;
		}
		return GetKey(buttonKeys[buttonName]);
	}

	public bool GetButtonUp(string buttonName)
	{
		if (!acceptBoundInput)
		{
			return false;
		}
		if (!buttonKeys.ContainsKey(buttonName))
		{
			Debug.LogError("No button found with name " + buttonName);
			return false;
		}
		return GetKeyUp(buttonKeys[buttonName]);
	}

	public bool GetButtonDown(string buttonName)
	{
		if (!acceptBoundInput)
		{
			return false;
		}
		if (!buttonKeys.ContainsKey(buttonName))
		{
			Debug.LogError("No button found with name " + buttonName);
			return false;
		}
		return GetKeyDown(buttonKeys[buttonName]);
	}

	public bool GetKey(CustomKey ck)
	{
		if (!string.IsNullOrEmpty(ck.axis))
		{
			if (ck.axisMax > ck.axisMin)
			{
				return Input.GetAxisRaw(ck.axis) > ck.axisMin && Input.GetAxisRaw(ck.axis) <= ck.axisMax;
			}
			return Input.GetAxisRaw(ck.axis) < ck.axisMin && Input.GetAxisRaw(ck.axis) >= ck.axisMax;
		}
		return Input.GetKey(ck.keyCode);
	}

	public bool GetKeyDown(CustomKey ck)
	{
		if (!string.IsNullOrEmpty(ck.axis))
		{
			return GetKey(ck) && !ck.downLastFrame;
		}
		return Input.GetKeyDown(ck.keyCode);
	}

	public bool GetKeyUp(CustomKey ck)
	{
		if (!string.IsNullOrEmpty(ck.axis))
		{
			return !GetKey(ck) && ck.downLastFrame;
		}
		return Input.GetKeyUp(ck.keyCode);
	}

	public int GetAxis(string buttonA, string buttonB)
	{
		return (GetButton(buttonA) ? 1 : 0) + (GetButton(buttonB) ? (-1) : 0);
	}
}
