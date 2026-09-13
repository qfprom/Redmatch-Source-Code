using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[SerializeField]
	private string text;

	private float delay = 0.5f;

	private bool waiting;

	private float time;

	private bool focused;

	public void OnPointerEnter(PointerEventData eventData)
	{
		waiting = true;
		focused = true;
		time = 0f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TooltipManager.Instance.ResolveTooltip();
		waiting = false;
		focused = false;
	}

	private void OnDisable()
	{
		if (focused)
		{
			TooltipManager.Instance.ResolveTooltip();
		}
	}

	private void Update()
	{
		if (waiting)
		{
			time += Time.deltaTime;
			if (time >= delay)
			{
				TooltipManager.Instance.RequestTooltip(text);
				waiting = false;
			}
		}
	}
}
