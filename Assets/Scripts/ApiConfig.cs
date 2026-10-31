using UnityEngine;

/// <summary>
/// Holds the OpenWeather API key. Create the asset locally via
/// Assets > Create > Weather > API Config, save it as Assets/Config/ApiConfig.asset,
/// and paste your key into it. The .asset file is listed in .gitignore, so the key
/// is never committed. This script (without a key) is safe to commit.
/// </summary>
[CreateAssetMenu(fileName = "ApiConfig", menuName = "Weather/API Config")]
public class ApiConfig : ScriptableObject
{
    public string apiKey;
}
