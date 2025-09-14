using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace TS_Lib.Util;

public static partial class TSUtil
{
	public delegate TaggedString TranslatorDelegate(string key);

	public static float ScrollbarSize => GUI.skin.verticalScrollbar.fixedWidth + 2;

	public static bool SliderLabeledWithValue(
		this Listing_Standard list,
		ref float value,
		string name,
		float min,
		float max,
		Dictionary<string, string> edit_buffers,
		string? tt = null,
		float? resetval = null,
		float? accuracy = null
	)
	{
		var buffer = edit_buffers.Ensure(name, () => string.Empty);
		var ret = list.SliderLabeledWithValue(ref value, name, min, max, ref buffer, tt, resetval, accuracy);
		edit_buffers[name] = buffer ?? string.Empty;
		return ret;
	}
	public static bool SliderLabeledWithValue(
		this Listing_Standard list,
		ref float value,
		string name,
		float min,
		float max,
		ref string? editbuffer,
		string? tt = null,
		float? resetval = null,
		float? accuracy = null
	)
	{
		float margin = 5;
		var prevfont = Text.Font;
		// Text.Font = GameFont.Small;

		float orig = value;
		var rect = list.GetRect(50);
		Widgets.DrawWindowBackground(rect);
		Widgets.Label(rect.Move(margin, margin), name);

		var valrect = rect.LeftPart(.9f).RightHalf();
		string? prevstr = editbuffer;

		if (editbuffer.NullOrEmpty())
			editbuffer = value.ToString();
		editbuffer = Widgets.TextField(valrect.TopHalf().ShrinkTop(margin), editbuffer);

		if (!string.Equals(editbuffer, prevstr)
			&& !string.IsNullOrEmpty(editbuffer)
			&& editbuffer.IsFullyTypedNumber<float>()
			&& editbuffer != "-")
		{
			value = float.Parse(editbuffer);
		}
		else if (!string.IsNullOrEmpty(editbuffer)
			&& !editbuffer.EndsWith(".")
			&& editbuffer != "-")
		{
			editbuffer = value.ToString();
		}

		value = Widgets.HorizontalSlider(
			valrect.BottomHalf().ShrinkTop(margin * 1.5f),
			value,
			min, max,
			roundTo: accuracy ?? .01f
		);

		var resetrect = rect
			.RightPart(.1f)
			.Square()
			.ExpandedBy(-3);

		if (resetval.HasValue && Widgets.ButtonImage(resetrect, TexButton.Delete))
		{
			value = resetval.Value;
		}

		if (tt is not null)
			TooltipHandler.TipRegion(rect, tt);

		Text.Font = prevfont;
		return orig != value;
	}

	public static void BeginScrollView(Rect outRect, ref Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
	{
		if (Widgets.mouseOverScrollViewStack.Count > 0)
		{
			Widgets.mouseOverScrollViewStack.Push(Widgets.mouseOverScrollViewStack.Peek() && outRect.Contains(Event.current.mousePosition));
		}
		else
		{
			Widgets.mouseOverScrollViewStack.Push(outRect.Contains(Event.current.mousePosition));
		}

		SteamDeck.HandleTouchScreenScrollViewScroll(outRect, ref scrollPosition);
		scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical);

		UnityGUIBugsFixer.Notify_BeginScrollView();
	}

	public static bool DrawColoredBox(this Rect rect, Color color, bool active = false, float darken = 0.6f, float saturation = 0.5f, bool is_button = false)
	{
		if (color == Color.clear)
			return false;

		darken = active ? 0 : darken;
		saturation = active ? 1 : saturation;

		Widgets.DrawBoxSolidWithOutline(
			rect,
			color.Darken(darken + .1f).Saturate(saturation),
			color.Darken(darken).Saturate(saturation)
		);

		if (is_button)
		{
			if (Mouse.IsOver(rect))
				using (new GUIColor_D(color))
					Widgets.DrawBox(rect, 2);

			return Widgets.ButtonInvisible(rect);
		}

		return false;
	}

	public static void SplitVerticallyPct(this Rect rect, float left_pct, out Rect left, out Rect right, float margin = 0)
	{
		var left_size = rect.width * left_pct;
		rect.SplitVerticallyWithMargin(out left, out right, out _, margin, left_size);
	}

	public static Rect GetRemaining(this Listing listing)
	{
		return new(listing.curX, listing.curY, listing.listingRect.width, listing.listingRect.height - listing.CurHeight);
	}

	public static bool DrawDraggableList<T>(
		this Listing_Standard listing,
		List<T> values,
		Action<T, Rect> draw_fun,
		Action<T, T, int>? drop_fun = null,
		Action<T>? click_fun = null,
		float height = 30,
		float gap = 2,
		float margin = 2,
		Func<T, Color>? color_fun = null,
		Predicate<T>? is_active = null,
		ScrollPosition? scroll_pos = null,
		bool no_drag = false,
		Func<T, float?>? button_size = null
	)
	{
		var total_height = values.Count * (height + gap);
		return listing
			.GetRect(total_height)
			.DrawDraggableList(
				values,
				draw_fun,
				drop_fun,
				click_fun,
				height,
				gap,
				margin,
				color_fun,
				is_active,
				scroll_pos,
				no_drag,
				button_size
			)
		;
	}

	public static bool DrawDraggableList<T>(
		this Rect draw_rect,
		List<T> values,
		Action<T, Rect> draw_fun,
		Action<T, T, int>? drop_fun = null,
		Action<T>? click_fun = null,
		float height = 30,
		float gap = 2,
		float margin = 2,
		Func<T, Color>? color_fun = null,
		Predicate<T>? is_active = null,
		ScrollPosition? scroll_pos = null,
		bool no_drag = false,
		Func<T, float?>? button_size = null
	)
	{
		//using (new GUIColor_D(Color.red))
		//    Widgets.DrawBox(draw_rect);
		const float SPLITTER_MARGIN = 8;

		bool changed = false;

		drop_fun ??= (val, dropped_on, offset) =>
		{
			var val_i = values.IndexOf(val);
			var dropped_on_i = values.IndexOf(dropped_on);
			if (val_i < 0 || dropped_on_i < 0 || val_i == dropped_on_i)
				return;

			Log.Message($"{val_i} -> {dropped_on_i} + {offset}");

			switch (offset)
			{
				case 0:
					// Swap
					(values[val_i], values[dropped_on_i]) = (values[dropped_on_i], values[val_i]);
					break;

				case -1:
					// Move a before b
					{
						var item = values[val_i];
						values.RemoveAt(val_i);
						// If aIndex < bIndex, removing shifts bIndex left by one
						if (val_i < dropped_on_i) dropped_on_i--;
						values.Insert(dropped_on_i, item);
					}
					break;

				case 1:
					// Move a after b
					{
						var item = values[val_i];
						values.RemoveAt(val_i);
						// If aIndex < bIndex, removing shifts bIndex left by one
						if (val_i < dropped_on_i) dropped_on_i--;
						values.Insert(dropped_on_i + 1, item);
					}
					break;
			}
			changed = true;
		};

		var item_count = values.Count;
		var content_rect = new Rect(
			0,
			0,
			draw_rect.width - (margin * 2),
			Mathf.Max(
				item_count * (height + gap),
				draw_rect.height
			)
		)
			.ShrinkRight(ScrollbarSize)
		;

		//using (new GUIColor_D(Color.red))
		//    Widgets.DrawBox(content_rect);

		using var _ = new Scroll_D(draw_rect, content_rect, scroll_pos);
		using var list = new Listing_D(scroll_pos is null ? draw_rect : content_rect);

		var listing = list.Listing;

		var drnd_group = DragAndDropWidget.NewGroup();
		var drag_size = no_drag ? 0 : height;
		var m_pos = Event.current.mousePosition;
		int i = 0;
		foreach (var val in values.ToList())
		{
			Rect prev_area;
			Rect? next_area = null;
			var var_rect = listing.GetRect(height);
			var top = var_rect.TopPartPixels(0);
			var bottom = var_rect.BottomPartPixels(0);

			var drag_rect = var_rect.LeftPartPixels(drag_size);
			var_rect = var_rect.ContractedBy(margin);
			// drop area for previous
			prev_area = top.ExpandedBy(0, SPLITTER_MARGIN);
			if (!no_drag && DragAndDropWidget.Dragging && Mouse.IsOver(prev_area))
			{
				// using (new GUIColor_D(Color.green))
				// 	Widgets.DrawBox(prev_area);
				Widgets.DrawBox(top.Move(0, -(gap * 0.5f)));
				DragAndDropWidget.DropArea(drnd_group, prev_area, drop =>
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					Log.Message($"dropping before {values.IndexOf(val)}");
					drop_fun((T)drop, val, -1);
				}, null);
			}

			var active = is_active?.Invoke(val) ?? false;
			if (color_fun is null)
			{
				Widgets.DrawOptionBackground(var_rect, active);
			}
			else
			{
				var color = color_fun(val);
				DrawColoredBox(var_rect, color, active);
			}

			var but_size = button_size?.Invoke(val);
			if (click_fun is not null && Widgets.ButtonInvisible(var_rect.ShrinkLeft(drag_size).LeftPart(but_size ?? 1)))
			{
				SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
				click_fun(val);
			}

			if (!no_drag)
			{
				Widgets.DrawTextureFitted(drag_rect.ContractedBy(2), TexButton.DragHash, 1);
				DragAndDropWidget.Draggable(drnd_group, drag_rect, val, onStartDragging: () =>
				{
					Log.Message($"started dragging: {val} [{values.IndexOf(val)}]");
				});

				// drop area for last
				if (i == item_count - 1)
				{
					next_area = bottom.ExpandedBy(0, SPLITTER_MARGIN);
					if (DragAndDropWidget.Dragging && Mouse.IsOver(next_area.Value))
					{
						// using (new GUIColor_D(Color.blue))
						// 	Widgets.DrawBox(next_area.Value);
						Widgets.DrawBox(bottom.Move(0, -(gap * 0.5f)));
						DragAndDropWidget.DropArea(drnd_group, next_area.Value, drop =>
						{
							SoundDefOf.Click.PlayOneShotOnCamera();
							Log.Message($"dropping after {values.IndexOf(val)}");
							drop_fun((T)drop, val, 1);
						}, null);
					}
				}

				var cur_area = var_rect;
				if (DragAndDropWidget.Dragging && !Mouse.IsOver(prev_area) && !Mouse.IsOver(prev_area.Move(0, height + gap)))
				{
					// using (new GUIColor_D(Color.red))
					// 	Widgets.DrawBox(cur_area);
					DragAndDropWidget.DropArea(drnd_group, var_rect, drop =>
					{
						SoundDefOf.Click.PlayOneShotOnCamera();
						Log.Message($"dropping onto {values.IndexOf(val)}");
						drop_fun((T)drop, val, 0);
					}, null);
				}
			}

			draw_fun(val, var_rect.ShrinkLeft(drag_size).ContractedBy(margin, 0));

			// using (new TextAnchor_D(TextAnchor.MiddleRight))
			// 	Widgets.Label(var_rect, i.ToString());

			listing.Gap(gap);

			i++;
		}
		return changed;
	}

	public static bool ButtonImage(this Listing listing, Texture2D image, float size = 20, string? tooltip = null, SoundDef? sound = null)
		=> listing
			.GetRect(size)
			.RectsIn()
			.First()
			.ButtonImage(image, tooltip, sound);

	public static bool ButtonImage(this Rect rect, Texture2D image, string? tooltip = null, SoundDef? sound = null)
	{
		if (Widgets.ButtonImage(
				rect,
				image,
				tooltip: tooltip
			))
		{
			(sound ?? SoundDefOf.Click).PlayOneShotOnCamera();
			return true;
		}
		return false;
	}

	public static IEnumerable<Rect> RectsIn(this Rect outer, bool reverse = false, float? gap = null, float size_ratio = 1)
	{
		var horizontal = outer.width > outer.height;
		var size = horizontal
			? outer.height
			: outer.width
		;
		gap ??= size * 0.1f;
		size *= size_ratio;

		for (int i = 0; true; i++)
		{
			yield return horizontal
				? new(
					outer.x + (reverse
						? outer.width - size - (i * size + gap.Value)
						: i * (size + gap.Value)),
					outer.y,
					size,
					outer.height
				)
				: new(
					outer.x,
					outer.y + (reverse
						? outer.height - size - (i * size + gap.Value)
						: i * (size + gap.Value)),
					outer.width,
					size
				)
			;
		}
	}

	public static IEnumerable<(T, Rect)> SplitIntoSquaresGap<T>(this IEnumerable<T> items, Rect rect, float? gap = null, bool reverse = false, bool reverse_order = false, float size_ratio = 1)
	{
		var horizontal = rect.width > rect.height;
		var size = horizontal
			? rect.height
			: rect.width
		;
		gap ??= size * 0.1f;
		List<T> list = [.. items];
		if (reverse_order)
			list.Reverse();

		var rects = rect.RectsIn(reverse, gap, size_ratio).GetEnumerator();
		foreach (var item in items)
		{
			yield return (item, rects.Next());
		}
	}

	public static void DrawAsColoredButtons<T>(
		this IEnumerable<T> values,
		Rect draw_rect,
		Predicate<T> is_active,
		Action<T> pressed,
		Func<T, string>? get_label = null,
		Func<T, string>? get_tooltip = null,
		Action<T, Rect>? draw_fun = null,
		float? gap = null,
		bool reverse = false,
		bool reverse_order = false,
		float size_ratio = 1
	)
	{
		var size = draw_rect.height;
		gap ??= size * 0.1f;

		values.SplitIntoSquaresGap(
			draw_rect,
			gap,
			reverse,
			reverse_order,
			size_ratio
		).Do((val, rect) =>
		{
			var active = is_active(val);
			bool clicked = false;
			if (val!.TryGetColorFrom(out var color))
			{
				var vis_rect = rect.ExpandedBy(-2);

				float darken = active ? 0 : 0.6f;
				float saturation = active ? 1 : 0.5f;
				clicked = DrawColoredBox(vis_rect, color.Value, active, is_button: true);
			}
			else
			{
				Widgets.DrawOptionBackground(rect, active);
				clicked = Widgets.ButtonInvisible(rect);
			}

			if (clicked)
				pressed(val);

			draw_fun?.Invoke(val, rect);

			if (get_label is not null)
				using (new TextAnchor_D(TextAnchor.MiddleCenter))
					Widgets.Label(rect, get_label(val));
			if (get_tooltip is not null)
				TooltipHandler.TipRegion(rect, get_tooltip(val));
		});
	}

	public static void DrawEnumAsButtons<T>(
		this Rect rect,
		ref T val,
		float? gap = null,
		bool reverse = false,
		bool reverse_order = false,
		float size_ratio = 1,
		TranslatorDelegate? translator = null
	)
		where
			T : struct, Enum
	{
		// ChangeNotifyVal<T, T>;
		var non_ref = val;
		DrawEnumAsButtons<T>(
			rect,
			en => Enum.Equals(en, non_ref),
			en => ChangeNotifyVal<Type, T>.Notify(typeof(T), en),
			en => translator is null ? en.ToString() : translator($"{typeof(T)}.{en}"),
			gap: gap,
			reverse: reverse,
			reverse_order: reverse_order,
			size_ratio: size_ratio
		);

		if (ChangeNotifyVal<Type, T>.TryConsume(typeof(T), out var new_val))
		{
			val = new_val;
		}
	}

	public static void DrawEnumAsButtons<T>(
		this Rect draw_rect,
		Predicate<T> is_active,
		Action<T> pressed,
		Func<T, string>? get_label = null,
		Func<T, string>? get_tooltip = null,
		Action<T, Rect>? draw_fun = null,
		float? gap = null,
		bool reverse = false,
		bool reverse_order = false,
		float size_ratio = 1
	)
		where
			T : struct, Enum
	{
		Enum
			.GetValues(typeof(T))
			.Cast<T>()
			.DrawAsColoredButtons(
				draw_rect,
				is_active,
				pressed,
				get_label,
				get_tooltip,
				draw_fun,
				gap,
				reverse,
				reverse_order,
				size_ratio
			)
		;
	}
	public static WidgetRow Row(this Listing list, float height, float row_gap = 4)
		=> list.GetRect(height).Row(row_gap);
	public static Rect Labled(this Listing list, float height, string label, TranslatorDelegate? translator = null, float split = 0.5f)
		=> list.GetRect(height).Labled(label, translator, split);

	public static WidgetRow LabeledRow(this Rect rect, string label, TranslatorDelegate? translator = null, float split = 0.5f, float row_gap = 4)
	{
		var row_rect = rect.Labled(label, translator, split);
		return row_rect.Row(row_gap);
	}
	public static WidgetRow Row(this Rect row_rect, float row_gap = 4)
	{
		return new(row_rect.x, row_rect.y, UIDirection.RightThenDown, gap: row_gap);
	}

	public static Rect Labled(this Rect rect, string label, TranslatorDelegate? translator = null, float split = 0.5f)
	{
		rect.SplitVerticallyPct(split, out var label_rect, out var content_rect);
		if (translator is not null)
			label = translator(label);
		using (new TextAnchor_D(TextAnchor.MiddleLeft))
			Widgets.Label(label_rect, label);
		return content_rect;
	}

	public static class Dropdown<T>
	{
		private static readonly Dictionary<int, T> ValuesToChange = [];
		public static bool Value(
			IEnumerable<T> col,
			Rect rect,
			ref T value,
			int id,
			Func<T, string>? string_func = null
		)
		{
			bool changed = false;
			if (ValuesToChange.TryGetValue(id, out var change))
			{
				ValuesToChange.Remove(id);
				value = change;
				changed = true;
			}

			string _get_opt_name(T val)
				=> string_func?.Invoke(val)
					?? val?.ToString()
					?? "none"
			;
			var val = value;
			Widgets.Dropdown(
				rect,
				value,
				tar => tar,
				tar => col.Select(x =>
				{
					var dropdown_el = new Widgets.DropdownMenuElement<T>()
					{
						payload = x,
						option = new(
							_get_opt_name(x),
							() => ValuesToChange[id] = x
						)
					};
					return dropdown_el;
				}),
				$"► {_get_opt_name(val)}"
			);
			return changed;
		}
	}
	public static bool ValueDropdown<T>(
		this IEnumerable<T> col,
		Rect rect,
		ref T value,
		int id,
		Func<T, string>? string_func = null
	) => Dropdown<T>.Value(col, rect, ref value, id, string_func);

	public static void Menu<T>(
		this IEnumerable<T> values,
		Action<T> press_func,
		Func<T, string> label_func,
		Func<T, ThingDef?>? icon_func = null,
		Func<T, bool>? enabled_func = null,
		string? title = null
	)
	{
		List<FloatMenuOption> opts = [];
		foreach (var val in values)
		{
			var icon = icon_func?.Invoke(val);
			FloatMenuOption opt = new(
				label_func(val),
				() => press_func(val),
				icon
			);
			if (enabled_func?.Invoke(val) == false)
				opt.action = null;
			opts.Add(opt);
		}
		var menu = new FloatMenu(opts);
		Find.WindowStack.Add(menu);
	}
}
