using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaypointManager : MonoBehaviour
{
	public enum WaypointType
	{
		None = 0,
		Distance = 1,
		Name = 2
	}

	public static WaypointManager Instance;

	[SerializeField]
	private GameObject waypointPrefab;

	[SerializeField]
	private Transform waypointContainer;

	[SerializeField]
	private Sprite defaultSprite;

	private List<WaypointData> waypoints = new List<WaypointData>();

	private List<WaypointData> purgeWaypoints = new List<WaypointData>();

	private Camera currentCamera;

	private void Awake()
	{
		Instance = this;
	}

	private void CameraCheck()
	{
		if (!currentCamera || !currentCamera.gameObject.activeSelf || !currentCamera.enabled)
		{
			Camera[] allCameras = Camera.allCameras;
			if (allCameras.Length > 0)
			{
				currentCamera = allCameras[0];
			}
		}
	}

	private void LateUpdate()
	{
		CameraCheck();
		foreach (WaypointData waypoint in waypoints)
		{
			if (waypoint.waypointTarget == null)
			{
				purgeWaypoints.Add(waypoint);
				continue;
			}
			Vector2 vector = Vector2.one * 20f;
			Vector2 vector2 = currentCamera.WorldToViewportPoint(waypoint.waypointTarget.position);
			bool flag = false;
			if (vector2.x > 0f && vector2.x < 1f && vector2.y > 0f && vector2.y < 1f)
			{
				flag = true;
			}
			float num = waypoint.waypointImage.GetPixelAdjustedRect().width / 2f + vector.x;
			float num2 = (float)Screen.width - num;
			float num3 = waypoint.waypointImage.GetPixelAdjustedRect().height / 2f + vector.y;
			float num4 = (float)Screen.height - num3;
			Vector2 vector3 = currentCamera.WorldToScreenPoint(waypoint.waypointTarget.position);
			if (Vector3.Dot(waypoint.waypointTarget.position - currentCamera.transform.position, currentCamera.transform.forward) < 0f)
			{
				if (vector3.x < (float)(Screen.width / 2))
				{
					vector3.x = num2;
				}
				else
				{
					vector3.x = num;
				}
				if (vector3.y < (float)(Screen.height / 2))
				{
					vector3.y = num4;
				}
				else
				{
					vector3.y = num3;
				}
				flag = false;
			}
			vector3.x = Mathf.Clamp(vector3.x, num, num2);
			vector3.y = Mathf.Clamp(vector3.y, num3, num4);
			waypoint.waypointImage.transform.position = vector3;
			if (flag)
			{
				waypoint.waypointImage.rectTransform.sizeDelta = Vector2.one * 40f;
			}
			else
			{
				waypoint.waypointImage.rectTransform.sizeDelta = Vector2.one * 25f;
			}
			WaypointType waypointType = waypoint.waypointType;
			if (waypointType == WaypointType.Distance)
			{
				waypoint.waypointText.text = Vector3.Distance(waypoint.waypointTarget.position, currentCamera.transform.position).ToString("0");
			}
		}
		bool flag2 = false;
		foreach (WaypointData purgeWaypoint in purgeWaypoints)
		{
			ResolveWaypoint(purgeWaypoint);
			flag2 = true;
		}
		if (flag2)
		{
			purgeWaypoints.Clear();
		}
	}

	public void RequestWaypoint(Transform waypointTarget, string waypointName, Sprite waypointSprite, WaypointType waypointType = WaypointType.None)
	{
		Image component = Object.Instantiate(waypointPrefab, waypointContainer).GetComponent<Image>();
		component.sprite = ((!waypointSprite) ? defaultSprite : waypointSprite);
		Text componentInChildren = component.GetComponentInChildren<Text>();
		waypoints.Add(new WaypointData(waypointTarget, waypointName, waypointType, component, componentInChildren));
	}

	public void RequestWaypoint(Transform waypointTarget, string waypointName, Color waypointColor, WaypointType waypointType = WaypointType.None)
	{
		Image component = Object.Instantiate(waypointPrefab, waypointContainer).GetComponent<Image>();
		component.color = waypointColor;
		component.sprite = defaultSprite;
		Text componentInChildren = component.GetComponentInChildren<Text>();
		waypoints.Add(new WaypointData(waypointTarget, waypointName, waypointType, component, componentInChildren));
	}

	public void ResolveWaypoint(Transform waypointTarget)
	{
		foreach (WaypointData waypoint in waypoints)
		{
			if (waypoint.waypointTarget == waypointTarget)
			{
				ResolveWaypoint(waypoint);
				break;
			}
		}
	}

	public void ResolveWaypoint(WaypointData wd)
	{
		waypoints.Remove(wd);
		Object.Destroy(wd.waypointImage.gameObject);
	}
}
