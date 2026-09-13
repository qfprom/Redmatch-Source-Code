using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerMap : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
	[SerializeField]
	private Bounds mapBounds;

	[SerializeField]
	private Bounds worldBounds;

	[SerializeField]
	private GameObject pinpointPrefab;

	private GameObject[] players;

	private GameObject[] playerPinpoints;

	private float nextTimeToPing;

	private float pingDelay = 2f;

	private void Start()
	{
	}

	private Vector3 WorldToMapPosition(Vector3 worldPos)
	{
		Vector3 result = default(Vector3);
		result.x = worldPos.x / worldBounds.size.x * mapBounds.size.x;
		result.y = worldPos.y / worldBounds.size.y * mapBounds.size.y;
		result.z = worldPos.z / worldBounds.size.z * mapBounds.size.z;
		return result;
	}
}
