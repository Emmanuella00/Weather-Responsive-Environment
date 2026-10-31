using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;


public sealed class LiveWeatherProvider : IWeatherProvider
{
    const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

    readonly ApiConfig config;
    readonly float timeoutSeconds;

    public string DisplayName => "Live (OpenWeather)";
    public bool IsTestMode => false;

    public LiveWeatherProvider(ApiConfig config, float timeoutSeconds = 10f)
    {
        this.config = config;
        this.timeoutSeconds = timeoutSeconds;
    }

    public IEnumerator Fetch(double latitude, double longitude, Action<FetchResult> onComplete)
    {
        string key = config != null && config.apiKey != null ? config.apiKey.Trim() : "";
        if (key.Length == 0)
        {
            onComplete(FetchResult.Fail(WeatherErrorKind.NoApiKey,
                "No API key configured. See the README section on setting up your key."));
            yield break;
        }

        string url = BuildUrl(latitude, longitude, key);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!operation.isDone)  
            {
                if (Time.realtimeSinceStartup >= deadline)
                {
                    request.Abort();
                    onComplete(FetchResult.Fail(WeatherErrorKind.Timeout,
                        "The request timed out. Check your connection and try again."));
                    yield break;
                }
                yield return null;
            }

            onComplete(Interpret(request));
        }
    }

    static string BuildUrl(double lat, double lon, string key)
    {
        // a comma decimal separator would break the query string.
        string latText = lat.ToString("0.######", CultureInfo.InvariantCulture);
        string lonText = lon.ToString("0.######", CultureInfo.InvariantCulture);
        return BaseUrl + "?lat=" + latText + "&lon=" + lonText +
               "&appid=" + Uri.EscapeDataString(key) + "&units=metric";
    }

    static FetchResult Interpret(UnityWebRequest request)
    {
        int status = (int)request.responseCode;

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                return FetchResult.Ok(request.downloadHandler != null ? request.downloadHandler.text : null);

            case UnityWebRequest.Result.ProtocolError:
                return FetchResult.Fail(WeatherErrorKind.HttpError, DescribeHttpStatus(status, request), status);

            default:
                return FetchResult.Fail(WeatherErrorKind.Network,
                    "Could not reach the weather service. Check your internet connection.", status);
        }
    }

    static string DescribeHttpStatus(int status, UnityWebRequest request)
    {
        if (status == 401) return "The API key was rejected. Check that it is correct and active.";
        if (status == 404) return "No weather data was found for this location.";
        if (status == 429) return "Too many requests. Please wait a moment and try again.";
        if (status >= 500) return "The weather service is having problems. Try again later.";

        string detail = TryReadApiMessage(request);
        return detail != null
            ? "The weather service reported an error: " + detail
            : "The weather service returned an error (HTTP " + status + ").";
    }

    static string TryReadApiMessage(UnityWebRequest request)
    {
        try
        {
            string body = request.downloadHandler != null ? request.downloadHandler.text : null;
            if (string.IsNullOrWhiteSpace(body)) return null;
            ApiError error = JsonUtility.FromJson<ApiError>(body);
            return error != null && !string.IsNullOrWhiteSpace(error.message) ? error.message : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
