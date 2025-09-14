using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using HarmonyLib;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public enum ListInclusionType
	{
		Any,
		All
	}

	public static Func<Func<T, bool>, bool> GetFuncFor<T>(this ListInclusionType inclusion, IEnumerable<T> col) => inclusion switch
	{
		ListInclusionType.Any => col.Any()
			? col.Any
			: f => true,
		ListInclusionType.All or _ => col.Any()
			? col.All
			: f => true,
	};

	public static void Do<T1, T2>(this IEnumerable<(T1, T2)> collection, Action<T1, T2> action)
		=> collection.Do(((T1 a, T2 b) val) => action(val.a, val.b));

	public static V Ensure<K, V>(this IDictionary<K, V> dict, K key)
		where
			V : new()
	{
		if (!dict.TryGetValue(key, out V val))
		{
			val = new();
			dict[key] = val;
		}
		return val;
	}

	public static V Ensure<K, V>(this IDictionary<K, V> dict, K key, Func<V> def_func)
	{
		if (!dict.TryGetValue(key, out V val))
		{
			val = def_func();
			dict[key] = val;
		}
		return val;
	}

	public static List<T> GetEnumValues<T>()
		where
			T : struct, Enum
		=> [.. Enum.GetValues(typeof(T)).Cast<T>()];

	public static T? Next<T>(this IEnumerator<T> en)
	{
		if (en.MoveNext())
			return en.Current;
		return default;
	}
}