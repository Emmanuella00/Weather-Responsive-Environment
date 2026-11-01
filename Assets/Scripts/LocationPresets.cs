/// <summary>One named place with its coordinates, stored in the application.</summary>
public readonly struct LocationPreset
{
    public string Name { get; }
    public string CountryCode { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public string Label => Name + ", " + CountryCode;

    public LocationPreset(string name, string countryCode, double latitude, double longitude)
    {
        Name = name;
        CountryCode = countryCode;
        Latitude = latitude;
        Longitude = longitude;
    }
}

/// <summary>
/// The dropdown's locations. Chosen for different time zones and usually different weather.
/// Add or remove entries here; the dropdown is built from this list.
/// </summary>
public static class LocationPresets
{
    public static readonly LocationPreset[] All =
    {
        new LocationPreset("Kigali",    "RW",  -1.9441,   30.0619),
        new LocationPreset("Tokyo",     "JP",  35.6762,  139.6503),
        new LocationPreset("Reykjavik", "IS",  64.1466,  -21.9426),
        new LocationPreset("Sydney",    "AU", -33.8688,  151.2093),
        new LocationPreset("Vancouver", "CA",  49.2827, -123.1207),
        new LocationPreset("Dubai",     "AE",  25.2048,   55.2708),
        new LocationPreset("Singapore", "SG",   1.3521,  103.8198),
        new LocationPreset("Anchorage", "US",  61.2181, -149.9003),
    };
}
