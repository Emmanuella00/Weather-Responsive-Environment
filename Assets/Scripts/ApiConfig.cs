using UnityEngine;

/// <summary>
/// Holds the key. 
/// </summary>
[CreateAssetMenu(fileName = "ApiConfig", menuName = "Weather/API Config")]
public class ApiConfig : ScriptableObject
{
    public string apiKey;
}
