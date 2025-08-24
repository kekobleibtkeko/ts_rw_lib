using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TS_Lib.Util;


public struct TSText
{
    public static implicit operator string(TSText e) => e.Text;
    public static implicit operator TSText(string e) => new(e);
    public static TSText operator +(TSText lhs, TSText rhs) => lhs.Text + rhs.Text;
    public static TSText operator +(TSText lhs, string rhs) => lhs.Text + rhs;
    public static TSText operator +(string lhs, TSText rhs) => lhs + rhs.Text;
    public string Text;
    public TSText(string text)
    {
        Text = text;
    }
    public override string ToString()
    {
        return Text;
    }

    public TSText Cen() => Tag("center");
    public TSText It() => Tag("i");
    public TSText B() => Tag("b");
    public TSText Clr(string clr) => Tag("color", $"#{clr}");
    public TSText Clr(Color clr) => Clr(ColorUtility.ToHtmlStringRGBA(clr));
    public TSText Tag(string tag) => $"<{tag}>{Text}</{tag}>";
    public TSText Tag(string tag, string val) => $"<{tag}={val}>{Text}</{tag}>";
    public TSText Size(int percent) => Tag("size", $"{percent}%");
}

