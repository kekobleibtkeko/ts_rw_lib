using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Verse;

namespace TS_Lib.Util;

public static class TickCache<K, V>
{
	private static readonly Dictionary<K, (int, V)> CachedValues = [];
	public static void ResetCache(K key)
	{
		if (CachedValues.ContainsKey(key))
			CachedValues.Remove(key);
	}
	public static bool TryGetCached(K key, [NotNullWhen(true)] out V? value)
	{
		var current_tick = TSUtil.TicksGame;
		value = default;
		if (CachedValues.TryGetValue(key, out var cache))
		{
			(var cache_tick, value) = cache;
			if (current_tick <= cache_tick)
				return value is not null;
		}
		return false;
	}

	public static V Cache(K key, V value, int timeout = 0)
	{
		CachedValues[key] = (TSUtil.TicksGame + timeout, value);
		return value;
	}
}