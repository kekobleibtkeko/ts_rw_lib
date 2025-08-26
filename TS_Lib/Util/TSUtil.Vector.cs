using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TS_Lib.Util;

public static partial class TSUtil
{
    public static Vector3 GetUpVector(float height) => new(0f, height, 0f);

    public static Vector3 ToUpFacingVec3(this Vector2 vec, float y = 0f) => new(vec.x, y, vec.y);
    public static Vector2 FromUpFacingVec3(this Vector3 vec) => new(vec.x, vec.z);
}
