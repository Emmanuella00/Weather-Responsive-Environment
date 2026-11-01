using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprites generated in code (rounded rectangles, a circle, a spinner arc), so the UI needs
/// no imported art and nothing can go missing in a Web build.
/// Sprites use 100 pixels per unit, so one sprite pixel equals one canvas unit and a
/// rounded-rect radius of 20 means a 20-unit corner.
/// </summary>
public static class UiSprites
{
    const int Size = 64;

    static readonly Dictionary<int, Sprite> RoundedCache = new Dictionary<int, Sprite>();
    static Sprite circle;
    static Sprite spinner;

    /// <summary>9-sliced rounded rectangle. Use it with Image.Type.Sliced.</summary>
    public static Sprite RoundedRect(int radius)
    {
        radius = Mathf.Clamp(radius, 2, Size / 2 - 1);
        if (RoundedCache.TryGetValue(radius, out Sprite cached) && cached != null) return cached;

        Sprite sprite = Build(
            (x, y) =>
            {
                float half = Size * 0.5f;
                float qx = Mathf.Abs(x - half) - (half - radius);
                float qy = Mathf.Abs(y - half) - (half - radius);
                float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                float dist = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                return Mathf.Clamp01(0.5f - dist);
            },
            new Vector4(radius, radius, radius, radius));

        RoundedCache[radius] = sprite;
        return sprite;
    }

    public static Sprite Circle()
    {
        if (circle != null) return circle;

        circle = Build(
            (x, y) =>
            {
                float half = Size * 0.5f;
                float len = new Vector2(x - half, y - half).magnitude;
                return Mathf.Clamp01(half - len);
            },
            Vector4.zero);
        return circle;
    }

    /// <summary>A ring with a fading tail. Rotate the Image to make a loading spinner.</summary>
    public static Sprite Spinner()
    {
        if (spinner != null) return spinner;

        spinner = Build(
            (x, y) =>
            {
                float half = Size * 0.5f;
                float dx = x - half;
                float dy = y - half;
                float len = Mathf.Sqrt(dx * dx + dy * dy);
                float ring = Mathf.Clamp01(30f - len + 0.5f) * Mathf.Clamp01(len - 21f + 0.5f);
                float t = (Mathf.Atan2(dy, dx) + Mathf.PI) / (2f * Mathf.PI);
                return ring * Mathf.Lerp(0.1f, 1f, t);
            },
            Vector4.zero);
        return spinner;
    }

    static Sprite Build(Func<float, float, float> alphaAt, Vector4 border)
    {
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var pixels = new Color32[Size * Size];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                byte alpha = (byte)Mathf.RoundToInt(255f * alphaAt(x + 0.5f, y + 0.5f));
                pixels[y * Size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f),
                             100f, 0, SpriteMeshType.FullRect, border);
    }
}
