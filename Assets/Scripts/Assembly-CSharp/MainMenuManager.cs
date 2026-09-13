using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public void Start()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
