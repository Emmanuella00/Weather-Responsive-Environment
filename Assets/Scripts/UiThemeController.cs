using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Card and panel colors for every weather state. Day colors are dark enough for white
/// text (roughly 4.5:1 contrast or better); night colors are darker still. Edit here to retheme.
/// </summary>
public static class UiPalette
{
    static Color Hex(uint rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);

    public static Color For(WeatherCategory category, bool isDay)
    {
        switch (category)
        {
            case WeatherCategory.Clear:      return Hex(isDay ? 0x2A6FC4u : 0x1F3A66u);
            case WeatherCategory.Clouds:     return Hex(isDay ? 0x46749Fu : 0x2B3F63u);
            case WeatherCategory.Rain:       return Hex(isDay ? 0x4D6579u : 0x26426Fu);
            case WeatherCategory.Snow:       return Hex(isDay ? 0x53728Fu : 0x33476Bu);
            case WeatherCategory.Atmosphere: return Hex(isDay ? 0x5E6F7Bu : 0x2F3A4Du);
            default:                         return Hex(isDay ? 0x58708Cu : 0x2D3A58u);
        }
    }
}

/// <summary>
/// Tints the card and the side panel to match the weather, from the same event the
/// 3D scene listens to. Put it on an always-active object, such as the Canvas.
/// </summary>
public class UiThemeController : MonoBehaviour
{
    [SerializeField] WeatherService service;
    [SerializeField] Image[] solidTargets;     // e.g. the weather card background
    [SerializeField] Image[] panelTargets;     // e.g. the location panel (slightly see-through)

    const float PanelAlpha = 0.92f;

    void OnEnable()
    {
        if (service == null) { Debug.LogWarning("UiThemeController: assign the WeatherService."); return; }
        service.OnWeatherLoaded += HandleWeatherLoaded;
        service.OnProviderChanged += HandleProviderChanged;
    }

    void OnDisable()
    {
        if (service == null) return;
        service.OnWeatherLoaded -= HandleWeatherLoaded;
        service.OnProviderChanged -= HandleProviderChanged;
    }

    void Start() => Apply(WeatherCategory.Fallback, true);

    void HandleWeatherLoaded(WeatherData data) =>
        Apply(data.Category, !data.IsDayKnown || data.IsDay);

    void HandleProviderChanged(bool isTestMode) => Apply(WeatherCategory.Fallback, true);

    void Apply(WeatherCategory category, bool isDay)
    {
        Color color = UiPalette.For(category, isDay);

        foreach (Image target in solidTargets)
            if (target != null) target.color = color;

        foreach (Image target in panelTargets)
            if (target != null) target.color = new Color(color.r, color.g, color.b, PanelAlpha);
    }
}
