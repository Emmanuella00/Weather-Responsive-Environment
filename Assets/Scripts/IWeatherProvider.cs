using System;
using System.Collections;

/// <summary>Outcome of a provider call: either raw JSON or an error. Never both.</summary>
public readonly struct FetchResult
{
    public bool Success { get; }
    public string Json { get; }
    public WeatherError Error { get; }

    FetchResult(bool success, string json, WeatherError error)
    {
        Success = success;
        Json = json;
        Error = error;
    }

    public static FetchResult Ok(string json) => new FetchResult(true, json, default);
    public static FetchResult Fail(WeatherError error) => new FetchResult(false, null, error);
    public static FetchResult Fail(WeatherErrorKind kind, string message, int httpStatus = 0) =>
        new FetchResult(false, null, new WeatherError(kind, message, httpStatus));
}


public interface IWeatherProvider
{
    string DisplayName { get; }
    bool IsTestMode { get; }    

    IEnumerator Fetch(double latitude, double longitude, Action<FetchResult> onComplete);
}
