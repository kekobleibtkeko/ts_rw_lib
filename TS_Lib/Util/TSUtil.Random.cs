using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Verse;
using Weighted_Randomizer;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public interface IWeightedRandom<T>
	{
		int Weight { get; }
		T Value { get; }
	}

	public static TVal? GetRandom<T, TVal>(this IEnumerable<T> items)
		where
			T : IWeightedRandom<TVal>
		where
			TVal: IComparable<TVal>
	{
		if (!items.Any())
			return default;
		var rand = new DynamicWeightedRandomizer<TVal>(Verse.Rand.Int);
		foreach (var item in items)
			if (item.Weight > 0)
				rand.Add(item.Value, item.Weight);
		return rand.NextWithReplacement();
	}
	public static bool TryGetRandom<T, TVal>(this IEnumerable<T> items, [NotNullWhen(true)] out TVal? item)
		where
			T : IWeightedRandom<TVal>
		where
			TVal: IComparable<TVal>
	{
		item = items.GetRandom<T, TVal>();
		return item is not null;
	}
}