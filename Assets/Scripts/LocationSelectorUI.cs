using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Input side of the UI: the preset dropdown and the custom coordinate fields.
/// It validates, then asks the WeatherService for weather. It never displays weather
/// (WeatherCardView does) and never talks to the API.
/// </summary>
public class LocationSelectorUI : MonoBehaviour
{
    [SerializeField] WeatherService service;

    [Header("Controls")]
    [SerializeField] CanvasGroup locationGroup;      // wraps everything below; dimmed in test mode
    [SerializeField] TMP_Dropdown presetDropdown;
    [SerializeField] TMP_InputField latitudeInput;
    [SerializeField] TMP_InputField longitudeInput;
    [SerializeField] Button getWeatherButton;

    [Header("Validation feedback")]
    [SerializeField] TMP_Text validationText;
    [SerializeField] Image validationBackground;
    [SerializeField] Image latitudeBackground;
    [SerializeField] Image longitudeBackground;

    static readonly Color ErrorBox = new Color(0.70f, 0.15f, 0.12f, 1f);
    static readonly Color ErrorField = new Color(0.85f, 0.20f, 0.18f, 0.90f);
    static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

    Color latitudeNormal = Color.white;
    Color longitudeNormal = Color.white;
    bool loading;

    void Awake()
    {
        if (latitudeBackground != null) latitudeNormal = latitudeBackground.color;
        if (longitudeBackground != null) longitudeNormal = longitudeBackground.color;

        // Option 0 is a prompt; options 1..N are the presets.
        var options = new List<string> { "Choose a location..." };
        foreach (LocationPreset preset in LocationPresets.All) options.Add(preset.Label);
        presetDropdown.ClearOptions();
        presetDropdown.AddOptions(options);
    }

    void OnEnable()
    {
        presetDropdown.onValueChanged.AddListener(OnPresetChanged);
        getWeatherButton.onClick.AddListener(OnGetWeather);
        latitudeInput.onValueChanged.AddListener(OnTextEdited);
        longitudeInput.onValueChanged.AddListener(OnTextEdited);

        if (service == null) { Debug.LogWarning("LocationSelectorUI: assign the WeatherService."); return; }
        service.OnLoadingChanged += HandleLoading;
        service.OnProviderChanged += HandleProviderChanged;
    }

    void OnDisable()
    {
        presetDropdown.onValueChanged.RemoveListener(OnPresetChanged);
        getWeatherButton.onClick.RemoveListener(OnGetWeather);
        latitudeInput.onValueChanged.RemoveListener(OnTextEdited);
        longitudeInput.onValueChanged.RemoveListener(OnTextEdited);

        if (service == null) return;
        service.OnLoadingChanged -= HandleLoading;
        service.OnProviderChanged -= HandleProviderChanged;
    }

    void Start()
    {
        ClearValidation();
        RefreshState();
    }

    void OnPresetChanged(int index)
    {
        if (index <= 0 || service == null) return;

        ClearValidation();
        LocationPreset preset = LocationPresets.All[index - 1];
        service.RequestWeather(preset.Latitude, preset.Longitude);
    }

    void OnGetWeather()
    {
        if (service == null) return;

        CoordinateValidation result = CoordinateValidator.Validate(latitudeInput.text, longitudeInput.text);
        if (!result.IsValid)
        {
            ShowValidation(result);
            return;                     // nothing is sent until both fields are valid
        }

        ClearValidation();
        service.RequestWeather(result.Latitude, result.Longitude);
    }

    void OnTextEdited(string _) => ClearValidation();

    void HandleLoading(bool isLoading)
    {
        loading = isLoading;
        RefreshState();
    }

    void HandleProviderChanged(bool isTestMode)
    {
        presetDropdown.SetValueWithoutNotify(0);
        ClearValidation();
        RefreshState();
    }

    // Disabled while a request is running, and in test mode (location is not used there).
    void RefreshState()
    {
        bool testMode = service != null && service.IsTestMode;
        if (locationGroup == null) return;

        locationGroup.interactable = !loading && !testMode;
        locationGroup.alpha = testMode ? 0.45f : 1f;
    }

    void ShowValidation(CoordinateValidation result)
    {
        if (validationText != null) validationText.text = result.Message;
        if (validationBackground != null) validationBackground.color = ErrorBox;
        if (latitudeBackground != null)
            latitudeBackground.color = result.LatitudeError != null ? ErrorField : latitudeNormal;
        if (longitudeBackground != null)
            longitudeBackground.color = result.LongitudeError != null ? ErrorField : longitudeNormal;
    }

    void ClearValidation()
    {
        if (validationText != null) validationText.text = "";
        if (validationBackground != null) validationBackground.color = Transparent;
        if (latitudeBackground != null) latitudeBackground.color = latitudeNormal;
        if (longitudeBackground != null) longitudeBackground.color = longitudeNormal;
    }
}
