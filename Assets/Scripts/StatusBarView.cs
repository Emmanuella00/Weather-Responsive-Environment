using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The status strip at the bottom: a spinner and "Loading weather..." while a request runs,
/// then a success note or a clear error message. Errors never contain the URL or API key
/// (WeatherError messages are already safe to show).
/// </summary>
public class StatusBarView : MonoBehaviour
{
    [SerializeField] WeatherService service;
    [SerializeField] TMP_Text statusText;
    [SerializeField] Image spinner;
    [SerializeField] float spinDegreesPerSecond = 360f;

    static readonly Color NormalColor = Color.white;
    static readonly Color ErrorColor = new Color32(0xFF, 0xB4, 0xAB, 0xFF);

    bool hasResult;

    void Awake()
    {
        spinner.sprite = UiSprites.Spinner();
        spinner.raycastTarget = false;
        spinner.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (service == null) { Debug.LogWarning("StatusBarView: assign the WeatherService."); return; }
        service.OnLoadingChanged += HandleLoading;
        service.OnWeatherLoaded += HandleLoaded;
        service.OnWeatherError += HandleError;
        service.OnProviderChanged += HandleProviderChanged;
    }

    void OnDisable()
    {
        if (service == null) return;
        service.OnLoadingChanged -= HandleLoading;
        service.OnWeatherLoaded -= HandleLoaded;
        service.OnWeatherError -= HandleError;
        service.OnProviderChanged -= HandleProviderChanged;
    }

    void Start() => HandleProviderChanged(service != null && service.IsTestMode);

    void Update()
    {
        if (spinner.gameObject.activeSelf)
            spinner.rectTransform.Rotate(0f, 0f, -spinDegreesPerSecond * Time.unscaledDeltaTime);
    }

    void HandleLoading(bool isLoading)
    {
        spinner.gameObject.SetActive(isLoading);
        if (!isLoading) return;

        statusText.color = NormalColor;
        statusText.text = "Loading weather...";
    }

    void HandleLoaded(WeatherData data)
    {
        hasResult = true;
        statusText.color = NormalColor;
        statusText.text = data.IsTestData ? "Test sample loaded." : "Weather updated.";
    }

    void HandleError(WeatherError error)
    {
        statusText.color = ErrorColor;
        statusText.text = error.Message + (hasResult ? " Showing the last result." : "");
    }

    void HandleProviderChanged(bool isTestMode)
    {
        hasResult = false;
        statusText.color = NormalColor;
        statusText.text = isTestMode
            ? "Pick a scenario to play."
            : "Choose a location or enter coordinates.";
    }
}
