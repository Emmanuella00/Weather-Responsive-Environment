using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display side of the UI: shows the current WeatherData on the card.
/// Missing values show N/A, never 0. After an error the card keeps the last good result,
/// dimmed and tagged "Last result", so it still matches the 3D scene.
/// It only listens to events; it never requests weather itself.
/// </summary>
public class WeatherCardView : MonoBehaviour
{
    [SerializeField] WeatherService service;

    [Header("Layout")]
    [SerializeField] CanvasGroup content;          // everything except the placeholder
    [SerializeField] GameObject placeholder;       // "No weather loaded yet."
    [SerializeField] WeatherIconView icon;

    [Header("Texts")]
    [SerializeField] TMP_Text timeOfDayText;
    [SerializeField] TMP_Text temperatureText;
    [SerializeField] TMP_Text cityText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text localTimeText;
    [SerializeField] TMP_Text humidityText;
    [SerializeField] TMP_Text windText;
    [SerializeField] TMP_Text cloudsText;

    [Header("Source tag (top right)")]
    [SerializeField] Image sourceTagBackground;
    [SerializeField] TMP_Text sourceTagText;

    const float DimmedAlpha = 0.55f;

    static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    static readonly Color DarkText = new Color32(0x3B, 0x23, 0x00, 0xFF);

    enum Tag { Live, TestSample, LastResult }

    bool hasData;

    void OnEnable()
    {
        if (service == null) { Debug.LogWarning("WeatherCardView: assign the WeatherService."); return; }
        service.OnWeatherLoaded += Show;
        service.OnWeatherError += HandleError;
        service.OnProviderChanged += HandleProviderChanged;
    }

    void OnDisable()
    {
        if (service == null) return;
        service.OnWeatherLoaded -= Show;
        service.OnWeatherError -= HandleError;
        service.OnProviderChanged -= HandleProviderChanged;
    }

    void Start() => Clear();

    void Show(WeatherData data)
    {
        hasData = true;
        placeholder.SetActive(false);
        content.alpha = 1f;

        cityText.text = data.LocationLabel;
        descriptionText.text = Capitalize(data.Description, "No description");

        temperatureText.text = data.TemperatureC.HasValue
            ? data.TemperatureC.Value.ToString("0.0", Invariant) + "<size=45%> °C</size>"
            : "N/A";

        timeOfDayText.text = !data.IsDayKnown ? "Time unknown" : (data.IsDay ? "Day" : "Night");
        localTimeText.text = data.LocalTime.HasValue
            ? "Local time " + data.LocalTime.Value.ToString("HH:mm", Invariant)
            : "Local time N/A";

        humidityText.text = data.HumidityPercent.HasValue ? data.HumidityPercent.Value + " %" : "N/A";
        windText.text = data.WindSpeedMs.HasValue
            ? data.WindSpeedMs.Value.ToString("0.0", Invariant) + " m/s"
            : "N/A";
        cloudsText.text = data.CloudCoverPercent.HasValue ? data.CloudCoverPercent.Value + " %" : "N/A";

        // Same decision the environment makes: unknown day/night shows a neutral day.
        if (icon != null) icon.Show(data.Category, !data.IsDayKnown || data.IsDay);

        SetTag(data.IsTestData ? Tag.TestSample : Tag.Live);
    }

    void HandleError(WeatherError error)
    {
        if (!hasData) return;           // nothing to dim; the status bar shows the error
        content.alpha = DimmedAlpha;
        SetTag(Tag.LastResult);
    }

    void HandleProviderChanged(bool isTestMode) => Clear();

    void Clear()
    {
        hasData = false;
        content.alpha = 0f;
        placeholder.SetActive(true);
    }

    void SetTag(Tag tag)
    {
        switch (tag)
        {
            case Tag.TestSample:
                sourceTagText.text = "TEST SAMPLE";
                sourceTagText.color = DarkText;
                sourceTagBackground.color = new Color32(0xFF, 0xB0, 0x20, 0xFF);
                break;
            case Tag.LastResult:
                sourceTagText.text = "Last result";
                sourceTagText.color = Color.white;
                sourceTagBackground.color = new Color(0f, 0f, 0f, 0.30f);
                break;
            default:
                sourceTagText.text = "Live";
                sourceTagText.color = Color.white;
                sourceTagBackground.color = new Color(1f, 1f, 1f, 0.22f);
                break;
        }
    }

    static string Capitalize(string text, string fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;
        text = text.Trim();
        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}
