using RimWorld;
using UnityEngine;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public static bool Ctrl => KeyBindingDefOf.ModifierIncrement_10x.IsDownEvent;
	public static bool Shift => KeyBindingDefOf.ModifierIncrement_100x.IsDownEvent;
	public static bool IsRightClick => Event.current.button == 1;
}