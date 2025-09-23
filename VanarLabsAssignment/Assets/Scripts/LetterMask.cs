using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class LetterMask : MonoBehaviour
{
    private RawImage rawImage;
    private Texture2D tex;
    private Color32[] pixels;
    private int texWidth, texHeight;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        tex = rawImage.texture as Texture2D;

        if (tex == null)
        {
            Debug.LogError("LetterMask: RawImage must use a Texture2D!");
            return;
        }

        if (!tex.isReadable)
        {
            Debug.LogError("LetterMask: Texture is not readable! Enable Read/Write in Import Settings.");
            return;
        }

        pixels = tex.GetPixels32();
        texWidth = tex.width;
        texHeight = tex.height;
    }

    public bool IsInsideLetter(Vector2 screenPos)
    {
        RectTransform rt = rawImage.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, null, out Vector2 local))
            return false;

        // Convert to normalized coords (0–1)
        float nx = (local.x - rt.rect.xMin) / rt.rect.width;
        float ny = (local.y - rt.rect.yMin) / rt.rect.height;

        if (nx < 0f || nx > 1f || ny < 0f || ny > 1f)
            return false;

        int px = Mathf.Clamp(Mathf.RoundToInt(nx * texWidth), 0, texWidth - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(ny * texHeight), 0, texHeight - 1);

        Color32 c = pixels[py * texWidth + px];
        return c.a > 10; // only allow if pixel is opaque
    }
}
