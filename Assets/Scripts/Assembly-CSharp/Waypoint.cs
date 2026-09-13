using UnityEngine;

public class Waypoint : MonoBehaviour
{
	[SerializeField]
	private Sprite waypointSprite;

	[SerializeField]
	private string waypointName;

	[SerializeField]
	private WaypointManager.WaypointType waypointType;

	private void Start()
	{
		WaypointManager.Instance.RequestWaypoint(base.transform, waypointName, waypointSprite, waypointType);
	}
}
