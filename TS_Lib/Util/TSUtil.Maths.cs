using UnityEngine;
using Verse;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public static float DistanceFrom(this FloatRange range, float f)
	{
		if (range.Includes(f))
			return 0;
		return Mathf.Min(
			Mathf.Abs(range.TrueMin - f),
			Mathf.Abs(range.TrueMax - f)
		);
	}
}