using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TS_Lib.Util;

public static partial class TSUtil
{
    public static void Clear(this RenderTexture rt)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prev;
    }

    public static class BlitUtils
    {
        public static void BlitWithTransform(
            RenderTexture dest,
            Material mat,
            Texture? source = null,
            Vector2? scale = null,
            Vector2 offset = default,
            float rotation = 0,
            bool flip_x = false,
            bool flip_y = true
        ) {
            scale ??= Vector2.one;
            using (new ActiveRT_D(dest))
            {
                GL.PushMatrix();
                GL.LoadOrtho();
                Matrix4x4 matrix =
                    Matrix4x4.TRS(
                        new Vector3(
                            0.5f + offset.x,
                            0.5f + offset.y,
                            0.5f
                        ),
                        Quaternion.Euler(0f, 0f, rotation),
                        new Vector3(
                            scale.Value.x,
                            scale.Value.y,
                            1f
                        )
                    )
                    * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f))
                ;
                GL.MultMatrix(matrix);

                Vector2 x_part = flip_x
                    ? new(1, -1)
                    : new(0, 1)
                ;
                Vector2 y_part = flip_y
                    ? new(1, -1)
                    : new(0, 1)
                ;

                Rect img_rect = new(x_part.x, y_part.x, x_part.y, y_part.y);
                Graphics.DrawTexture(img_rect, source ?? mat.mainTexture, mat, 0);
                GL.PopMatrix();
            }
        }
    }
}
