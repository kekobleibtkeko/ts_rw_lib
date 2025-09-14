using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public static bool ContainsLowerInvariant(this string str, object value)
	{
		return str.ToLowerInvariant().Contains(value.ToString().ToLowerInvariant());
	}

	public static int FuzzyRatio(string a, string b, bool lower = true)
	{
		if (lower)
		{
			a = a.ToLowerInvariant();
			b = b.ToLowerInvariant();
		}
		return FuzzySharp.Fuzz.WeightedRatio(a, b);
	}
}
