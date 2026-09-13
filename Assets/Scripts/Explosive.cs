using Photon.Pun;
using UnityEngine;

public class Explosive : MonoBehaviour
{
	private PhotonView PV;

	[SerializeField]
	private float explosionDistance;

	[SerializeField]
	private float explosionDamage;

	[SerializeField]
	private GameObject explosionEffectPrefab;

	[HideInInspector]
	public float timer;

	private int serverTimerStartTime;

	private int timerDuration;

	private bool timing;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	public string GetFormattedTime()
	{
		int num = Mathf.FloorToInt(((float)timerDuration - timer) / 1000f / 60f);
		int num2 = Mathf.FloorToInt(((float)timerDuration - timer) / 1000f - (float)(num * 60));
		return string.Format("{0:0}:{1:00}", num, num2);
	}

	private void Update()
	{
		if (timing)
		{
			timer = PhotonNetwork.ServerTimestamp - serverTimerStartTime;
			if (timer >= (float)timerDuration)
			{
				Detonate();
			}
		}
	}

	[PunRPC]
	private void RPC_StartTimer(int serverTime, int duration)
	{
		timing = true;
		serverTimerStartTime = serverTime;
		timerDuration = duration;
	}

	private void Detonate()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, explosionDistance);
		foreach (Collider collider in array)
		{
			PlayerController component = collider.gameObject.GetComponent<PlayerController>();
			if ((bool)component)
			{
				component.PV.RPC("RPC_TakeDamage", RpcTarget.All, explosionDamage * Mathf.Max(0f, Vector3.Distance(base.transform.position, component.transform.position) / explosionDistance), PV.ViewID, base.transform.position, "Explosive");
			}
		}
		Object.Destroy(Object.Instantiate(explosionEffectPrefab, base.transform.position, explosionEffectPrefab.transform.rotation), 5f);
		PhotonNetwork.Destroy(base.gameObject);
	}
}
