using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gives an Image rounded corners (or a circle) at runtime, using a generated sprite.
/// In the editor the Image stays a plain rectangle until you press Play.
/// The Image's own Color still tints it, so the theme can recolor it freely.
/// </summary>
[RequireComponent(typeof(Image))]
public sealed class RoundedGraphic : MonoBehaviour
{
    [SerializeField, Range(2, 31)] int cornerRadius = 20;
    [SerializeField] bool circle;

    void Awake()
    {
        Image image = GetComponent<Image>();

        if (circle)
        {
            image.sprite = UiSprites.Circle();
            image.type = Image.Type.Simple;
        }
        else
        {
            image.sprite = UiSprites.RoundedRect(cornerRadius);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }
    }
}
