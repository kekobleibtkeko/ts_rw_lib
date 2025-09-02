using System;
using System.Collections.Generic;
using System.Linq;
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
    ) {
        float margin = 5;
        var prevfont = Text.Font;
        // Text.Font = GameFont.Small;

        float orig = value;
        var rect = list.GetRect(50);
        Widgets.DrawWindowBackground(rect);
        Widgets.Label(rect.Move(margin, margin), name);

        var valrect = rect.LeftPart(.9f).RightHalf();
        string? prevstr = editbuffer;

        editbuffer ??= value.ToString();
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

    public static void DrawDraggableList<T>(
        this Rect draw_rect,
        List<T> values,
        Action<T, Rect> draw_fun,
        Action<T, T?, int?>? drop_fun = null,
        Action<T>? click_fun = null,
        float height = 30,
        float gap = 2,
        float margin = 2,
        Func<T, Color>? color_fun = null,
        Predicate<T>? is_active = null,
        ScrollPosition? scroll_pos = null
    ) {
        //using (new GUIColor_D(Color.red))
        //    Widgets.DrawBox(draw_rect);
        const float SPLITTER_MARGIN = 8;

        drop_fun ??= (val, dropped, index) =>
        {
            if (dropped is not null)
            {
                values.Swap(values.IndexOf(dropped), values.IndexOf(val));
            }
            else if (index.HasValue)
            {
                var insert_index = index.Value;
                var prev_index = values.IndexOf(val);
                if (prev_index > insert_index)
                    insert_index++;
                values.Remove(val);
                Log.Message($"{index.Value} -> {insert_index}");
                if (insert_index < values.Count)
                    values.Insert(insert_index, val);
                else
                    values.Add(val);
            }
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
            .ShrinkRight(GUI.skin.verticalScrollbar.fixedWidth)
        ;

        //using (new GUIColor_D(Color.red))
        //    Widgets.DrawBox(content_rect);

        using var _ = new Scroll_D(draw_rect, content_rect, scroll_pos);
        using var list  = new Listing_D(scroll_pos is null ? draw_rect : content_rect);

        var listing = list.Listing;

        var drnd_group = DragAndDropWidget.NewGroup();
        var m_pos = Event.current.mousePosition;
        int i = 0;
        int drop_index = 0;
        foreach (var val in values.ToList())
        {
            var var_rect = listing.GetRect(height);
            var top = var_rect.TopPartPixels(0);
            var bottom = var_rect.BottomPartPixels(0);

            var_rect = var_rect.ContractedBy(margin);

            // drop area for previous
            var prev_area = top.ExpandedBy(0, SPLITTER_MARGIN);
            if (DragAndDropWidget.Dragging && Mouse.IsOver(prev_area))
            {
                drop_index = i;
                Widgets.DrawBox(top.Move(0, -(gap * 0.5f)));
                DragAndDropWidget.DropArea(drnd_group, prev_area, drop =>
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Log.Message($"dropping before {drop_index}");
                    drop_fun((T)drop, default, drop_index - 1);
                }, default);
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

            DragAndDropWidget.Draggable(drnd_group, var_rect, val, () => {
                if (click_fun is null)
                    return;
                SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
                click_fun(val);
            });

            DragAndDropWidget.DropArea(drnd_group, var_rect.ContractedBy(0, SPLITTER_MARGIN), drop =>
            {
                drop_index = i;
                SoundDefOf.Click.PlayOneShotOnCamera();
                Log.Message($"dropping onto {drop_index}");
                drop_fun((T)drop, val, drop_index);
            }, val);

            // drop area for last
            if (i == item_count - 1)
            {
                var next_area = bottom.ExpandedBy(0, SPLITTER_MARGIN);
                if (DragAndDropWidget.Dragging && Mouse.IsOver(next_area))
                {
                    drop_index = i;
                    Widgets.DrawBox(bottom.Move(0, -(gap * 0.5f)));
                    DragAndDropWidget.DropArea(drnd_group, next_area, drop =>
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        Log.Message($"dropping after {drop_index}");
                        drop_fun((T)drop, default, drop_index + 1);
                    }, default);
                }
            }

            draw_fun(val, var_rect.ContractedBy(margin, 0));

            using (new TextAnchor_D(TextAnchor.MiddleRight))
                Widgets.Label(var_rect, i.ToString());

            listing.Gap(gap);
            
            i++;
        }
    }

    public static void DrawEnumAsButtons<T>(
        this Rect draw_rect,
        Predicate<T> is_active,
        Action<T> pressed,
        Func<T, string>? get_label = null,
        Func<T, string>? get_tooltip = null,
        float? gap = null,
        bool reverse = false
    )
        where
            T : struct, Enum
    {
        var size = draw_rect.height;
        gap ??= size * 0.1f;
        Rect cur_rect;

        int i = 0;
        List<T> list = [.. Enum.GetValues(typeof(T)).Cast<T>()];
        if (reverse)
            list.Reverse();
        foreach (T en in list)
        {
            cur_rect = new(
                reverse
                    ? draw_rect.width - size - (i * size + gap.Value)
                    : i * (size + gap.Value),
                draw_rect.y,
                size,
                size
            );
            var vis_rect = cur_rect.ExpandedBy(-2);
            var active = is_active(en);

            float darken = active ? 0 : 0.6f;
            float saturation = active ? 1 : 0.5f;

            var color = en.GetColorFrom();
            Widgets.DrawBoxSolidWithOutline(
                vis_rect,
                color.Darken(darken + .1f).Saturate(saturation),
                color.Darken(darken).Saturate(saturation)
            );

            if (Mouse.IsOver(cur_rect))
                using (new GUIColor_D(color))
                    Widgets.DrawBox(cur_rect, 2);

            if (Widgets.ClickedInsideRect(cur_rect))
                pressed(en);

            
            using (new TextAnchor_D(TextAnchor.MiddleCenter))
                Widgets.Label(vis_rect, get_label?.Invoke(en) ?? en.ToString());
            if (get_tooltip is not null)
                TooltipHandler.TipRegion(cur_rect, get_tooltip(en));
            i++;
        }
    }
}
