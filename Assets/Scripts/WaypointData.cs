using UnityEngine;
using UnityEngine.UI;

public class WaypointData
{
	public Transform waypointTarget;

	public Image waypointImage;

	public Text waypointText;

	public WaypointManager.WaypointType waypointType;

	public WaypointData(Transform _target, string waypointName, WaypointManager.WaypointType _waypointType, Image _waypointImage, Text _waypointText)
	{
		waypointTarget = _target;
		waypointImage = _waypointImage;
		waypointType = _waypointType;
		waypointText = _waypointText;
		switch (waypointType)
		{
		case WaypointManager.WaypointType.Distance:
			break;
		case WaypointManager.WaypointType.Name:
			waypointText.text = waypointName;
			break;
		default:
			waypointText.gameObject.SetActive(false);
			break;
		}
	}
}
