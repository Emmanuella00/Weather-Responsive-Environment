using System;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// TEMPORARY test harness (delete once the real UI exists).
/// Uses IMGUI so it needs no Canvas: just add it to a GameObject and assign the WeatherService.
/// Live mode: preset buttons and custom coordinates. Test mode: one button per sample scenario.
/// Shows results on screen because browser console logs are awkward to read in a WebGL build.
/// </summary>
public class PipelineDebugHud : MonoBehaviour
{
    [SerializeField] WeatherService service;
    [SerializeField, Range(1f, 3f)] float uiScale = 1.6f;

    readonly MockWeatherProvider mock = new MockWeatherProvider();
    readonly MockScenario[] scenarios = (MockScenario[])Enum.GetValues(typeof(MockScenario));

    string latText = "-1.9441";   // Kigali example
    string lonText = "30.0619";
    string status = "Idle.";
    string output = "";
    Vector2 scroll;

    void OnEnable()
    {
        if (service == null) { status = "Assign the WeatherService in the Inspector."; return; }
        service.OnWeatherLoaded += HandleLoaded;
        service.OnWeatherError += HandleError;
        service.OnLoadingChanged += HandleLoading;
        service.OnProviderChanged += HandleProviderChanged;
    }

    void OnDisable()
    {
        if (service == null) return;
        service.OnWeatherLoaded -= HandleLoaded;
        service.OnWeatherError -= HandleError;
        service.OnLoadingChanged -= HandleLoading;
        service.OnProviderChanged -= HandleProviderChanged;
    }

    void HandleLoading(bool loading) { if (loading) status = "Loading..."; }

    void HandleProviderChanged(bool isTestMode)
    {
        status = isTestMode ? "Switched to TEST MODE." : "Switched to LIVE.";
        output = "";
    }

    void HandleLoaded(WeatherData d)
    {
        status = "Success.";
        var sb = new StringBuilder();
        sb.AppendLine("Source:    " + (d.IsTestData ? "TEST SAMPLE (not real weather)" : "LIVE"));
        sb.AppendLine("Location:  " + d.LocationLabel);
        sb.AppendLine("Condition: " + d.ConditionId + " / " + d.ConditionGroup + " / \"" + d.Description + "\" / icon " + (d.IconCode.Length == 0 ? "(none)" : d.IconCode));
        sb.AppendLine("Category:  " + d.Category);
        sb.AppendLine("Time of day: " + (d.IsDayKnown ? (d.IsDay ? "Day" : "Night") : "Unknown"));
        sb.AppendLine("Local time: " + (d.LocalTime.HasValue ? d.LocalTime.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : "N/A"));
        sb.AppendLine("Temp: " + Fmt(d.TemperatureC, "0.0", " C"));
        sb.AppendLine("Humidity: " + (d.HumidityPercent.HasValue ? d.HumidityPercent.Value + " %" : "N/A"));
        sb.AppendLine("Wind: " + Fmt(d.WindSpeedMs, "0.0", " m/s"));
        sb.AppendLine("Cloud cover: " + (d.CloudCoverPercent.HasValue ? d.CloudCoverPercent.Value + " %" : "N/A"));
        output = sb.ToString();
    }

    void HandleError(WeatherError e)
    {
        status = "Error.";
        output = e.ToString();   // kind + message + HTTP status; never the URL or key
    }

    static string Fmt(float? value, string format, string unit) =>
        value.HasValue ? value.Value.ToString(format, CultureInfo.InvariantCulture) + unit : "N/A";

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
        GUILayout.BeginArea(new Rect(10, 10, Screen.width / uiScale - 20, Screen.height / uiScale - 20));
        scroll = GUILayout.BeginScrollView(scroll);

        bool test = service != null && service.IsTestMode;

        // Clear banner so test data can never pass for live data.
        if (test)
        {
            Color old = GUI.color;
            GUI.color = new Color(1f, 0.45f, 0.35f);
            GUILayout.Label("*** TEST MODE: SAMPLE DATA, NOT REAL WEATHER ***");
            GUI.color = old;
        }

        GUILayout.Label("PIPELINE DEBUG (temporary)  |  Provider: " + (service != null ? service.ProviderName : "-"));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(test ? "Live mode" : "[ Live mode ]")) UseLive();
        if (GUILayout.Button(test ? "[ Test mode ]" : "Test mode")) UseTest();
        GUILayout.EndHorizontal();

        if (!test)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kigali")) Fetch(-1.9441, 30.0619);
            if (GUILayout.Button("Tokyo")) Fetch(35.6762, 139.6503);
            if (GUILayout.Button("Reykjavik")) Fetch(64.1466, -21.9426);
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("Scenarios (the Lat/Lon below only matters for NoLocationName):");
            for (int i = 0; i < scenarios.Length; i++)
            {
                if (i % 3 == 0) GUILayout.BeginHorizontal();
                if (GUILayout.Button(scenarios[i].ToString())) PlayScenario(scenarios[i]);
                if (i % 3 == 2 || i == scenarios.Length - 1) GUILayout.EndHorizontal();
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lat", GUILayout.Width(30));
        latText = GUILayout.TextField(latText, GUILayout.Width(110));
        GUILayout.Label("Lon", GUILayout.Width(30));
        lonText = GUILayout.TextField(lonText, GUILayout.Width(110));
        if (!test && GUILayout.Button("Fetch custom")) FetchCustom();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Run parser probe (JsonUtility behaviour)")) output = RunProbe();

        GUILayout.Label("Status: " + status);
        GUILayout.Label(output);

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void UseLive()
    {
        if (service == null) return;
        service.UseLiveProvider();
    }

    void UseTest()
    {
        if (service == null) return;
        service.SetProvider(mock);
    }

    void PlayScenario(MockScenario scenario)
    {
        if (service == null) return;
        mock.Scenario = scenario;

        // Requested coordinates are only used for the label when a sample has no name.
        if (!TryReadCoordinates(out double lat, out double lon)) { lat = 0; lon = 0; }
        service.RequestWeather(lat, lon);
    }

    void Fetch(double lat, double lon)
    {
        if (service == null) return;
        service.RequestWeather(lat, lon);
    }

    void FetchCustom()
    {
        // Minimal parsing only; the real CoordinateValidator comes with the UI step.
        if (TryReadCoordinates(out double lat, out double lon)) Fetch(lat, lon);
        else status = "Lat/Lon are not valid numbers.";
    }

    bool TryReadCoordinates(out double lat, out double lon)
    {
        lon = 0;
        return double.TryParse(latText, NumberStyles.Float, CultureInfo.InvariantCulture, out lat) &&
               double.TryParse(lonText, NumberStyles.Float, CultureInfo.InvariantCulture, out lon);
    }

    // Answers: what does JsonUtility return for missing nested objects, and how does the parser react?
    static string RunProbe()
    {
        string[] samples =
        {
            "{}",
            "{\"weather\":[]}",
            "{\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"clear sky\",\"icon\":\"01d\"}]}",
            "not json at all",
        };

        var sb = new StringBuilder();
        foreach (string json in samples)
        {
            sb.AppendLine("JSON: " + json);

            try
            {
                WeatherResponse r = JsonUtility.FromJson<WeatherResponse>(json);
                sb.AppendLine("  DTO null=" + (r == null) +
                              (r == null ? "" :
                              " | weather null=" + (r.weather == null) +
                              " len=" + (r.weather == null ? -1 : r.weather.Length) +
                              " | main null=" + (r.main == null) +
                              " | wind null=" + (r.wind == null) +
                              " | clouds null=" + (r.clouds == null) +
                              " | sys null=" + (r.sys == null)));
            }
            catch (Exception ex)
            {
                sb.AppendLine("  FromJson threw " + ex.GetType().Name);
            }

            ParseResult p = WeatherParser.Parse(json, 0, 0);
            sb.AppendLine("  Parser: " + (p.Success
                ? "OK, temp=" + (p.Data.TemperatureC.HasValue ? p.Data.TemperatureC.Value.ToString(CultureInfo.InvariantCulture) : "N/A")
                : p.Error.ToString()));
        }
        return sb.ToString();
    }
}