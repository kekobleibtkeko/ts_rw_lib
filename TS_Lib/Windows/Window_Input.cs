using System;
using System.Runtime.CompilerServices;
using TS_Lib.Util;
using UnityEngine;
using Verse;

namespace TS_Lib.Windows;

public class Window_Input<T> : Window
{
	public const float WIDTH = 400;
	public const float HEIGHT = 200;

	public T Value;

	public override Vector2 InitialSize => new(WIDTH, HEIGHT);

	public Action<T> AcceptFunc { get; }

	public Window_Input(T initial_val, Action<T> accept_func)
	{
		Value = initial_val;
		AcceptFunc = accept_func;

		// Window-internal stuff
		preventCameraMotion = false;
		draggable = true;
		doCloseX = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		using var list = new TSUtil.Listing_D(inRect);
		using (new TSUtil.TextSize_D(GameFont.Medium))
			list.Listing.Label("TS.value_select".Translate());

		switch (Value)
		{
			case string:
				ref string str = ref Unsafe.As<T, string>(ref Value);
				str = list.Listing.TextEntry(str);
				break;
			default:
				list.Listing.Label($"Error: invalid input type '{typeof(T)}'");
				break;
		}

		if (list.Listing.ButtonText("TS.accept".Translate()))
		{
			AcceptFunc(Value);
			Close();
		}
	}
}