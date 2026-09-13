using UnityEngine;

public struct SafeFloat
{
	private float offset;

	private float value;

	public SafeFloat(float value = 0f)
	{
		offset = Random.Range(-1000, 1000);
		this.value = value + offset;
	}

	public float GetValue()
	{
		return value - offset;
	}

	public void Dispose()
	{
		offset = 0f;
		value = 0f;
	}

	public override string ToString()
	{
		return GetValue().ToString();
	}

	public static SafeFloat operator +(SafeFloat f1, SafeFloat f2)
	{
		return new SafeFloat(f1.GetValue() + f2.GetValue());
	}
}
