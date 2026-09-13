using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimedHover : MonoBehaviour
{
	[SerializeField]
	private float timerDuration;

	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private UnityEvent onFinish;

	private bool timing;

	private float timer;

	public void StartTimer()
	{
		timing = true;
		timer = 0f;
		progressImage.gameObject.SetActive(true);
	}

	public void StopTimer()
	{
		timing = false;
		progressImage.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (timing)
		{
			timer += Time.deltaTime;
			progressImage.fillAmount = timer / timerDuration;
			if (timer >= timerDuration)
			{
				FinishTimer();
			}
		}
	}

	private void FinishTimer()
	{
		onFinish.Invoke();
		StopTimer();
	}
}
