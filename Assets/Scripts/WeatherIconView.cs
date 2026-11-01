using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A small weather icon drawn from generated circles and rounded bars (no imported art).
/// Put it on an empty RectTransform of about 124 x 86; WeatherCardView calls Show().
/// </summary>
public sealed class WeatherIconView : MonoBehaviour
{
    const float BoxWidth = 124f;
    const float BoxHeight = 86f;

    static readonly Color SunColor = new Color32(0xF5, 0xC5, 0x42, 0xFF);
    static readonly Color MoonColor = new Color32(0xED, 0xEB, 0xDD, 0xFF);
    static readonly Color DropColor = new Color32(0xBF, 0xE0, 0xFF, 0xFF);
    static readonly Color FogColor = new Color(1f, 1f, 1f, 0.8f);

    bool built;
    GameObject sunBehind, sun, moon, cloud, drops, snow, fog;

    public void Show(WeatherCategory category, bool isDay)
    {
        EnsureBuilt();

        sunBehind.SetActive(category == WeatherCategory.Clouds && isDay);
        sun.SetActive(category == WeatherCategory.Clear && isDay);
        moon.SetActive(category == WeatherCategory.Clear && !isDay);
        cloud.SetActive(category != WeatherCategory.Clear);
        drops.SetActive(category == WeatherCategory.Rain);
        snow.SetActive(category == WeatherCategory.Snow);
        fog.SetActive(category == WeatherCategory.Atmosphere);
    }

    // Built on first use: Awake does not run while this object is inactive.
    void EnsureBuilt()
    {
        if (built) return;
        built = true;

        Sprite circle = UiSprites.Circle();

        // Groups are created back to front, so the cloud draws over the sun behind it.
        sunBehind = Group("SunBehind");
        Part(sunBehind, 84, 24, 32, 32, circle, SunColor);

        sun = Group("Sun");
        Part(sun, 62, 42, 54, 54, circle, SunColor);

        moon = Group("Moon");
        Part(moon, 62, 42, 46, 46, circle, MoonColor);
        Part(moon, 100, 14, 5, 5, circle, MoonColor);
        Part(moon, 24, 22, 4, 4, circle, MoonColor);

        cloud = Group("Cloud");
        Part(cloud, 40, 40, 30, 30, circle, Color.white);
        Part(cloud, 62, 31, 40, 40, circle, Color.white);
        Part(cloud, 84, 40, 30, 30, circle, Color.white);
        Part(cloud, 62, 46, 66, 22, UiSprites.RoundedRect(11), Color.white);

        drops = Group("Drops");
        Sprite bar = UiSprites.RoundedRect(2);
        Part(drops, 42, 68, 4, 14, bar, DropColor, -15f);
        Part(drops, 62, 68, 4, 14, bar, DropColor, -15f);
        Part(drops, 82, 68, 4, 14, bar, DropColor, -15f);

        snow = Group("Snow");
        Part(snow, 42, 67, 6, 6, circle, Color.white);
        Part(snow, 62, 74, 6, 6, circle, Color.white);
        Part(snow, 82, 67, 6, 6, circle, Color.white);

        fog = Group("Fog");
        Part(fog, 62, 66, 54, 4, bar, FogColor);
        Part(fog, 62, 75, 40, 4, bar, FogColor);
    }

    GameObject Group(string groupName)
    {
        var go = new GameObject(groupName, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        go.SetActive(false);
        return go;
    }

    // Position is the CENTER of the part, measured from the top-left of the 124 x 86 box.
    void Part(GameObject parent, float centerX, float centerY, float width, float height,
              Sprite sprite, Color color, float rotationZ = 0f)
    {
        var go = new GameObject("Part", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(centerX - BoxWidth * 0.5f, BoxHeight * 0.5f - centerY);
        rt.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        image.type = sprite.border == Vector4.zero ? Image.Type.Simple : Image.Type.Sliced;
    }
}
