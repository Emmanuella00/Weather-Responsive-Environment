using UnityEngine;

/// <summary>
/// Sets a renderer's main color without caring which shader it uses
/// (URP shaders call it _BaseColor, older ones _Color).
/// </summary>
public static class RendererTint
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    public static void Apply(Renderer renderer, Color color)
    {
        if (renderer == null) return;

        Material material = renderer.material;   // per-renderer instance: shared assets stay untouched
        if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, color);
        if (material.HasProperty(ColorId)) material.SetColor(ColorId, color);
    }
}
