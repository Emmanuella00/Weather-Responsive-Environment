using UnityEngine;

/// <summary>
/// A group of cloud puffs (flattened spheres) as one switchable effect.
/// Put this on a parent object; every Renderer below it counts as one puff.
/// Cloud cover decides how many puffs are visible (extra credit: cloud coverage).
/// </summary>
public sealed class CloudLayerEffect : MonoBehaviour, IWeatherEffect
{
    Renderer[] puffs;

    // Looked up lazily: Awake does not run while the object is inactive.
    Renderer[] Puffs
    {
        get
        {
            if (puffs == null) puffs = GetComponentsInChildren<Renderer>(true);
            return puffs;
        }
    }

    public void SetTint(Color color)
    {
        foreach (Renderer puff in Puffs) RendererTint.Apply(puff, color);
    }

    /// <param name="coverage01">0 to 1. At least one puff always stays visible.</param>
    public void SetCoverage(float coverage01)
    {
        Renderer[] all = Puffs;
        if (all.Length == 0) return;

        int visible = Mathf.Clamp(Mathf.CeilToInt(all.Length * Mathf.Clamp01(coverage01)), 1, all.Length);
        for (int i = 0; i < all.Length; i++) all[i].enabled = i < visible;
    }

    public void Activate() => gameObject.SetActive(true);
    public void Deactivate() => gameObject.SetActive(false);
}
