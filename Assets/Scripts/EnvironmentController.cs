using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Turns weather data into the 3D scene's appearance. Listens to WeatherService.OnWeatherLoaded,
/// exactly like the UI, so both always show the same response. It never touches raw API codes:
/// only WeatherData.Category and the day/night flag.
/// On an error it keeps the last good appearance (the UI shows the error).
/// </summary>
public class EnvironmentController : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] WeatherService service;

    [Header("Scene")]
    [SerializeField] Light sunLight;          // the directional light: sun by day, moon by night
    [SerializeField] Camera skyCamera;        // its background color is the sky (falls back to Camera.main)
    [SerializeField] Renderer ground;

    [Header("Effects")]
    [SerializeField] CloudLayerEffect clouds;
    [SerializeField] ParticleWeatherEffect rain;
    [SerializeField] ParticleWeatherEffect snow;

    [Header("Optional")]
    [SerializeField] GameObject sunDisc;      // shown on clear days
    [SerializeField] GameObject moonDisc;     // shown on clear nights
    [SerializeField] GameObject[] nightOnly;  // e.g. a lit window: on at night, off by day

    readonly List<IWeatherEffect> effects = new List<IWeatherEffect>();

    public WeatherCategory CurrentCategory { get; private set; } = WeatherCategory.Fallback;
    public bool CurrentIsDay { get; private set; } = true;

    void Awake()
    {
        if (skyCamera == null) skyCamera = Camera.main;

        if (clouds != null) effects.Add(clouds);
        if (rain != null) effects.Add(rain);
        if (snow != null) effects.Add(snow);

        // One-time render setup, so the scene does not depend on how the Lighting window was left.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        if (skyCamera != null) skyCamera.clearFlags = CameraClearFlags.SolidColor;

        if (sunLight == null) Debug.LogWarning("EnvironmentController: no Sun Light assigned.");
        if (skyCamera == null) Debug.LogWarning("EnvironmentController: no camera found for the sky.");
        if (clouds == null || rain == null || snow == null)
            Debug.LogWarning("EnvironmentController: a cloud, rain or snow effect is not assigned.");
    }

    void OnEnable()
    {
        if (service == null)
        {
            Debug.LogWarning("EnvironmentController: assign the WeatherService.");
            return;
        }
        service.OnWeatherLoaded += HandleWeatherLoaded;
    }

    void OnDisable()
    {
        if (service != null) service.OnWeatherLoaded -= HandleWeatherLoaded;
    }

    // Neutral overcast day until the first response arrives.
    void Start() => ApplyState(WeatherCategory.Fallback, true, null);

    void HandleWeatherLoaded(WeatherData data)
    {
        // If day/night could not be determined, show a neutral day rather than guessing night.
        bool isDay = !data.IsDayKnown || data.IsDay;
        ApplyState(data.Category, isDay, data.CloudCoverPercent);
    }

    /// <summary>
    /// Switches the whole scene to one state. Kept as one method so a smooth transition
    /// (extra credit) can later replace the jump with a coroutine that lerps these values.
    /// </summary>
    void ApplyState(WeatherCategory category, bool isDay, int? cloudCoverPercent)
    {
        EnvironmentLook look = EnvironmentLooks.For(category, isDay);
        CurrentCategory = category;
        CurrentIsDay = isDay;

        // 1. Stop everything from the previous state.
        foreach (IWeatherEffect effect in effects) effect.Deactivate();

        // 2. Lighting, sky, fog and ground.
        ApplyLighting(look);
        ApplyCelestial(category, isDay);

        // 3. Switch on only what this state needs.
        if (look.Clouds && clouds != null)
        {
            clouds.SetTint(look.CloudColor);
            clouds.SetCoverage(cloudCoverPercent.HasValue ? cloudCoverPercent.Value / 100f : 1f);
            clouds.Activate();
        }
        if (look.Rain && rain != null) rain.Activate();
        if (look.Snow && snow != null) snow.Activate();

        Debug.Log("Environment: " + Describe());
    }

    void ApplyLighting(EnvironmentLook look)
    {
        if (sunLight != null)
        {
            sunLight.color = look.LightColor;
            sunLight.intensity = look.LightIntensity;
            sunLight.transform.rotation = Quaternion.Euler(look.LightPitch, sunLight.transform.eulerAngles.y, 0f);
        }

        RenderSettings.ambientLight = look.Ambient;
        RenderSettings.fog = look.FogDensity > 0f;
        RenderSettings.fogColor = look.Sky;
        RenderSettings.fogDensity = look.FogDensity;

        if (skyCamera != null) skyCamera.backgroundColor = look.Sky;
        RendererTint.Apply(ground, look.Ground);
    }

    void ApplyCelestial(WeatherCategory category, bool isDay)
    {
        if (sunDisc != null) sunDisc.SetActive(category == WeatherCategory.Clear && isDay);
        if (moonDisc != null) moonDisc.SetActive(category == WeatherCategory.Clear && !isDay);

        if (nightOnly == null) return;
        foreach (GameObject item in nightOnly)
            if (item != null) item.SetActive(!isDay);
    }

    public string Describe() => CurrentCategory + " / " + (CurrentIsDay ? "day" : "night");
}
