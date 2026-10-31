
public static class WeatherCategoryMapper
{
    public static WeatherCategory MapCategory(int conditionId)
    {
        if (conditionId >= 200 && conditionId < 400) return WeatherCategory.Rain;        // thunderstorm + less rain
        if (conditionId >= 500 && conditionId < 600) return WeatherCategory.Rain;
        if (conditionId >= 600 && conditionId < 700) return WeatherCategory.Snow;
        if (conditionId >= 700 && conditionId < 800) return WeatherCategory.Atmosphere;
        if (conditionId == 800) return WeatherCategory.Clear;
        if (conditionId >= 801 && conditionId <= 804) return WeatherCategory.Clouds;
        return WeatherCategory.Fallback;
    }

    
    public static bool TryResolveDayNight(string icon, long observation, long sunrise, long sunset, out bool isDay)
    {
        if (!string.IsNullOrEmpty(icon))
        {
            char suffix = icon[icon.Length - 1];
            if (suffix == 'd') { isDay = true; return true; }
            if (suffix == 'n') { isDay = false; return true; }
        }

        if (observation > 0 && sunrise > 0 && sunset > sunrise)
        {
            isDay = observation >= sunrise && observation < sunset;
            return true;
        }

        isDay = false;
        return false;
    }
}
