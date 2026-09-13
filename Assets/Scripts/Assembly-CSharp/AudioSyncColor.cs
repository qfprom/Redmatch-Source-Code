using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AudioSyncColor : AudioSyncer
{
	[SerializeField]
	private Color[] beatColors;

	[SerializeField]
	private Color restColor;

	private int randomIndx;

	private Image img;

	private IEnumerator MoveToColor(Color _target)
	{
		Color _curr = img.color;
		Color _initial = _curr;
		float _timer = 0f;
		while (_curr != _target)
		{
			_curr = Color.Lerp(_initial, _target, _timer / timeToBeat);
			_timer += Time.deltaTime;
			img.color = _curr;
			yield return null;
		}
		isBeat = false;
	}

	private Color RandomColor()
	{
		if (beatColors == null || beatColors.Length == 0)
		{
			return Color.white;
		}
		randomIndx = Random.Range(0, beatColors.Length);
		return beatColors[randomIndx];
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!isBeat)
		{
			img.color = Color.Lerp(img.color, restColor, restSmoothTime * Time.deltaTime);
		}
	}

	public override void OnBeat()
	{
		base.OnBeat();
		Color color = RandomColor();
		StopCoroutine("MoveToColor");
		StartCoroutine("MoveToColor", color);
	}

	private void Awake()
	{
		img = GetComponent<Image>();
	}
}
