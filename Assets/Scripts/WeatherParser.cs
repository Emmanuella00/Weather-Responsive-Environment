using System;
using UnityEngine;

public readonly struct ParseResult
{
    public bool Success { get; }
    public WeatherData Data { get; }
    public WeatherError Error { get; }

    ParseResult(bool success, WeatherData data, WeatherError error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public static ParseResult Ok(WeatherData data) => new ParseResult(true, data, default);
    public static ParseResult Fail(WeatherErrorKind kind, string message) =>
        new ParseResult(false, null, new WeatherError(kind, message));
}

 
public static class WeatherParser
{
    const long MaxUnixSeconds = 253402300799L;  

    public static ParseResult Parse(string json, double requestedLat, double requestedLon)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ParseResult.Fail(WeatherErrorKind.ParseError, "The weather service returned an empty response.");

        WeatherResponse response;
        try
        {
            response = JsonUtility.FromJson<WeatherResponse>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Weather JSON could not be parsed: " + ex.GetType().Name);
            return ParseResult.Fail(WeatherErrorKind.ParseError, "The weather data could not be read.");
        }

        if (response == null)
            return ParseResult.Fail(WeatherErrorKind.ParseError, "The weather data could not be read.");

        if (response.weather == null || response.weather.Length == 0 || response.weather[0] == null)
            return ParseResult.Fail(WeatherErrorKind.InvalidData, "The response contained no weather condition.");

        WeatherInfo condition = response.weather[0];
        SysData sys = response.sys;

        
        var data = new WeatherData
        {
            RequestedLatitude = requestedLat,
            RequestedLongitude = requestedLon,
            LocationName = (response.name ?? "").Trim(),
            CountryCode = (sys != null && sys.country != null) ? sys.country.Trim() : "",

            ConditionId = condition.id,
            ConditionGroup = condition.main ?? "",
            Description = condition.description ?? "",
            IconCode = condition.icon ?? "",
            Category = WeatherCategoryMapper.MapCategory(condition.id),

            ObservationUnix = response.dt,

            TemperatureC = (response.main != null && HasKey(json, "temp") && IsFinite(response.main.temp))
                ? response.main.temp : (float?)null,
            HumidityPercent = (response.main != null && HasKey(json, "humidity"))
                ? response.main.humidity : (int?)null,
            WindSpeedMs = (response.wind != null && HasKey(json, "speed") && IsFinite(response.wind.speed))
                ? response.wind.speed : (float?)null,
            CloudCoverPercent = (response.clouds != null && HasKey(json, "all"))
                ? response.clouds.all : (int?)null,
        };

        long sunrise = sys != null ? sys.sunrise : 0;
        long sunset = sys != null ? sys.sunset : 0;
        data.IsDayKnown = WeatherCategoryMapper.TryResolveDayNight(
            data.IconCode, response.dt, sunrise, sunset, out bool isDay);
        data.IsDay = isDay;

        if (response.dt > 0 && response.dt < MaxUnixSeconds && HasKey(json, "timezone"))
        {
            data.LocalTime = DateTimeOffset.FromUnixTimeSeconds(response.dt).UtcDateTime
                .AddSeconds(response.timezone);
        }

        return ParseResult.Ok(data);
    }

    static bool HasKey(string json, string key) =>
        json.IndexOf("\"" + key + "\"", StringComparison.Ordinal) >= 0;

    static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
