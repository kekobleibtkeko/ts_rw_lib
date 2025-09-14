using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TS_Lib.Util;

public static partial class TSUtil
{
    public interface IToColor
    {
        Color ToColor();
    }

    public static Dictionary<Type, Func<object, Color>> ColorConverters = [];

    public static Color ColorFromHTML(string text)
    {
        if (ColorUtility.TryParseHtmlString(text, out var color))
            return color;
        return Color.red;
    }

    public static Color Darken(this Color clr, float t) => Color.LerpUnclamped(clr, Color.black, t);
    public static Color Saturate(this Color color, float t)
    {
        Color.RGBToHSV(color, out float hue, out float saturation, out float brightness);
        return Color.HSVToRGB(hue, saturation * t, brightness);
    }

    public static void RegisterToColorHandler<T>(Func<T, Color> color_fun)
    {
        ColorConverters[typeof(T)] = (val) =>
        {
            if (val is not T t)
                return Color.red;
            return color_fun(t);
        };
    }

	public static bool TryGetColorFrom(this object val, [NotNullWhen(true)] out Color? color)
	{
		if (val is IToColor to_color)
		{
			color = to_color.ToColor();
			return true;
		}
		if (ColorConverters.TryGetValue(val.GetType(), out var color_fun))
		{
			color = color_fun(val);
			return true;
		}

		color = null;
		return false;
	}

	public static Color GetColorFrom(this object val)
	{
		if (val is IToColor to_clr)
			return to_clr.ToColor();
		if (!ColorConverters.TryGetValue(val.GetType(), out var clr_fun))
			return Color.red;
		return clr_fun(val);
	}
}
