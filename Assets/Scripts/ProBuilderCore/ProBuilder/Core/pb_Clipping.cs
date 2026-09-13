using System;
using UnityEngine;

namespace ProBuilder.Core
{
	internal static class pb_Clipping
	{
		[Flags]
		private enum OutCode
		{
			INSIDE = 0,
			LEFT = 1,
			RIGHT = 2,
			BOTTOM = 4,
			TOP = 8
		}

		private static OutCode ComputeOutCode(Rect rect, float x, float y)
		{
			OutCode outCode = OutCode.INSIDE;
			if (x < rect.xMin)
			{
				outCode |= OutCode.LEFT;
			}
			else if (x > rect.xMax)
			{
				outCode |= OutCode.RIGHT;
			}
			if (y < rect.yMin)
			{
				outCode |= OutCode.BOTTOM;
			}
			else if (y > rect.yMax)
			{
				outCode |= OutCode.TOP;
			}
			return outCode;
		}

		internal static bool RectContainsLineSegment(Rect rect, float x0, float y0, float x1, float y1)
		{
			OutCode outCode = ComputeOutCode(rect, x0, y0);
			OutCode outCode2 = ComputeOutCode(rect, x1, y1);
			bool result = false;
			while (true)
			{
				if ((outCode | outCode2) == OutCode.INSIDE)
				{
					result = true;
					break;
				}
				if ((outCode & outCode2) != OutCode.INSIDE)
				{
					break;
				}
				float num = 0f;
				float num2 = 0f;
				OutCode outCode3 = ((outCode == OutCode.INSIDE) ? outCode2 : outCode);
				if ((outCode3 & OutCode.TOP) == OutCode.TOP)
				{
					num = x0 + (x1 - x0) * (rect.yMax - y0) / (y1 - y0);
					num2 = rect.yMax;
				}
				else if ((outCode3 & OutCode.BOTTOM) == OutCode.BOTTOM)
				{
					num = x0 + (x1 - x0) * (rect.yMin - y0) / (y1 - y0);
					num2 = rect.yMin;
				}
				else if ((outCode3 & OutCode.RIGHT) == OutCode.RIGHT)
				{
					num2 = y0 + (y1 - y0) * (rect.xMax - x0) / (x1 - x0);
					num = rect.xMax;
				}
				else if ((outCode3 & OutCode.LEFT) == OutCode.LEFT)
				{
					num2 = y0 + (y1 - y0) * (rect.xMin - x0) / (x1 - x0);
					num = rect.xMin;
				}
				if (outCode3 == outCode)
				{
					x0 = num;
					y0 = num2;
					outCode = ComputeOutCode(rect, x0, y0);
				}
				else
				{
					x1 = num;
					y1 = num2;
					outCode2 = ComputeOutCode(rect, x1, y1);
				}
			}
			return result;
		}
	}
}
