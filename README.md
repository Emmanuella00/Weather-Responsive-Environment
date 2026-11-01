# Weather-Responsive Environment

A Unity app that fetches live weather from the OpenWeather Current Weather API and shows it on a Canvas UI and in a 3D scene that changes to match. 

## Features

- Live weather for 8 preset locations (Kigali, Tokyo, Reykjavik, Sydney, Vancouver, Dubai, Singapore, Anchorage) or for custom latitude and longitude.
- The weather card shows the place and country, description, temperature in °C, humidity, wind speed in m/s, cloud cover, local time, and day or night at that location.
- A loading spinner, clear error messages, and "Last result" handling that keeps the last good result on screen after an error.
- Coordinate validation (latitude -90 to 90, longitude -180 to 180) with clear messages for empty or invalid input. Nothing is sent until both values are valid.
- A **Test mode** with 19 sample scenarios that run through the same parser, UI and scene as live data. 

## Setup 

The API key is not in this repository. It lives in a local asset that is listed in `.gitignore`.

1. Get a free key at [openweathermap.org](https://openweathermap.org)  
2. Open the project in Unity 6000.5.10f1 and open the scene `Assets/Scenes/WeatherResponsiveScene`.
3. Create the folder `Assets/Config`, right-click it, and choose **Create > Weather > API Config**. Keep the name `ApiConfig`.
4. Paste your key into the asset's **Api Key** field.
5. Select the `WeatherService` object in the scene and drag the asset into its **Api Config** slot.
6. Press Play and pick a location.

Never commit `Assets/Config/ApiConfig.asset` or its `.meta` file. 

## How to use the app

Pick a preset from the dropdown, or type coordinates and press **Get weather**. Click **Test** to switch to sample data: a banner reads "TEST MODE: SAMPLE DATA, NOT REAL WEATHER" and a scenario dropdown becomes active. Click **Live** to return to real data.

## Architecture

`IWeatherProvider` returns raw JSON, either from `LiveWeatherProvider` (UnityWebRequest) or `MockWeatherProvider` (sample data). `WeatherService` runs the request, passes the text to `WeatherParser`, and raises events (`OnWeatherLoaded`, `OnWeatherError`, `OnLoadingChanged`, `OnProviderChanged`). The parser uses `JsonUtility` to fill the DTO classes in `WeatherDtos.cs` and builds a clean `WeatherData` model, where missing values stay null and show as "N/A". The 3D scene (`EnvironmentController`) and the UI views (`WeatherCardView`, `StatusBarView`, `UiThemeController`, `LocationSelectorUI`, `TestModePanel`) subscribe to the service's events and read only `WeatherData`. All scripts are in `Assets/Scripts`.


## Credits

Weather data from [OpenWeather](https://openweathermap.org).
