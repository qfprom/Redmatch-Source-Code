using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PleaseDontCheat : MonoBehaviour
{
	private SafeFloat nttc;

	private SafeFloat cd;

	public static PleaseDontCheat Instance;

	private int bullets = 2;

	private int index = 5;

	private int test = 6;

	private void Awake()
	{
		Instance = this;
		nttc = new SafeFloat(5f);
		cd = new SafeFloat(30f);
		PleaseDontCheat[] array = Object.FindObjectsOfType<PleaseDontCheat>();
		PleaseDontCheat[] array2 = array;
		foreach (PleaseDontCheat pleaseDontCheat in array2)
		{
			if (pleaseDontCheat != this)
			{
				Object.Destroy(pleaseDontCheat.gameObject);
			}
		}
		Object.DontDestroyOnLoad(this);
		if (PlayerPrefs.GetInt("UNFRIENDLY") == 1)
		{
			NotFriendlyAction();
		}
		Invoke("TestNotFriendlyAccAction", 5f);
	}

	private void FixedUpdate()
	{
		if (!(Time.unscaledTime >= nttc.GetValue()))
		{
			return;
		}
		nttc = new SafeFloat(Time.unscaledTime + cd.GetValue());
		Process[] processes = Process.GetProcesses();
		Process[] array = processes;
		foreach (Process process in array)
		{
			try
			{
				if (process.ProcessName.Contains("cheatengine"))
				{
					NotFriendlyAction();
				}
			}
			catch
			{
			}
		}
	}

	public void Help()
	{
		bullets--;
		index--;
		test += 5;
		if (bullets <= 0)
		{
			NotFriendlyAction();
		}
	}

	private void TestNotFriendlyAccAction()
	{
		if (UserDataManager.IsDataReady)
		{
			if (UserDataManager.Instance.GetProperty("UNFRIENDLY") == "1")
			{
				NotFriendlyAction();
			}
		}
		else
		{
			Invoke("TestNotFriendlyAccAction", 10f);
		}
	}

	private void NotFriendlyAction()
	{
		if ((bool)GameSetup.gameSetup)
		{
			GameSetup.gameSetup.DisconnectPlayerAndDontLoad();
		}
		if (UserDataManager.IsDataReady)
		{
			UserDataManager.Instance.SetProperty("UNFRIENDLY", 1);
		}
		PlayerPrefs.SetInt("UNFRIENDLY", 1);
		SceneManager.LoadScene("Unfriendly");
	}
}
