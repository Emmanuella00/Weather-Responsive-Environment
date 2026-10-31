using System.Collections.Generic;

/// <summary>
/// Every scenario the test mode can play. Each one is either a raw JSON string that goes through
/// the normal parser, or a simulated provider-level failure (no JSON at all).
/// </summary>
public enum MockScenario
{
    // The four required weather states, each in day and night.
    ClearDay, ClearNight,
    CloudsDay, CloudsNight,
    RainDay, RainNight,
    SnowDay, SnowNight,

    // Extra conditions and edge cases.
    MistDay,                 // 7xx group -> Atmosphere
    UnsupportedCode,         // unknown condition id -> Fallback
    NoIconDayFromSun,        // no icon: day/night must come from sunrise/sunset
    NoIconNightFromSun,
    MissingMeasurements,     // no temperature, humidity, wind or clouds: must show N/A, not 0
    NoLocationName,          // no name or country: must show the requested coordinates
    EmptyWeatherArray,       // valid JSON but no condition -> InvalidData
    UnparseableText,         // not JSON -> ParseError

    // API failure / unavailable data (provider-level, no JSON).
    HttpError500,
    NetworkFailure,
    Timeout
}

/// <summary>
/// Sample OpenWeather responses for TEST MODE. All timestamps are fixed:
///   day samples   dt = 1761900000 (08:40 UTC), sunrise 1761885600, sunset 1761928800
///   night samples dt = 1761940000 (19:46 UTC), same sunrise/sunset
/// Single quotes stand in for double quotes (swapped in J) so the JSON stays readable here.
/// Samples with a name use fake names and the fake country code "TS" so nobody mistakes them for real data.
/// </summary>
public static class MockSamples
{
    static string J(string singleQuoted) => singleQuoted.Replace('\'', '"');

    static readonly Dictionary<MockScenario, string> Json = new Dictionary<MockScenario, string>
    {
        { MockScenario.ClearDay, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':800,'main':'Clear','description':'clear sky','icon':'01d'}],'base':'stations','main':{'temp':27.4,'feels_like':25.9,'temp_min':26.4,'temp_max':28.4,'pressure':1012,'humidity':45},'visibility':10000,'wind':{'speed':2.6,'deg':200},'clouds':{'all':0},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Clear, Day)','cod':200}") },
        { MockScenario.ClearNight, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':800,'main':'Clear','description':'clear sky','icon':'01n'}],'base':'stations','main':{'temp':14.2,'feels_like':12.7,'temp_min':13.2,'temp_max':15.2,'pressure':1012,'humidity':60},'visibility':10000,'wind':{'speed':1.8,'deg':200},'clouds':{'all':0},'dt':1761940000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Clear, Night)','cod':200}") },
        { MockScenario.CloudsDay, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':803,'main':'Clouds','description':'broken clouds','icon':'04d'}],'base':'stations','main':{'temp':21.0,'feels_like':19.5,'temp_min':20.0,'temp_max':22.0,'pressure':1012,'humidity':62},'visibility':10000,'wind':{'speed':4.1,'deg':200},'clouds':{'all':75},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Clouds, Day)','cod':200}") },
        { MockScenario.CloudsNight, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':804,'main':'Clouds','description':'overcast clouds','icon':'04n'}],'base':'stations','main':{'temp':12.5,'feels_like':11.0,'temp_min':11.5,'temp_max':13.5,'pressure':1012,'humidity':78},'visibility':10000,'wind':{'speed':3.0,'deg':200},'clouds':{'all':100},'dt':1761940000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Clouds, Night)','cod':200}") },
        { MockScenario.RainDay, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':500,'main':'Rain','description':'light rain','icon':'10d'}],'base':'stations','main':{'temp':16.8,'feels_like':15.3,'temp_min':15.8,'temp_max':17.8,'pressure':1012,'humidity':83},'visibility':10000,'wind':{'speed':5.2,'deg':200},'rain':{'1h':0.8},'clouds':{'all':90},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Rain, Day)','cod':200}") },
        { MockScenario.RainNight, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':501,'main':'Rain','description':'moderate rain','icon':'10n'}],'base':'stations','main':{'temp':13.9,'feels_like':12.4,'temp_min':12.9,'temp_max':14.9,'pressure':1012,'humidity':90},'visibility':10000,'wind':{'speed':6.4,'deg':200},'rain':{'1h':2.9},'clouds':{'all':100},'dt':1761940000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Rain, Night)','cod':200}") },
        { MockScenario.SnowDay, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':601,'main':'Snow','description':'snow','icon':'13d'}],'base':'stations','main':{'temp':-2.1,'feels_like':-3.6,'temp_min':-3.1,'temp_max':-1.1,'pressure':1012,'humidity':85},'visibility':10000,'wind':{'speed':2.4,'deg':200},'snow':{'1h':1.1},'clouds':{'all':100},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Snow, Day)','cod':200}") },
        { MockScenario.SnowNight, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':601,'main':'Snow','description':'snow','icon':'13n'}],'base':'stations','main':{'temp':-4.2,'feels_like':-5.7,'temp_min':-5.2,'temp_max':-3.2,'pressure':1012,'humidity':88},'visibility':10000,'wind':{'speed':3.1,'deg':200},'snow':{'1h':1.2},'clouds':{'all':100},'dt':1761940000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Snow, Night)','cod':200}") },
        { MockScenario.MistDay, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':741,'main':'Fog','description':'fog','icon':'50d'}],'base':'stations','main':{'temp':9.3,'feels_like':7.800000000000001,'temp_min':8.3,'temp_max':10.3,'pressure':1012,'humidity':96},'visibility':10000,'wind':{'speed':0.9,'deg':200},'clouds':{'all':100},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Mist, Day)','cod':200}") },
        { MockScenario.UnsupportedCode, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':999,'main':'Unknown','description':'unknown condition','icon':'01d'}],'base':'stations','main':{'temp':18.0,'feels_like':16.5,'temp_min':17.0,'temp_max':19.0,'pressure':1012,'humidity':50},'visibility':10000,'wind':{'speed':2.0,'deg':200},'clouds':{'all':40},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (Unsupported code)','cod':200}") },
        { MockScenario.NoIconDayFromSun, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':800,'main':'Clear','description':'clear sky'}],'base':'stations','main':{'temp':25.0,'feels_like':23.5,'temp_min':24.0,'temp_max':26.0,'pressure':1012,'humidity':40},'visibility':10000,'wind':{'speed':2.2,'deg':200},'clouds':{'all':5},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (No icon, Day)','cod':200}") },
        { MockScenario.NoIconNightFromSun, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':600,'main':'Snow','description':'light snow'}],'base':'stations','main':{'temp':-1.0,'feels_like':-2.5,'temp_min':-2.0,'temp_max':0.0,'pressure':1012,'humidity':80},'visibility':10000,'wind':{'speed':1.5,'deg':200},'clouds':{'all':90},'dt':1761940000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800,'country':'TS'},'timezone':0,'id':1,'name':'Sample City (No icon, Night)','cod':200}") },
        { MockScenario.MissingMeasurements, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':800,'main':'Clear','description':'clear sky','icon':'01d'}],'dt':1761900000,'sys':{'country':'TS','sunrise':1761885600,'sunset':1761928800},'timezone':0,'name':'Sample City (Missing data)'}") },
        { MockScenario.NoLocationName, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[{'id':802,'main':'Clouds','description':'scattered clouds','icon':'03d'}],'base':'stations','main':{'temp':24.0,'feels_like':22.5,'temp_min':23.0,'temp_max':25.0,'pressure':1012,'humidity':70},'visibility':10000,'wind':{'speed':4.5,'deg':200},'clouds':{'all':40},'dt':1761900000,'sys':{'type':1,'id':1,'sunrise':1761885600,'sunset':1761928800},'timezone':0,'id':1,'name':'','cod':200}") },
        { MockScenario.EmptyWeatherArray, J("{'coord':{'lon':20.0,'lat':10.0},'weather':[],'dt':1761900000,'name':'Sample City (No condition)','cod':200}") },
        { MockScenario.UnparseableText, "<html><body>502 Bad Gateway</body></html>" },
    };

    /// <summary>Raw JSON for a scenario, or a simulated failure. Same FetchResult type as the live provider.</summary>
    public static FetchResult Get(MockScenario scenario)
    {
        // Failure messages match the live provider so the UI shows identical text.
        switch (scenario)
        {
            case MockScenario.HttpError500:
                return FetchResult.Fail(WeatherErrorKind.HttpError,
                    "The weather service is having problems. Try again later.", 500);
            case MockScenario.NetworkFailure:
                return FetchResult.Fail(WeatherErrorKind.Network,
                    "Could not reach the weather service. Check your internet connection.");
            case MockScenario.Timeout:
                return FetchResult.Fail(WeatherErrorKind.Timeout,
                    "The request timed out. Check your connection and try again.");
        }

        return Json.TryGetValue(scenario, out string json)
            ? FetchResult.Ok(json)
            : FetchResult.Fail(WeatherErrorKind.InvalidData, "No sample data exists for this scenario.");
    }
}
