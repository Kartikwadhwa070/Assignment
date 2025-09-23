using UnityEngine;
using UnityEngine.UI;

public class PencilLineUI : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform canvasRect;       // Parent canvas for line segments
    public GameObject lineSegmentPrefab;   // Prefab: 1x1 Image with "LineSegment" tag
    public LetterMask letterMask;          // Reference to LetterMask

    [Header("Settings")]
    public float minDistance = 5f;         // Distance threshold before new segment
    public float lineThickness = 6f;       // Visual thickness of the drawn line

    private Vector2 lastPoint;
    private bool isDrawing = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            if (letterMask != null && letterMask.IsInsideLetter(pos))
            {
                isDrawing = true;
                lastPoint = pos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing && Input.GetMouseButton(0))
        {
            Vector2 currentPos = Input.mousePosition;

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

    public void ClearLine()
    {
        foreach (Transform child in canvasRect)
        {
            if (child.CompareTag("LineSegment"))
                Destroy(child.gameObject);
        }
    }
}
