using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TS_Lib.Util;

public static partial class TSUtil
{
    public static Rect ShrinkRight(this Rect rect, float p) => new(rect.x, rect.y, rect.width - p, rect.height);
    public static Rect ShrinkLeft(this Rect rect, float p) => new(rect.x + p, rect.y, rect.width - p, rect.height);
    public static Rect ShrinkTop(this Rect rect, float p) => new(rect.x, rect.y + p, rect.width, rect.height - p);
    public static Rect ShrinkBottom(this Rect rect, float p) => new(rect.x, rect.y, rect.width, rect.height - p);

    public static Rect GrowRight(this Rect rect, float p) => ShrinkRight(rect, -p);
    public static Rect GrowLeft(this Rect rect, float p) => ShrinkLeft(rect, -p);
    public static Rect GrowTop(this Rect rect, float p) => ShrinkTop(rect, -p);
    public static Rect GrowBottom(this Rect rect, float p) => ShrinkBottom(rect, -p);

    public static Rect Move(this Rect rect, float x = 0, float y = 0) => new(rect.x + x, rect.y + y, rect.width, rect.height);

    public static Rect Square(this Rect rect) => new(rect.x, rect.y, Mathf.Min(rect.width, rect.height), Mathf.Min(rect.width, rect.height));

}
