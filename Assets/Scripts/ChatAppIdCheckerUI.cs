using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ChatAppIdCheckerUI : MonoBehaviour
{
	public Text Description;

	public void Update()
	{
		if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat))
		{
			Description.text = "<Color=Red>WARNING:</Color>\nPlease setup a Chat AppId in the PhotonServerSettings file.";
		}
		else
		{
			Description.text = string.Empty;
		}
	}
}
