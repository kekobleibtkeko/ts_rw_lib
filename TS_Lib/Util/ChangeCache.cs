using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TS_Lib.Util;

public static class ChangeCache<K, V>
{
	private static readonly Dictionary<K, V> CacheVals = [];

	public static void NotifyChange(K key)
	{
		if (CacheVals.ContainsKey(key))
			CacheVals.Remove(key);
	}

	public static void Cache(K key, V value)
	{
		CacheVals[key] = value;
	}

	public static bool TryGet(K key, [NotNullWhen(true)] out V? cached)
	{
		if (CacheVals.TryGetValue(key, out cached!))
			return true;

		cached = default;
		return false;
	}
}