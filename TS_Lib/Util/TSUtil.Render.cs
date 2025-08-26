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
        private static Mesh Quad = MakeXYQuad();
        static Mesh MakeXYQuad()
        {
            var mesh = new Mesh
            {
                vertices = [
                    new(-0.5f, -0.5f, 0f),
                    new( 0.5f, -0.5f, 0f),
                    new( 0.5f,  0.5f, 0f),
                    new(-0.5f,  0.5f, 0f)
                ],
                uv = [
                    new(0f, 0f),
                    new(1f, 0f),
                    new(1f, 1f),
                    new(0f, 1f)
                ],
                triangles = [
                    0, 1, 2,
                    0, 2, 3
                ]
            };
            mesh.RecalculateNormals();
            return mesh;
        }
        public static void BlitWithTransform(
            RenderTexture dest,
            Material mat,
            Texture? source = null,
            Vector2? scale = null,
            Vector2? offset = null,
            float rotation = 0f
        ) {
            if (source is not null)
            {
                mat = new(mat)
                {
                    mainTexture = source
                };
            }

            using (new TSUtil.ActiveRT_D(dest)) // sets the active render texture (including Graphics.SetRenderTarget)
            {
                GL.PushMatrix();
                //GL.wireframe = true;

                GL.LoadOrtho();

                if (!mat.SetPass(0))
                {
                    Log.Error("unable to set material to render");
                    return;
                }

                Matrix4x4 matrix =
                    Matrix4x4.TRS(
                        new Vector3(
                            0.5f + (offset?.x ?? 0f),
                            0.5f + (offset?.y ?? 0f),
                            0.5f
                        ),
                        Quaternion.Euler(0f, 0f, rotation),
                        new Vector3(
                            scale?.x ?? 1f,
                            scale?.y ?? 1f,
                            1f
                        )
                    )
                    * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f))
                ;
                Graphics.DrawMeshNow(MeshPool.plane10, matrix);
                //GL.wireframe = false;
                GL.PopMatrix();
            }
        }
    }
}
