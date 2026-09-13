using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
	public static TooltipManager Instance;

	[SerializeField]
	private RectTransform tooltip;

	[SerializeField]
	private Text tooltipText;

	[SerializeField]
	private CanvasGroup tooltipCanvasGroup;

	[SerializeField]
	private Canvas canvas;

	private int tooltipRequests;

	private void Awake()
	{
		Instance = this;
	}

	public void RequestTooltip(string text)
	{
		tooltipRequests++;
		tooltipText.text = text;
		ShowTooltip();
	}

	public void ResolveTooltip()
	{
		tooltipRequests--;
		if (tooltipRequests <= 0)
		{
			HideTooltip();
		}
	}

	private void ShowTooltip()
	{
		StartCoroutine(TooltipFadeIn(0.1f));
	}

	private void HideTooltip()
	{
		StartCoroutine(TooltipFadeOut(0.1f));
	}

	private IEnumerator TooltipFadeIn(float time)
	{
		float alpha = tooltipCanvasGroup.alpha;
		for (float t = 0f; t < 1f; t += Time.deltaTime / time)
		{
			tooltipCanvasGroup.alpha = Mathf.Lerp(alpha, 1f, t);
			yield return null;
		}
		tooltipCanvasGroup.alpha = 1f;
	}

	private IEnumerator TooltipFadeOut(float time)
	{
		float alpha = tooltipCanvasGroup.alpha;
		for (float t = 0f; t < 1f; t += Time.deltaTime / time)
		{
			tooltipCanvasGroup.alpha = Mathf.Lerp(alpha, 0f, t);
			yield return null;
		}
		tooltipCanvasGroup.alpha = 0f;
	}

	private void Update()
	{
		float min = 0f;
		float max = (float)Screen.width - tooltip.rect.width * canvas.scaleFactor;
		float min2 = tooltip.rect.height * canvas.scaleFactor;
		float max2 = Screen.height;
		tooltip.transform.position = new Vector3(Mathf.Clamp(Input.mousePosition.x, min, max), Mathf.Clamp(Input.mousePosition.y, min2, max2));
	}
}
