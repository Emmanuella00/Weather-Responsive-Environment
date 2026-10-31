using System;
using System.Collections;
using UnityEngine;

 
public class WeatherService : MonoBehaviour
{
    [SerializeField] ApiConfig apiConfig;
    [SerializeField, Min(1f)] float timeoutSeconds = 10f;

    public event Action<WeatherData> OnWeatherLoaded;
    public event Action<WeatherError> OnWeatherError;
    public event Action<bool> OnLoadingChanged;

    public bool IsLoading { get; private set; }
    public bool IsTestMode => provider != null && provider.IsTestMode;
    public string ProviderName => provider != null ? provider.DisplayName : "None";

    IWeatherProvider provider;
    int latestRequestId;   

    void Awake()
    {
        if (provider == null) UseLiveProvider();
    }

    void OnDisable()
    {
        latestRequestId++;
        SetLoading(false);
    }

    public void SetProvider(IWeatherProvider newProvider)
    {
        if (newProvider == null) UseLiveProvider();
        else provider = newProvider;
    }

    public void UseLiveProvider() => provider = new LiveWeatherProvider(apiConfig, timeoutSeconds);

    public void RequestWeather(double latitude, double longitude)
    {
        if (provider == null) UseLiveProvider();

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
        if (parsed.Success) Finish(default, parsed.Data);
        else Finish(parsed.Error, null);
    }

    void Finish(WeatherError error, WeatherData data)
    {
        SetLoading(false);

        if (data != null)
        {
            Debug.Log($"Weather loaded: {data.LocationLabel} | id {data.ConditionId} -> {data.Category} | " +
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
