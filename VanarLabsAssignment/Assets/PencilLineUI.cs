using UnityEngine;
using UnityEngine.UI;

public class PencilLineUI : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform canvasRect;       // The canvas (assign in inspector)
    public GameObject lineSegmentPrefab;   // A 1x1 Image prefab used for drawing
    public LetterMask letterMask;          // The RawImage with LetterMask.cs attached

    [Header("Settings")]
    public float minDistance = 5f;         // Minimum distance before adding a new segment
    public float lineThickness = 6f;       // Thickness of drawn line

    private Vector2 lastPoint;
    private bool isDrawing = false;

    void Update()
    {
        // Mouse Down / Touch Begin
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;

            if (letterMask != null && letterMask.IsInsideLetter(pos))
            {
                isDrawing = true;
                lastPoint = pos;
            }
        }
        // Mouse Up / Touch End
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        // Drawing movement
        if (isDrawing && Input.GetMouseButton(0))
        {
            Vector2 currentPos = Input.mousePosition;

            // Only draw if inside the letter mask
            if (letterMask != null && letterMask.IsInsideLetter(currentPos))
            {
                if (Vector2.Distance(lastPoint, currentPos) >= minDistance)
                {
                    CreateLineSegment(lastPoint, currentPos);
                    lastPoint = currentPos;
                }
            }
        }
    }

    void CreateLineSegment(Vector2 start, Vector2 end)
    {
        GameObject seg = Instantiate(lineSegmentPrefab, canvasRect);
        RectTransform rt = seg.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(Vector2.Distance(start, end), lineThickness);
        rt.position = start;

        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Optional: clear all lines
    public void ClearLine()
    {
        foreach (Transform child in canvasRect)
        {
            if (child.CompareTag("LineSegment")) // tag your prefab as "LineSegment"
                Destroy(child.gameObject);
        }
    }
}
