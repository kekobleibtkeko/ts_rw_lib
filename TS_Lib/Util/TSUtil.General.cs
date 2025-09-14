using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public static void SetFlag<T>(this ref T flags, T flag, bool state)
		where T : struct, Enum
	{
		flags = (T)(object)(state
			? Convert.ToInt32(flags) | Convert.ToInt32(flag)    // SetFlag
			: Convert.ToInt32(flags) & ~Convert.ToInt32(flag)   // ClearFlag 
		);
	}

	public static void ToggleFlag<T>(this ref T flags, T flag)
		where T : struct, Enum
	{
		SetFlag(ref flags, flag, !flags.HasFlag(flag));
	}

	public static T? DirtyClone<T>(this T obj)
	{
		if (object.Equals(obj, default(T)))
			return default;
		return (T)AccessTools.Method(typeof(object), "MemberwiseClone").Invoke(obj, null);
	}

	public static IEnumerable<T> IterateFlags<T>()
		where
			T : struct, Enum
	{
		foreach (T en in Enum.GetValues(typeof(T)))
		{
			if ((int)(object)en == 0)
				continue;
			yield return en;
		}
	}
}
