using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TS_Lib.Util;

public static class ChangeNotify<T>
{
	private static readonly HashSet<T> ChangedSinceCheck = [];
	public static void Notify(T changed) => ChangedSinceCheck.Add(changed);
	public static bool TryConsume(T check) => ChangedSinceCheck.Remove(check);
}

public static class ChangeNotifyVal<T, V>
{
	private static readonly Dictionary<T, V> ChangedSinceCheck = [];
	public static void Notify(T changed, V val) => ChangedSinceCheck[changed] = val;
	public static bool TryConsume(T check, [NotNullWhen(true)] out V? val)
	{
		if (ChangedSinceCheck.TryGetValue(check, out val))
		{
			ChangedSinceCheck.Remove(check);
		}
		return val?.Equals(default) == false;
	}
}