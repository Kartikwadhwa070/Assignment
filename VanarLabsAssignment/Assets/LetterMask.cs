using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class LetterMask : MonoBehaviour
{
    private RawImage rawImage;
    private Texture2D tex;
    private Color32[] pixels;
    private Rect texRect;
    private int texWidth, texHeight;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();

        // IMPORTANT: RawImage must use a Texture2D, not RenderTexture
        tex = rawImage.texture as Texture2D;

        if (tex == null)
        {
            Debug.LogError("RawImage does not have a Texture2D assigned!");
            return;
        }

        pixels = tex.GetPixels32();
        texWidth = tex.width;
        texHeight = tex.height;
        texRect = new Rect(0, 0, texWidth, texHeight);
    }

    public bool IsInsideLetter(Vector2 screenPos)
    {
        RectTransform rt = rawImage.rectTransform;

        // Convert screen → local point in RawImage rect
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, null, out Vector2 local))
            return false;

        // Normalize into 0–1 inside the rect
        float nx = (local.x - rt.rect.x) / rt.rect.width;
        float ny = (local.y - rt.rect.y) / rt.rect.height;

        if (nx < 0f || nx > 1f || ny < 0f || ny > 1f)
            return false;

        // Convert normalized → pixel coords
        int px = Mathf.Clamp(Mathf.RoundToInt(nx * texRect.width), 0, texWidth - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(ny * texRect.height), 0, texHeight - 1);

        Color32 c = pixels[py * texWidth + px];

        // Only accept if alpha > 0 (visible) and color matches letter (yellow in your case)
        return c.a > 10; // just alpha check; OR use c.r/g/b if you want strictly yellow
    }
}
