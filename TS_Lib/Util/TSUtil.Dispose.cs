using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TS_Lib.Util;

public static partial class TSUtil
{

    public abstract class TSDisposableHelper<T> : IDisposable
    {
        public T OldValue { get; }
        public abstract void SetValue(T value);
        public abstract T GetValue();

        public TSDisposableHelper(T value)
        {
            OldValue = GetValue();
            SetValue(value);
        }

        public virtual void Dispose()
        {
            SetValue(OldValue);
        }
    }


    public class ScrollPosition(Vector2 start)
    {
        public Vector2 Current = start;
    }


    public class TextSize_D(GameFont value) : TSDisposableHelper<GameFont>(value)
    {
        public override GameFont GetValue() => Text.Font;
        public override void SetValue(GameFont value) => Text.Font = value;
    }

    public class GUIColor_D(Color value) : TSDisposableHelper<Color>(value)
    {
        public override Color GetValue() => GUI.color;
        public override void SetValue(Color value) => GUI.color = value;
    }

    public class ActiveRT_D(RenderTexture value) : TSDisposableHelper<RenderTexture>(value)
    {
        public override RenderTexture GetValue() => RenderTexture.active;
        public override void SetValue(RenderTexture value)
        {
            RenderTexture.active = value;
            Graphics.SetRenderTarget(value);
        }
    }

    public class Listing_D(Rect value) : TSDisposableHelper<Rect>(value)
    {
        public readonly Listing_Standard Listing = new();

        public Rect GetRect(float height, float width_pct = 1)
        {
            return Listing.GetRect(height, width_pct);
        }

        public override Rect GetValue() => Listing.listingRect;
        public override void SetValue(Rect value) => Listing.Begin(value);

        public override void Dispose()
        {
            Listing.End();
        }
    }

    public class TextAnchor_D(TextAnchor value) : TSDisposableHelper<TextAnchor>(value)
    {
        public override TextAnchor GetValue() => Text.Anchor;
        public override void SetValue(TextAnchor value) => Text.Anchor = value;
    }

    public class Scroll_D(Rect area, Rect content_rect, ScrollPosition? scroll_pos) : TSDisposableHelper<(Rect, Rect)>((area, content_rect))
    {
        public override (Rect, Rect) GetValue() => (area, content_rect);

        public override void SetValue((Rect, Rect) value)
        {
            if (scroll_pos is not null)
            {
                BeginScrollView(area, ref scroll_pos.Current, content_rect, false, true);
                //Widgets.BeginScrollView(area, ref scroll_pos.Current, content_rect);
            }
        }

        public override void Dispose()
        {
            if (scroll_pos is not null)
                Widgets.EndScrollView();
        }
    }
}
