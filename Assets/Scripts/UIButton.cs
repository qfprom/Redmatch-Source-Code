using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
	private Vector3 originalScale;

	private Image image;

	private Text text;

	private Color originalImageColor;

	private Color originalTextColor;

	private void Awake()
	{
		originalScale = base.transform.localScale;
		image = GetComponent<Image>();
		text = GetComponentInChildren<Text>();
		if ((bool)image)
		{
			originalImageColor = image.color;
		}
		if ((bool)text)
		{
			originalTextColor = text.color;
		}
	}

	private void OnDisable()
	{
		base.transform.localScale = originalScale;
		if ((bool)image)
		{
			image.color = originalImageColor;
		}
		if ((bool)text)
		{
			text.color = originalTextColor;
		}
	}
}
