using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// TEMPORARY test harness (delete once the real UI exists).
/// Uses IMGUI so it needs no Canvas: just add it to a GameObject and assign the WeatherService.
/// Shows results on screen because browser console logs are awkward to read in a WebGL build.
/// </summary>
public class PipelineDebugHud : MonoBehaviour
{
    [SerializeField] WeatherService service;
    [SerializeField, Range(1f, 3f)] float uiScale = 1.6f;

    string latText = "-1.9441";   // Kigali
    string lonText = "30.0619";
    string status = "Idle.";
    string output = "";

    void OnEnable()
    {
        if (service == null) { status = "Assign the WeatherService in the Inspector."; return; }
        service.OnWeatherLoaded += HandleLoaded;
        service.OnWeatherError += HandleError;
        service.OnLoadingChanged += HandleLoading;
    }

    void OnDisable()
    {
        if (service == null) return;
        service.OnWeatherLoaded -= HandleLoaded;
        service.OnWeatherError -= HandleError;
        service.OnLoadingChanged -= HandleLoading;
    }

    void HandleLoading(bool loading) { if (loading) status = "Loading..."; }

    void HandleLoaded(WeatherData d)
    {
        status = "Success.";
        var sb = new StringBuilder();
        sb.AppendLine("Location:  " + d.LocationLabel);
        sb.AppendLine("Condition: " + d.ConditionId + " / " + d.ConditionGroup + " / \"" + d.Description + "\" / icon " + d.IconCode);
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

        GUILayout.Label("PIPELINE DEBUG (temporary)  |  Provider: " + (service != null ? service.ProviderName : "-"));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Kigali")) Fetch(-1.9441, 30.0619);
        if (GUILayout.Button("Tokyo")) Fetch(35.6762, 139.6503);
        if (GUILayout.Button("Reykjavik")) Fetch(64.1466, -21.9426);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lat", GUILayout.Width(30));
        latText = GUILayout.TextField(latText, GUILayout.Width(110));
        GUILayout.Label("Lon", GUILayout.Width(30));
        lonText = GUILayout.TextField(lonText, GUILayout.Width(110));
        if (GUILayout.Button("Fetch custom")) FetchCustom();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Run parser probe (JsonUtility behaviour)")) output = RunProbe();

        GUILayout.Label("Status: " + status);
        GUILayout.Label(output);

        GUILayout.EndArea();
    }

    void Fetch(double lat, double lon)
    {
        if (service == null) return;
        service.RequestWeather(lat, lon);
    }

    void FetchCustom()
    {
        // Minimal parsing only; the real CoordinateValidator comes with the UI step.
        if (double.TryParse(latText, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) &&
            double.TryParse(lonText, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
            Fetch(lat, lon);
        else
            status = "Lat/Lon are not valid numbers.";
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
            catch (System.Exception ex)
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
