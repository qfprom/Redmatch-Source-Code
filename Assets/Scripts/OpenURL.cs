using UnityEngine;

public class OpenURL : MonoBehaviour
{
	public void OpenExternalURL(string url)
	{
		Application.OpenURL(url);
	}
}
