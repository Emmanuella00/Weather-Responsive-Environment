using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TEST MODE source. Returns sample JSON (or a simulated failure) through the same
/// IWeatherProvider contract as the live provider, so parser, model, events, UI and
/// environment all run the normal workflow. Set Scenario, then request weather.
/// </summary>
public sealed class MockWeatherProvider : IWeatherProvider
{
    readonly float delaySeconds;

    public MockScenario Scenario { get; set; } = MockScenario.ClearDay;

    public string DisplayName => "TEST MODE (sample data)";
    public bool IsTestMode => true;

    /// <param name="delaySeconds">Simulated network delay so the loading indicator is visible.</param>
    public MockWeatherProvider(float delaySeconds = 0.5f)
    {
        this.delaySeconds = Mathf.Max(0f, delaySeconds);
    }

    public IEnumerator Fetch(double latitude, double longitude, Action<FetchResult> onComplete)
    {
        // Capture now: the scenario could be changed while we wait.
        MockScenario scenario = Scenario;

        if (delaySeconds > 0f)
            yield return new WaitForSecondsRealtime(delaySeconds);

        onComplete(MockSamples.Get(scenario));
    }
}
