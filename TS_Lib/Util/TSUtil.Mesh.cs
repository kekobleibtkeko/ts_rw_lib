using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TS_Lib.Util;

public static partial class TSUtil
{
    public static Dictionary<Mesh, Mesh> FlipCache = [];

    public static void TransformVertices(
         this Mesh mesh,
         Vector3? translation = null,
         Quaternion? rotation = null,
         Vector3? scale = null)
    {
        if (mesh == null) return;

        Vector3[] vertices = mesh.vertices;

        // Default to identity transform
        Vector3 t = translation ?? Vector3.zero;
        Quaternion r = rotation ?? Quaternion.identity;
        Vector3 s = scale ?? Vector3.one;

        Matrix4x4 matrix = Matrix4x4.TRS(t, r, s);

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public static Mesh GetFlippedMesh(this Mesh original)
    {
        if (!FlipCache.TryGetValue(original, out var flipped))
        {
            flipped = UnityEngine.Object.Instantiate(original);
            Vector3[] vertices = flipped.vertices;

            // Invert X coordinate of each vertex
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].x = -vertices[i].x;

            flipped.vertices = vertices;

            // Reverse triangle winding
            for (int i = 0; i < flipped.subMeshCount; i++)
            {
                int[] triangles = flipped.GetTriangles(i);
                System.Array.Reverse(triangles);
                flipped.SetTriangles(triangles, i);
            }

            flipped.RecalculateNormals(); // Optional, but good for correctness
            flipped.RecalculateBounds();
            FlipCache.Add(original, flipped);
        }
        return flipped;
    }
}
