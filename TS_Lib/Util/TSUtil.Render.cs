using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            float rotation = 0
        ) {
            scale ??= Vector2.one;
            bool temp_mat = false;
            using (new ActiveRT_D(dest))
            {
                GL.PushMatrix();
                GL.LoadOrtho();

                // Create material with source texture if needed
                if (source != null)
                {
                    mat = new Material(mat) { mainTexture = source };
                    temp_mat = true;
                }

                if (!mat.SetPass(0))
                {
                    Log.Error("unable to set material to render");
                    GL.PopMatrix();
                    return;
                }

                // Use the exact same matrix calculation as the working GL approach
                Matrix4x4 matrix = Matrix4x4.TRS(
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
                ) * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f));

                GL.MultMatrix(matrix);
                Graphics.DrawTexture(new Rect(0, 1, 1, -1), source);

                GL.PopMatrix();
                if (temp_mat)
                    UnityEngine.Object.DestroyImmediate(mat);
            }
        }
    }
}
