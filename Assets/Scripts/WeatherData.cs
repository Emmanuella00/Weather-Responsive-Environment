using System;
using System.Globalization;

public enum WeatherCategory
{
    Clear,
    Clouds,
    Rain,        
    Snow,
    Atmosphere,  // mist, fog, haze, smoke
    Fallback     // anything unsupported: gets a neutral overcast look
}


public sealed class WeatherData
{
    public double RequestedLatitude { get; internal set; }
    public double RequestedLongitude { get; internal set; }
    public string LocationName { get; internal set; } = "";   
    public string CountryCode { get; internal set; } = "";    

    public int ConditionId { get; internal set; }
    public string ConditionGroup { get; internal set; } = "";
    public string Description { get; internal set; } = "";
    public string IconCode { get; internal set; } = "";
    public WeatherCategory Category { get; internal set; } = WeatherCategory.Fallback;

    public bool IsDayKnown { get; internal set; }
    public bool IsDay { get; internal set; }                  
    public long ObservationUnix { get; internal set; }
    public DateTime? LocalTime { get; internal set; }         

    public float? TemperatureC { get; internal set; }
    public int? HumidityPercent { get; internal set; }
    public float? WindSpeedMs { get; internal set; }
    public int? CloudCoverPercent { get; internal set; }

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
