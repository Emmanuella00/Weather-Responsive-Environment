using System;
using System.Globalization;

public enum WeatherCategory
{
    Clear,
    Clouds,
    Rain,        // thunderstorm, drizzle and rain codes
    Snow,
    Atmosphere,  // mist, fog, haze, smoke... (7xx)
    Fallback     // anything unsupported: gets a neutral overcast look
}

/// <summary>
/// Clean domain model. The UI and the environment only ever see this class.
/// Optional values are nullable: null means "not in the response" and must be shown as N/A, never as 0.
/// </summary>
public sealed class WeatherData
{
    // Origin of the data. True when it came from the sample (test mode) provider.
    public bool IsTestData { get; internal set; }

    // Location
    public double RequestedLatitude { get; internal set; }
    public double RequestedLongitude { get; internal set; }
    public string LocationName { get; internal set; } = "";   // empty when the API returns none
    public string CountryCode { get; internal set; } = "";    // empty when missing

    // Condition
    public int ConditionId { get; internal set; }
    public string ConditionGroup { get; internal set; } = "";
    public string Description { get; internal set; } = "";
    public string IconCode { get; internal set; } = "";
    public WeatherCategory Category { get; internal set; } = WeatherCategory.Fallback;

    // Time of day AT THE LOCATION (never the computer's clock)
    public bool IsDayKnown { get; internal set; }
    public bool IsDay { get; internal set; }                  // only meaningful when IsDayKnown
    public long ObservationUnix { get; internal set; }
    public DateTime? LocalTime { get; internal set; }         // observation time shifted by the location's offset

    // Measurements (null = missing)
    public float? TemperatureC { get; internal set; }
    public int? HumidityPercent { get; internal set; }
    public float? WindSpeedMs { get; internal set; }
    public int? CloudCoverPercent { get; internal set; }

    /// <summary>"Name, CC" when available, otherwise the requested coordinates.</summary>
    public string LocationLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(LocationName))
                return string.IsNullOrWhiteSpace(CountryCode) ? LocationName : LocationName + ", " + CountryCode;

            return string.Format(CultureInfo.InvariantCulture,
                "Lat {0:0.00}, Lon {1:0.00}", RequestedLatitude, RequestedLongitude);
        }
    }
}