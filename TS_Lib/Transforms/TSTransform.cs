using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_Lib.Save;
using UnityEngine;
using Verse;

namespace TS_Lib.Transforms;

public class TSTransform : IExposable
{
    public Rot4 Rotation;
    public Vector2 Pivot = DrawData.PivotCenter;
    public Vector3 Offset;
    public Vector2 Scale = Vector2.one;
    public float RotationOffset;

    [Obsolete("do not use")] public TSTransform() { }
    public TSTransform(Rot4 rotation)
    {
        Rotation = rotation;
    }

    public TSTransform CreateCopy()
    {
        return new(Rotation)
        {
            Pivot = Pivot,
            Offset = Offset,
            Scale = Scale,
            RotationOffset = RotationOffset
        };
    }

    public void CopyFrom(TSTransform other)
    {
        Rotation = other.Rotation;
        Pivot = other.Pivot;
        Offset = other.Offset;
        Scale = other.Scale;
        RotationOffset = other.RotationOffset;
    }

    public TSTransform Mirror()
    {
        return new(Rotation.Opposite)
        {
            Offset = new(-Offset.x, Offset.y, Offset.z),
            Scale = new(Scale.x, Scale.y),
            RotationOffset = 360 - RotationOffset
        };
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref Rotation, "rotation");
        Scribe_Values.Look(ref RotationOffset, "rotoffset");
        Scribe_Values.Look(ref Pivot, "pivot", DrawData.PivotCenter);
        TSSaveUtility.LookAccurate(ref Offset, "offset", default);
        TSSaveUtility.LookAccurate(ref Scale, "scale", Vector2.one);
    }
}

public class TSTransform4 : IExposable
{
    public TSTransform TransformLeft;
    public TSTransform TransformRight;
    public TSTransform TransformUp;
    public TSTransform TransformDown;

    public TSTransform4()
    {
        TransformLeft ??= new(Rot4.West);
        TransformRight ??= new(Rot4.East);
        TransformUp ??= new(Rot4.North);
        TransformDown ??= new(Rot4.South);
    }

    public void CopyFrom(TSTransform4 other)
    {
        TransformLeft.CopyFrom(other.TransformLeft);
        TransformRight.CopyFrom(other.TransformRight);
        TransformUp.CopyFrom(other.TransformUp);
        TransformDown.CopyFrom(other.TransformDown);
    }

    public TSTransform4 CreateCopy()
    {
        return new()
        {
            TransformLeft = TransformLeft.CreateCopy(),
            TransformRight = TransformRight.CreateCopy(),
            TransformUp = TransformUp.CreateCopy(),
            TransformDown = TransformDown.CreateCopy()
        };
    }

    public TSTransform4 Mirror()
    {
        return new()
        {
            TransformLeft = TransformLeft.Mirror(),
            TransformRight = TransformRight.Mirror(),
            TransformUp = TransformUp.Mirror(),
            TransformDown = TransformDown.Mirror(),
        };
    }

    public TSTransform ForRot(Rot4 rot)
    {
        return rot.AsInt switch
        {
            Rot4.EastInt => TransformRight,
            Rot4.WestInt => TransformLeft,
            Rot4.NorthInt => TransformUp,
            Rot4.SouthInt or _ => TransformDown
        };
    }

    public void ExposeData()
    {
        Scribe_Deep.Look(ref TransformLeft, "left");
        Scribe_Deep.Look(ref TransformRight, "right");
        Scribe_Deep.Look(ref TransformUp, "up");
        Scribe_Deep.Look(ref TransformDown, "down");
    }
}