using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	public static SettingsManager settings;

	[SerializeField]
	private AudioMixer audioMixer;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Text masterVolumeText;

	[SerializeField]
	private Text voiceVolumeText;

	[SerializeField]
	private Text musicVolumeText;

	[SerializeField]
	private Text sensitivityText;

	[SerializeField]
	private Slider masterVolumeSlider;

	[SerializeField]
	private Slider voiceVolumeSlider;

	[SerializeField]
	private Slider musicVolumeSlider;

	[SerializeField]
	private Slider sensitivitySlider;

	[SerializeField]
	private Toggle transmitToggle;

	[SerializeField]
	private Toggle showGoreToggle;

	[SerializeField]
	private Toggle developerModeToggle;

	[SerializeField]
	private Toggle showRoomNotificationsToggle;

	[SerializeField]
	private Toggle blurToggle;

	[SerializeField]
	private Image[] blurImages;

	[SerializeField]
	private Material blurMaterial;

	[SerializeField]
	private Dropdown qualityDropdown;

	[SerializeField]
	private Dropdown resolutionDropdown;

	[SerializeField]
	private Dropdown fullscreenDropdown;

	[HideInInspector]
	public float sensitivity;

	[HideInInspector]
	public bool showGore;

	[HideInInspector]
	public bool developerMode;

	[HideInInspector]
	public bool showRoomNotifications;

	[HideInInspector]
	public bool blur;

	private bool hiddenUI;

	private Resolution[] resolutions;

	private void Awake()
	{
		settings = this;
	}

	private void Start()
	{
		resolutions = Screen.resolutions.Select((Resolution resolution2) => new Resolution
		{
			width = resolution2.width,
			height = resolution2.height
		}).Distinct().ToArray();
		Array.Reverse(resolutions);
		LoadSettings();
		List<string> list = new List<string>();
		int value = 0;
		int num = 0;
		Resolution[] array = resolutions;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			Resolution resolution = array[num2];
			list.Add(resolution.width + " x " + resolution.height);
			if (resolution.width == Screen.width && resolution.height == Screen.height)
			{
				value = num;
			}
			num++;
		}
		resolutionDropdown.AddOptions(list);
		resolutionDropdown.value = value;
		resolutionDropdown.RefreshShownValue();
		string[] names = QualitySettings.names;
		qualityDropdown.AddOptions(names.ToList());
		qualityDropdown.value = QualitySettings.GetQualityLevel();
		qualityDropdown.RefreshShownValue();
		string[] names2 = Enum.GetNames(typeof(FullScreenMode));
		fullscreenDropdown.AddOptions(names2.ToList());
		int value2 = 0;
		int num3 = 0;
		foreach (FullScreenMode value3 in Enum.GetValues(typeof(FullScreenMode)))
		{
			if (value3 == Screen.fullScreenMode)
			{
				value2 = num3;
				break;
			}
			num3++;
		}
		fullscreenDropdown.value = value2;
		fullscreenDropdown.RefreshShownValue();
	}

	private void Update()
	{
		if (InputManager.IM.GetButtonDown("ToggleUI"))
		{
			hiddenUI = !hiddenUI;
			canvasGroup.alpha = ((!hiddenUI) ? 1 : 0);
		}
	}

	private void LoadSettings()
	{
		if (PlayerPrefs.HasKey("Settings.Sensitivity"))
		{
			sensitivity = PlayerPrefs.GetFloat("Settings.Sensitivity");
		}
		else
		{
			sensitivity = 150f;
			PlayerPrefs.SetFloat("Settings.Sensitivity", sensitivity);
		}
		if (PlayerPrefs.HasKey("Settings.VoiceChatVolume"))
		{
			audioMixer.SetFloat("voiceChatVolume", PlayerPrefs.GetFloat("Settings.VoiceChatVolume"));
		}
		else
		{
			audioMixer.SetFloat("voiceChatVolume", 0f);
			PlayerPrefs.SetFloat("Settings.VoiceChatVolume", 0f);
		}
		if (PlayerPrefs.HasKey("Settings.MasterVolume"))
		{
			audioMixer.SetFloat("masterVolume", PlayerPrefs.GetFloat("Settings.MasterVolume"));
		}
		else
		{
			audioMixer.SetFloat("masterVolume", 0f);
			PlayerPrefs.SetFloat("Settings.MasterVolume", 0f);
		}
		if (PlayerPrefs.HasKey("Settings.MusicVolume"))
		{
			audioMixer.SetFloat("musicVolume", PlayerPrefs.GetFloat("Settings.MusicVolume"));
		}
		else
		{
			audioMixer.SetFloat("musicVolume", 0f);
			PlayerPrefs.SetFloat("Settings.MusicVolume", 0f);
		}
		if (PlayerPrefs.HasKey("Settings.TransmitVoice"))
		{
			if ((bool)GameSetup.gameSetup)
			{
				GameSetup.gameSetup.voiceRecorder.TransmitEnabled = PlayerPrefs.GetInt("Settings.TransmitVoice") == 1;
			}
			PlayerPrefs.SetInt("Settings.TransmitVoice", 0);
		}
		else
		{
			GameSetup.gameSetup.voiceRecorder.TransmitEnabled = false;
			PlayerPrefs.SetInt("Settings.TransmitVoice", 0);
		}
		if (PlayerPrefs.HasKey("Settings.ShowGore"))
		{
			showGore = PlayerPrefs.GetInt("Settings.ShowGore") == 1;
		}
		else
		{
			showGore = true;
			PlayerPrefs.SetFloat("Settings.ShowGore", 1f);
		}
		if (PlayerPrefs.HasKey("Settings.DeveloperMode"))
		{
			developerMode = PlayerPrefs.GetInt("Settings.DeveloperMode") == 1;
		}
		else
		{
			developerMode = false;
			PlayerPrefs.SetFloat("Settings.DeveloperMode", 0f);
		}
		if (PlayerPrefs.HasKey("Settings.ShowRoomNotifications"))
		{
			showRoomNotifications = PlayerPrefs.GetInt("Settings.ShowRoomNotifications") == 1;
		}
		else
		{
			showRoomNotifications = true;
			PlayerPrefs.SetFloat("Settings.ShowRoomNotifications", 1f);
		}
		if (PlayerPrefs.HasKey("Settings.Blur"))
		{
			if (PlayerPrefs.GetInt("Settings.Blur") == 0)
			{
				Image[] array = blurImages;
				foreach (Image image in array)
				{
					image.material = null;
				}
			}
			blur = PlayerPrefs.GetInt("Settings.Blur") == 1;
		}
		else
		{
			blur = true;
			PlayerPrefs.SetFloat("Settings.Blur", 1f);
		}
		if (PlayerPrefs.HasKey("Settings.Quality"))
		{
			QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Settings.Quality"));
		}
		else
		{
			PlayerPrefs.SetInt("Settings.Quality", QualitySettings.GetQualityLevel());
		}
		if (PlayerPrefs.HasKey("Settings.Resolution"))
		{
			Screen.SetResolution(resolutions[PlayerPrefs.GetInt("Settings.Resolution")].width, resolutions[PlayerPrefs.GetInt("Settings.Resolution")].height, Screen.fullScreen);
		}
		else
		{
			int num = 0;
			Resolution[] array2 = resolutions;
			for (int j = 0; j < array2.Length; j++)
			{
				Resolution resolution = array2[j];
				if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
				{
					PlayerPrefs.SetInt("Settings.Resolution", num);
					break;
				}
				num++;
			}
		}
		if (PlayerPrefs.HasKey("Settings.Fullscreen"))
		{
			int num2 = 0;
			foreach (FullScreenMode value in Enum.GetValues(typeof(FullScreenMode)))
			{
				if (num2 == PlayerPrefs.GetInt("Settings.Fullscreen"))
				{
					Screen.fullScreenMode = value;
					break;
				}
				num2++;
			}
		}
		else
		{
			int num3 = 0;
			foreach (FullScreenMode value2 in Enum.GetValues(typeof(FullScreenMode)))
			{
				if (value2 == Screen.fullScreenMode)
				{
					PlayerPrefs.SetInt("Settings.Fullscreen", num3);
					break;
				}
				num3++;
			}
		}
		DelegateSettings();
		UpdateUI();
	}

	private void DelegateSettings()
	{
		masterVolumeSlider.onValueChanged.AddListener(delegate
		{
			OnMasterVolumeSliderChanged();
		});
		voiceVolumeSlider.onValueChanged.AddListener(delegate
		{
			OnVoiceVolumeSliderChanged();
		});
		musicVolumeSlider.onValueChanged.AddListener(delegate
		{
			OnMusicVolumeSliderChanged();
		});
		developerModeToggle.onValueChanged.AddListener(delegate
		{
			OnDeveloperModeToggleChanged();
		});
		showGoreToggle.onValueChanged.AddListener(delegate
		{
			OnShowGoreToggleChanged();
		});
		transmitToggle.onValueChanged.AddListener(delegate
		{
			OnTransmitToggleChanged();
		});
		showRoomNotificationsToggle.onValueChanged.AddListener(delegate
		{
			OnRoomNotificationToggleChanged();
		});
		blurToggle.onValueChanged.AddListener(delegate
		{
			OnBlurToggleChanged();
		});
		sensitivitySlider.onValueChanged.AddListener(delegate
		{
			OnSensitivitySliderChanged();
		});
		qualityDropdown.onValueChanged.AddListener(delegate
		{
			OnQualityChanged();
		});
		resolutionDropdown.onValueChanged.AddListener(delegate
		{
			OnResolutionChanged();
		});
		fullscreenDropdown.onValueChanged.AddListener(delegate
		{
			OnFullscreenChanged();
		});
	}

	private void UpdateUI()
	{
		sensitivitySlider.value = sensitivity;
		sensitivityText.text = "Sensitivity: " + sensitivity.ToString("0");
		float value;
		audioMixer.GetFloat("voiceChatVolume", out value);
		voiceVolumeSlider.value = value;
		voiceVolumeText.text = "Voice Volume: " + (100f - (value - voiceVolumeSlider.maxValue) / (voiceVolumeSlider.minValue - voiceVolumeSlider.maxValue) * 100f).ToString("0.0") + "%";
		float value2;
		audioMixer.GetFloat("masterVolume", out value2);
		masterVolumeSlider.value = value2;
		masterVolumeText.text = "Master Volume: " + (100f - (value2 - masterVolumeSlider.maxValue) / (masterVolumeSlider.minValue - masterVolumeSlider.maxValue) * 100f).ToString("0.0") + "%";
		float value3;
		audioMixer.GetFloat("musicVolume", out value3);
		musicVolumeSlider.value = value3;
		musicVolumeText.text = "Music Volume: " + (100f - (value3 - musicVolumeSlider.maxValue) / (musicVolumeSlider.minValue - musicVolumeSlider.maxValue) * 100f).ToString("0.0") + "%";
		if ((bool)GameSetup.gameSetup)
		{
			transmitToggle.isOn = GameSetup.gameSetup.voiceRecorder.TransmitEnabled;
		}
		developerModeToggle.isOn = developerMode;
		blurToggle.isOn = blur;
		showGoreToggle.isOn = showGore;
		canvasGroup.alpha = ((!hiddenUI) ? 1 : 0);
	}

	public void OnSensitivitySliderChanged()
	{
		sensitivity = sensitivitySlider.value;
		sensitivityText.text = "Sensitivity: " + sensitivity.ToString("0");
		PlayerPrefs.SetFloat("Settings.Sensitivity", sensitivity);
	}

	public void OnVoiceVolumeSliderChanged()
	{
		audioMixer.SetFloat("voiceChatVolume", voiceVolumeSlider.value);
		voiceVolumeText.text = "Voice Volume: " + (100f - (voiceVolumeSlider.value - voiceVolumeSlider.maxValue) / (voiceVolumeSlider.minValue - voiceVolumeSlider.maxValue) * 100f).ToString("0.0") + "%";
		PlayerPrefs.SetFloat("Settings.VoiceChatVolume", voiceVolumeSlider.value);
	}

	public void OnMasterVolumeSliderChanged()
	{
		audioMixer.SetFloat("masterVolume", masterVolumeSlider.value);
		masterVolumeText.text = "Master Volume: " + (100f - (masterVolumeSlider.value - masterVolumeSlider.maxValue) / (masterVolumeSlider.minValue - masterVolumeSlider.maxValue) * 100f).ToString("0.0") + "%";
		PlayerPrefs.SetFloat("Settings.MasterVolume", masterVolumeSlider.value);
	}

	public void OnMusicVolumeSliderChanged()
	{
		audioMixer.SetFloat("musicVolume", musicVolumeSlider.value);
		musicVolumeText.text = "Music Volume: " + (100f - (musicVolumeSlider.value - musicVolumeSlider.maxValue) / (musicVolumeSlider.minValue - musicVolumeSlider.maxValue) * 100f).ToString("0.0") + "%";
		PlayerPrefs.SetFloat("Settings.MusicVolume", musicVolumeSlider.value);
	}

	public void OnTransmitToggleChanged()
	{
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.voiceRecorder.TransmitEnabled = transmitToggle.isOn;
		}
		PlayerPrefs.SetInt("Settings.TransmitVoice", transmitToggle.isOn ? 1 : 0);
	}

	public void OnShowGoreToggleChanged()
	{
		showGore = showGoreToggle.isOn;
		PlayerPrefs.SetInt("Settings.ShowGore", showGoreToggle.isOn ? 1 : 0);
	}

	public void OnDeveloperModeToggleChanged()
	{
		developerMode = developerModeToggle.isOn;
		PlayerPrefs.SetInt("Settings.DeveloperMode", developerModeToggle.isOn ? 1 : 0);
	}

	public void OnRoomNotificationToggleChanged()
	{
		showRoomNotifications = showRoomNotificationsToggle.isOn;
		PlayerPrefs.SetInt("Settings.ShowRoomNotifications", showRoomNotificationsToggle.isOn ? 1 : 0);
	}

	public void OnBlurToggleChanged()
	{
		Image[] array = blurImages;
		foreach (Image image in array)
		{
			image.material = ((!blurToggle.isOn) ? null : blurMaterial);
		}
		PlayerPrefs.SetInt("Settings.Blur", blurToggle.isOn ? 1 : 0);
	}

	public void OnQualityChanged()
	{
		QualitySettings.SetQualityLevel(qualityDropdown.value);
		PlayerPrefs.SetInt("Settings.Quality", qualityDropdown.value);
	}

	public void OnResolutionChanged()
	{
		Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, Screen.fullScreen);
		PlayerPrefs.SetInt("Settings.Resolution", resolutionDropdown.value);
	}

	public void OnFullscreenChanged()
	{
		int num = 0;
		foreach (FullScreenMode value in Enum.GetValues(typeof(FullScreenMode)))
		{
			if (num == fullscreenDropdown.value)
			{
				Screen.fullScreenMode = value;
				PlayerPrefs.SetInt("Settings.Fullscreen", num);
				break;
			}
			num++;
		}
	}
}
