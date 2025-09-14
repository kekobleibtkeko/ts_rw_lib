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
	public static IDictionary<(Texture2D, float), Texture2D> TextureRotateCache = new Dictionary<(Texture2D, float), Texture2D>();
	public static IDictionary<Texture2D, Texture2D> TextureFlipCache = new Dictionary<Texture2D, Texture2D>();

	/// <summary>
	/// from https://stackoverflow.com/questions/58873582/what-is-arraypool-in-netcore-c-sharp
	/// </summary>
	/// <param name="tex"></param>
	/// <param name="angleDegrees"></param>
	public static void RotateImage(Texture2D tex, float angleDegrees)
	{
		int width = tex.width;
		int height = tex.height;
		float halfHeight = height * 0.5f;
		float halfWidth = width * 0.5f;

		var texels = tex.GetRawTextureData<Color32>();
		// var copy = System.Buffers.ArrayPool<Color32>.Shared.Rent(texels.Length);
		var copy = new Color32[texels.Length];
		Unity.Collections.NativeArray<Color32>.Copy(texels, copy, texels.Length);

		float phi = Mathf.Deg2Rad * angleDegrees;
		float cosPhi = Mathf.Cos(phi);
		float sinPhi = Mathf.Sin(phi);

		int address = 0;
		for (int newY = 0; newY < height; newY++)
		{
			for (int newX = 0; newX < width; newX++)
			{
				float cX = newX - halfWidth;
				float cY = newY - halfHeight;
				int oldX = Mathf.RoundToInt(cosPhi * cX + sinPhi * cY + halfWidth);
				int oldY = Mathf.RoundToInt(-sinPhi * cX + cosPhi * cY + halfHeight);
				bool InsideImageBounds = (oldX > -1) & (oldX < width)
									& (oldY > -1) & (oldY < height);

				texels[address++] = InsideImageBounds ? copy[oldY * width + oldX] : default;
			}
		}

		// No need to reinitialize or SetPixels - data is already in-place.
		tex.Apply(true);
	}

	public static Texture2D Rotated(this Texture2D tex_gpu, float angle)
	{
		var rt = new RenderTexture(tex_gpu.width, tex_gpu.height, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
		var tex_cpu = new Texture2D(tex_gpu.width, tex_gpu.height);

		Graphics.Blit(tex_gpu, rt);

		using (new ActiveRT_D(rt))
			tex_cpu.ReadPixels(new(0, 0, tex_gpu.width, tex_gpu.height), 0, 0);

		RotateImage(tex_cpu, -angle);
		return tex_cpu;
	}

	public static Color GetPixelColor(this Texture2D tex, float x, float y)
	{
		Color pix;
		int x1 = (int)Mathf.Floor(x);
		int y1 = (int)Mathf.Floor(y);

		if (x1 > tex.width || x1 < 0 ||
		y1 > tex.height || y1 < 0)
		{
			pix = Color.clear;
		}
		else
		{
			pix = tex.GetPixel(x1, y1);
		}

		return pix;
	}

	public static float RotX(float angle, float x, float y)
	{
		float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
		float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
		return (x * cos + y * (-sin));
	}
	public static float RotY(float angle, float x, float y)
	{
		float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
		float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
		return (x * sin + y * cos);
	}

	public static Texture2D GetRotatedCached(this Texture2D tex, float angle)
	{
		var key = (tex, angle);
		if (!TextureRotateCache.TryGetValue(key, out var res))
		{
			res = TextureRotateCache[key] = tex.Rotated(angle);
		}
		return res;
	}
	
	public static void DrawFitted(this Texture2D? tex, Rect rect, Color? color = null, float scale = 1f, float rotation = 0f)
	{
		if (tex is null)
			return;
		if (rotation != 0)
				tex = tex.GetRotatedCached(rotation);

		using (new GUIColor_D(color ?? GUI.color))
			Widgets.DrawTextureFitted(rect, tex, scale, new Vector2(1, 1), new(0, 0, 1, 1));
	}
}
