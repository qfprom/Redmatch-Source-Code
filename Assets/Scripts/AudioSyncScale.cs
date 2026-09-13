using System.Collections;
using UnityEngine;

public class AudioSyncScale : AudioSyncer
{
	[SerializeField]
	private Vector3 beatScale;

	[SerializeField]
	private Vector3 restScale;

	private IEnumerator MoveToScale(Vector3 _target)
	{
		Vector3 _curr = base.transform.localScale;
		Vector3 _initial = _curr;
		float _timer = 0f;
		while (_curr != _target)
		{
			_curr = Vector3.Lerp(_initial, _target, _timer / timeToBeat);
			_timer += Time.deltaTime;
			base.transform.localScale = _curr;
			yield return null;
		}
		isBeat = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!isBeat)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, restScale, restSmoothTime * Time.deltaTime);
		}
	}

	public override void OnBeat()
	{
		base.OnBeat();
		StopCoroutine("MoveToScale");
		StartCoroutine("MoveToScale", beatScale);
	}
}
