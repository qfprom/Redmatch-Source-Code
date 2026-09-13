using UnityEngine;

public class ErrorMessageManager : MonoBehaviour
{
	private int messageCount;

	private void OnEnable()
	{
		Application.logMessageReceived += LogMessage;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= LogMessage;
	}

	public void LogMessage(string message, string stackTrace, LogType type)
	{
		if ((type == LogType.Exception || type == LogType.Error) && SettingsManager.settings.developerMode)
		{
			GameSetup.gameSetup.SendGameInfo("<size=12>" + message + ((messageCount % 2 != 0) ? "\n<color=#f49242>" : "\n<color=red>") + stackTrace + "</color></size>");
		}
		messageCount++;
	}
}
