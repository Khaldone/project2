using System;
using UnityEngine;

/// <summary>
/// Provides easing functions for smooth animation transitions including back, bounce, circular, cubic, elastic, exponential, linear, quadratic, quartic, quintic, and sine easing.
/// </summary>
public class Tweener
{
	public static float BackEaseIn(float t, float b, float c, float d)
	{
		return BackEaseIn(t, b, c, d, 1.70158f);
	}

	public static float BackEaseIn(float t, float b, float c, float d, float s)
	{
		return c * (t /= d) * t * ((s + 1f) * t - s) + b;
	}

	public static float BackEaseOut(float t, float b, float c, float d)
	{
		return BackEaseOut(t, b, c, d, 1.70158f);
	}

	public static float BackEaseOut(float t, float b, float c, float d, float s)
	{
		return c * ((t = t / d - 1f) * t * ((s + 1f) * t + s) + 1f) + b;
	}

	public static float BackEaseInOut(float t, float b, float c, float d)
	{
		return BackEaseInOut(t, b, c, d, 1.70158f);
	}

	public static float BackEaseInOut(float t, float b, float c, float d, float s)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * (t * t * (((s *= 1.525f) + 1f) * t - s)) + b;
		}
		return c / 2f * ((t -= 2f) * t * (((s *= 1.525f) + 1f) * t + s) + 2f) + b;
	}

	public static float BounceEaseOut(float t, float b, float c, float d)
	{
		if ((t /= d) < 0.363636374f)
		{
			return c * (7.5625f * t * t) + b;
		}
		if (t < 0.727272749f)
		{
			return c * (7.5625f * (t -= 0.545454562f) * t + 0.75f) + b;
		}
		if (t < 0.909090936f)
		{
			return c * (7.5625f * (t -= 0.8181818f) * t + 0.9375f) + b;
		}
		return c * (7.5625f * (t -= 21f / 22f) * t + 63f / 64f) + b;
	}

	public static float BounceEaseIn(float t, float b, float c, float d)
	{
		return c - BounceEaseOut(d - t, 0f, c, d) + b;
	}

	public static float BounceEaseInOut(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return BounceEaseIn(t * 2f, 0f, c, d) * 0.5f + b;
		}
		return BounceEaseOut(t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
	}

	public static float CircEaseIn(float t, float b, float c, float d)
	{
		return (0f - c) * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
	}

	public static float CircEaseOut(float t, float b, float c, float d)
	{
		return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
	}

	public static float CircEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
		}
		return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
	}

	public static float CubicEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * t + b;
	}

	public static float CubicEaseOut(float t, float b, float c, float d)
	{
		return c * ((t = t / d - 1f) * t * t + 1f) + b;
	}

	public static float CubicEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t * t + b;
		}
		return c / 2f * ((t -= 2f) * t * t + 2f) + b;
	}

	public static float ElasticEaseIn(float t, float b, float c, float d)
	{
		return ElasticEaseIn(t, b, c, d, Mathf.Abs(c) - 1f, d * 0.3f);
	}

	public static float ElasticEaseIn(float t, float b, float c, float d, float a)
	{
		return ElasticEaseIn(t, b, c, d, a, d * 0.3f);
	}

	public static float ElasticEaseIn(float t, float b, float c, float d, float a, float p)
	{
		if (t == 0f)
		{
			return b;
		}
		if ((t /= d) == 1f)
		{
			return b + c;
		}
		float num;
		if (a < Mathf.Abs(c))
		{
			a = c;
			num = p / 4f;
		}
		else
		{
			num = p / ((float)Math.PI * 2f) * Mathf.Asin(c / a);
		}
		return 0f - a * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - num) * ((float)Math.PI * 2f) / p) + b;
	}

	public static float ElasticEaseOut(float t, float b, float c, float d)
	{
		return ElasticEaseOut(t, b, c, d, Mathf.Abs(c) - 1f, d * 0.3f);
	}

	public static float ElasticEaseOut(float t, float b, float c, float d, float a)
	{
		return ElasticEaseOut(t, b, c, d, a, d * 0.3f);
	}

	public static float ElasticEaseOut(float t, float b, float c, float d, float a, float p)
	{
		if (t == 0f)
		{
			return b;
		}
		if ((t /= d) == 1f)
		{
			return b + c;
		}
		float num;
		if (a < Mathf.Abs(c))
		{
			a = c;
			num = p / 4f;
		}
		else
		{
			num = p / ((float)Math.PI * 2f) * Mathf.Asin(c / a);
		}
		return a * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * d - num) * ((float)Math.PI * 2f) / p) + c + b;
	}

	public static float ElasticEaseInOut(float t, float b, float c, float d)
	{
		return ElasticEaseInOut(t, b, c, d, Mathf.Abs(c) - 1f, d * 0.450000018f);
	}

	public static float ElasticEaseInOut(float t, float b, float c, float d, float a)
	{
		return ElasticEaseInOut(t, b, c, d, a, d * 0.450000018f);
	}

	public static float ElasticEaseInOut(float t, float b, float c, float d, float a, float p)
	{
		if (t == 0f)
		{
			return b;
		}
		if ((t /= d / 2f) == 2f)
		{
			return b + c;
		}
		float num;
		if (a < Mathf.Abs(c))
		{
			a = c;
			num = p / 4f;
		}
		else
		{
			num = p / ((float)Math.PI * 2f) * Mathf.Asin(c / a);
		}
		if (t < 1f)
		{
			return -0.5f * (a * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - num) * ((float)Math.PI * 2f) / p)) + b;
		}
		return a * Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t * d - num) * ((float)Math.PI * 2f) / p) * 0.5f + c + b;
	}

	public static float ExpoEaseIn(float t, float b, float c, float d)
	{
		return (t != 0f) ? (c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b) : b;
	}

	public static float ExpoEaseOut(float t, float b, float c, float d)
	{
		return (t != d) ? (c * (0f - Mathf.Pow(2f, -10f * t / d) + 1f) + b) : (b + c);
	}

	public static float ExpoEaseInOut(float t, float b, float c, float d)
	{
		if (t == 0f)
		{
			return b;
		}
		if (t == d)
		{
			return b + c;
		}
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
		}
		return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
	}

	public static float LinearEaseNone(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static float LinearEaseIn(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static float LinearEaseOut(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static float LinearEaseInOut(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static float QuadEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t + b;
	}

	public static float QuadEaseOut(float t, float b, float c, float d)
	{
		return (0f - c) * (t /= d) * (t - 2f) + b;
	}

	public static float QuadEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t + b;
		}
		return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
	}

	public static float QuartEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * t * t + b;
	}

	public static float QuartEaseOut(float t, float b, float c, float d)
	{
		return (0f - c) * ((t = t / d - 1f) * t * t * t - 1f) + b;
	}

	public static float QuartEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t * t * t + b;
		}
		return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
	}

	public static float QuintEaseIn(float t, float b, float c, float d)
	{
		return c * (t /= d) * t * t * t * t + b;
	}

	public static float QuintEaseOut(float t, float b, float c, float d)
	{
		return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
	}

	public static float QuintEaseInOut(float t, float b, float c, float d)
	{
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * t * t * t * t * t + b;
		}
		return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
	}

	public static float SineEaseIn(float t, float b, float c, float d)
	{
		return (0f - c) * Mathf.Cos(t / d * ((float)Math.PI / 2f)) + c + b;
	}

	public static float SineEaseOut(float t, float b, float c, float d)
	{
		return c * Mathf.Sin(t / d * ((float)Math.PI / 2f)) + b;
	}

	public static float SineEaseInOut(float t, float b, float c, float d)
	{
		return (0f - c) / 2f * (Mathf.Cos((float)Math.PI * t / d) - 1f) + b;
	}
}
