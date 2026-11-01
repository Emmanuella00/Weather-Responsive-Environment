using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Live / Test switch and the scenario list. Test mode swaps in the MockWeatherProvider, so
/// sample data runs through the same parser, events, card and scene as live data.
/// Also shows the TEST MODE banner whenever test mode is on.
/// </summary>
public class TestModePanel : MonoBehaviour
{
    [SerializeField] WeatherService service;

    [Header("Live / Test switch")]
    [SerializeField] Button liveButton;
    [SerializeField] Button testButton;
    [SerializeField] Image liveBackground;
    [SerializeField] Image testBackground;

    [Header("Scenarios")]
    [SerializeField] TMP_Dropdown scenarioDropdown;
    [SerializeField] CanvasGroup scenarioGroup;      // the dropdown (or its parent), dimmed in live mode

    [Header("Banner")]
    [SerializeField] GameObject testBanner;

    static readonly Color Selected = new Color(1f, 1f, 1f, 0.32f);
    static readonly Color Unselected = new Color(1f, 1f, 1f, 0.10f);

    readonly MockWeatherProvider mock = new MockWeatherProvider();
    readonly MockScenario[] scenarios = (MockScenario[])Enum.GetValues(typeof(MockScenario));
    bool loading;

    void Awake()
    {
        // Option 0 is a prompt; options 1..N are the scenarios.
        var options = new List<string> { "Choose a scenario..." };
        foreach (MockScenario scenario in scenarios) options.Add(scenario.ToString());
        scenarioDropdown.ClearOptions();
        scenarioDropdown.AddOptions(options);
    }

    void OnEnable()
    {
        liveButton.onClick.AddListener(UseLive);
        testButton.onClick.AddListener(UseTest);
        scenarioDropdown.onValueChanged.AddListener(OnScenarioChanged);

        if (service == null) { Debug.LogWarning("TestModePanel: assign the WeatherService."); return; }
        service.OnProviderChanged += HandleProviderChanged;
        service.OnLoadingChanged += HandleLoading;
    }

    void OnDisable()
    {
        liveButton.onClick.RemoveListener(UseLive);
        testButton.onClick.RemoveListener(UseTest);
        scenarioDropdown.onValueChanged.RemoveListener(OnScenarioChanged);

        if (service == null) return;
        service.OnProviderChanged -= HandleProviderChanged;
        service.OnLoadingChanged -= HandleLoading;
    }

    void Start() => Refresh();

    void UseLive()
    {
        if (service != null && service.IsTestMode) service.UseLiveProvider();
    }

    void UseTest()
    {
        if (service != null && !service.IsTestMode) service.SetProvider(mock);
    }

    void OnScenarioChanged(int index)
    {
        if (index <= 0 || service == null || !service.IsTestMode) return;

        mock.Scenario = scenarios[index - 1];
        // Coordinates only matter for the "no location name" sample, which shows them as its label.
        service.RequestWeather(service.LastLatitude, service.LastLongitude);
    }

    void HandleProviderChanged(bool isTestMode)
    {
        scenarioDropdown.SetValueWithoutNotify(0);
        Refresh();
    }

    void HandleLoading(bool isLoading)
    {
        loading = isLoading;
        Refresh();
    }

    void Refresh()
    {
        bool test = service != null && service.IsTestMode;

        if (testBanner != null) testBanner.SetActive(test);
        if (liveBackground != null) liveBackground.color = test ? Unselected : Selected;
        if (testBackground != null) testBackground.color = test ? Selected : Unselected;

        if (scenarioGroup != null)
        {
            scenarioGroup.alpha = test ? 1f : 0.45f;
            scenarioGroup.interactable = test && !loading;
        }
    }
}
