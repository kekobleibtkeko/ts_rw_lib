using System.Diagnostics.CodeAnalysis;

namespace TS_Lib.Util;

public static class Clipboard<T>
{
	private static T? Value;
	public static void SetValue(T value) => Value = value;
	public static bool ClearValue()
	{
		bool cleared = Value is not null;
		Value = default;
		return cleared;
	}
	public static T? GetValue() => Value;
	public static bool HasValue => Value is not null;
	public static bool TryGetValue([NotNullWhen(true)] out T? value)
	{
		value = GetValue();
		return value is not null;
	}
}