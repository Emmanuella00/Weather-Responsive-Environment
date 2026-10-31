using UnityEngine;

/// <summary>One complete appearance: lighting, sky, fog, ground and which effects are on.</summary>
public readonly struct EnvironmentLook
{
    public readonly Color LightColor;
    public readonly float LightIntensity;
    public readonly float LightPitch;      // sun/moon angle above the horizon, degrees
    public readonly Color Ambient;         // flat ambient light
    public readonly Color Sky;             // camera background; also used as the fog color
    public readonly float FogDensity;      // 0 = fog off
    public readonly Color Ground;
    public readonly bool Clouds;
    public readonly Color CloudColor;
    public readonly bool Rain;
    public readonly bool Snow;

    public EnvironmentLook(Color lightColor, float lightIntensity, float lightPitch, Color ambient, Color sky,
                           float fogDensity, Color ground, bool clouds, Color cloudColor, bool rain, bool snow)
    {
        LightColor = lightColor;
        LightIntensity = lightIntensity;
        LightPitch = lightPitch;
        Ambient = ambient;
        Sky = sky;
        FogDensity = fogDensity;
        Ground = ground;
        Clouds = clouds;
        CloudColor = cloudColor;
        Rain = rain;
        Snow = snow;
    }
}

/// <summary>
/// The look table: every weather category in day and night. This is the file to edit when tuning.
/// Colors are hex RGB. Weather and time of day are independent, so "snow at night" is just
/// the SnowNight row. Unknown conditions use the Fallback rows (neutral overcast).
/// </summary>
public static class EnvironmentLooks
{
    const float DayPitch = 50f;
    const float NightPitch = 30f;

    static Color Hex(uint rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);

    // cloud = 0 means "no clouds".
    static EnvironmentLook Make(uint light, float intensity, float pitch, uint ambient, uint sky,
                                float fog, uint ground, uint cloud = 0, bool rain = false, bool snow = false) =>
        new EnvironmentLook(Hex(light), intensity, pitch, Hex(ambient), Hex(sky), fog, Hex(ground),
                            cloud != 0, Hex(cloud), rain, snow);

    //                                      light    intens pitch      ambient  sky      fog     ground    clouds
    static readonly EnvironmentLook ClearDay      = Make(0xFFF4D6, 1.2f,  DayPitch,   0x8FA8C0, 0x8EC5E8, 0f,     0x6FA35B);
    static readonly EnvironmentLook ClearNight    = Make(0x7F93C9, 0.35f, NightPitch, 0x2A3858, 0x0E1730, 0f,     0x6FA35B);

    static readonly EnvironmentLook CloudsDay     = Make(0xEAF0F5, 0.9f,  DayPitch,   0x8C9FB0, 0xA9C3D6, 0f,     0x668F55, cloud: 0xF4F6F8);
    static readonly EnvironmentLook CloudsNight   = Make(0x6C7FB5, 0.3f,  NightPitch, 0x27334F, 0x1A233A, 0f,     0x668F55, cloud: 0x4A5673);

    static readonly EnvironmentLook RainDay       = Make(0xC5D0DA, 0.6f,  DayPitch,   0x7C8C99, 0x8D9FAE, 0.015f, 0x58804C, cloud: 0x9AA6B2, rain: true);
    static readonly EnvironmentLook RainNight     = Make(0x5A6A95, 0.25f, NightPitch, 0x222C44, 0x151D2E, 0.02f,  0x58804C, cloud: 0x3B465E, rain: true);

    static readonly EnvironmentLook SnowDay       = Make(0xE6EDF3, 0.8f,  DayPitch,   0xA5B4C2, 0xB7C3CD, 0.02f,  0xF2F5F8, cloud: 0xE3E8EC, snow: true);
    static readonly EnvironmentLook SnowNight     = Make(0x7F90B8, 0.3f,  NightPitch, 0x2F3C5A, 0x232D47, 0.02f,  0xF2F5F8, cloud: 0x59647F, snow: true);

    // Mist, fog, haze (7xx): heavy fog and a washed-out sky.
    static readonly EnvironmentLook AtmosphereDay   = Make(0xD8DDE0, 0.7f,  DayPitch,   0x9AA5AD, 0xB5BDC3, 0.05f,  0x7A8C72, cloud: 0xC9CFD4);
    static readonly EnvironmentLook AtmosphereNight = Make(0x5F6C8A, 0.25f, NightPitch, 0x252E42, 0x1E2533, 0.05f,  0x7A8C72, cloud: 0x3E4860);

    // Anything unsupported: neutral overcast.
    static readonly EnvironmentLook FallbackDay   = Make(0xD0D6DC, 0.7f,  DayPitch,   0x8E99A4, 0x9AA5B0, 0.01f,  0x6A8F5A, cloud: 0xAEB7C0);
    static readonly EnvironmentLook FallbackNight = Make(0x5D6A8A, 0.25f, NightPitch, 0x232C42, 0x1B2333, 0.01f,  0x6A8F5A, cloud: 0x3D4760);

    public static EnvironmentLook For(WeatherCategory category, bool isDay)
    {
        switch (category)
        {
            case WeatherCategory.Clear:      return isDay ? ClearDay : ClearNight;
            case WeatherCategory.Clouds:     return isDay ? CloudsDay : CloudsNight;
            case WeatherCategory.Rain:       return isDay ? RainDay : RainNight;
            case WeatherCategory.Snow:       return isDay ? SnowDay : SnowNight;
            case WeatherCategory.Atmosphere: return isDay ? AtmosphereDay : AtmosphereNight;
            default:                         return isDay ? FallbackDay : FallbackNight;
        }
    }
}
