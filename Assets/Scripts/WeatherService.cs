using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Orchestrates one weather request at a time from the user's point of view:
/// provider -> parser -> events. UI and environment subscribe; they never call the API.
/// </summary>
public class WeatherService : MonoBehaviour
{
    [SerializeField] ApiConfig apiConfig;
    [SerializeField, Min(1f)] float timeoutSeconds = 10f;

    public event Action<WeatherData> OnWeatherLoaded;
    public event Action<WeatherError> OnWeatherError;
    public event Action<bool> OnLoadingChanged;
    /// <summary>Raised when the data source changes. The argument is true for TEST MODE.</summary>
    public event Action<bool> OnProviderChanged;

    public bool IsLoading { get; private set; }
    public bool IsTestMode => provider != null && provider.IsTestMode;
    public string ProviderName => provider != null ? provider.DisplayName : "None";

    /// <summary>Coordinates of the most recent request (0, 0 before the first one).</summary>
    public double LastLatitude { get; private set; }
    public double LastLongitude { get; private set; }

    IWeatherProvider provider;
    int latestRequestId;   // only the newest request may touch loading state or raise events

    void Awake()
    {
        if (provider == null) UseLiveProvider();
    }

    void OnDisable()
    {
        // Disabling stops our coroutines silently, so invalidate them and reset loading.
        latestRequestId++;
        SetLoading(false);
    }

    public void SetProvider(IWeatherProvider newProvider)
    {
        if (newProvider == null) UseLiveProvider();
        else ApplyProvider(newProvider);
    }

    public void UseLiveProvider() => ApplyProvider(new LiveWeatherProvider(apiConfig, timeoutSeconds));

    void ApplyProvider(IWeatherProvider newProvider)
    {
        // A response still in flight belongs to the old provider: drop it so live and
        // test data can never be mixed up on screen.
        latestRequestId++;
        SetLoading(false);

        provider = newProvider;
        OnProviderChanged?.Invoke(provider.IsTestMode);
    }

    /// <summary>Starts a request. A newer call supersedes any request still in flight.</summary>
    public void RequestWeather(double latitude, double longitude)
    {
        if (provider == null) UseLiveProvider();

        LastLatitude = latitude;
        LastLongitude = longitude;

        int requestId = ++latestRequestId;
        SetLoading(true);
        StartCoroutine(RunRequest(requestId, latitude, longitude, provider));
    }

    IEnumerator RunRequest(int requestId, double latitude, double longitude, IWeatherProvider activeProvider)
    {
        FetchResult result = default;
        bool completed = false;

        yield return StartCoroutine(activeProvider.Fetch(latitude, longitude, r =>
        {
            result = r;
            completed = true;
        }));

        // A newer request exists (or the service was disabled): drop this response entirely.
        if (requestId != latestRequestId) yield break;

        if (!completed)
        {
            result = FetchResult.Fail(WeatherErrorKind.Network, "The weather request ended unexpectedly.");
        }

        if (!result.Success)
        {
            Finish(result.Error, null);
            yield break;
        }

        ParseResult parsed = WeatherParser.Parse(result.Json, latitude, longitude);
        if (parsed.Success)
        {
            // Stamp the data itself, so anything showing it can label it truthfully.
            parsed.Data.IsTestData = activeProvider.IsTestMode;
            Finish(default, parsed.Data);
        }
        else
        {
            Finish(parsed.Error, null);
        }
    }

    void Finish(WeatherError error, WeatherData data)
    {
        SetLoading(false);

        if (data != null)
        {
            Debug.Log((data.IsTestData ? "[TEST] " : "") +
                      $"Weather loaded: {data.LocationLabel} | id {data.ConditionId} -> {data.Category} | " +
                      (data.IsDayKnown ? (data.IsDay ? "day" : "night") : "day/night unknown"));
            OnWeatherLoaded?.Invoke(data);
        }
        else
        {
            Debug.LogWarning("Weather request failed: " + error);
            OnWeatherError?.Invoke(error);
        }
    }

    void SetLoading(bool value)
    {
        if (IsLoading == value) return;
        IsLoading = value;
        OnLoadingChanged?.Invoke(value);
    }
}