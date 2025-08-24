using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TS_Lib.Util;

public static partial class TSUtil
{
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
}
