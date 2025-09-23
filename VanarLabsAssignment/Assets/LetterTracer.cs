using UnityEngine;
using UnityEngine.UI;

public class LetterTracer : MonoBehaviour
{
    [Header("Setup")]
    public PathPoint[] pathPoints;         // Assign in inspector (in order)
    public RectTransform canvasRect;       // The Canvas RectTransform
    public GameObject lineSegmentPrefab;   // Prefab for line drawing

    [Header("Settings")]
    public float minDistance = 5f;         // Minimum distance before placing segment
    public float tolerance = 50f;          // Allowed distance from the path line

    private int currentIndex = 0;
    private Vector2 lastPoint;
    private bool isDrawing = false;

    void Start()
    {
        // Activate first point
        currentIndex = 0;
        pathPoints[0].Activate();

        if (pathPoints.Length > 1)
            pathPoints[1].Highlight(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;

            // Must start at the current or next point
            if (IsOverPoint(pos, currentIndex) ||
                (currentIndex + 1 < pathPoints.Length && IsOverPoint(pos, currentIndex + 1)))
            {
                isDrawing = true;
                lastPoint = pos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing && currentIndex + 1 < pathPoints.Length)
        {
            Vector2 pos = Input.mousePosition;

            Vector2 start = pathPoints[currentIndex].GetComponent<RectTransform>().position;
            Vector2 end = pathPoints[currentIndex + 1].GetComponent<RectTransform>().position;

            // Only draw if near the line between current and next point
            if (IsNearLine(start, end, pos, tolerance))
            {
                if (Vector2.Distance(lastPoint, pos) >= minDistance)
                {
                    CreateLineSegment(lastPoint, pos);
                    lastPoint = pos;
                }

                // Reached the next point
                if (IsOverPoint(pos, currentIndex + 1))
                {
                    pathPoints[currentIndex + 1].Activate();
                    pathPoints[currentIndex + 1].Highlight(false);
                    currentIndex++;

                    if (currentIndex + 1 < pathPoints.Length)
                        pathPoints[currentIndex + 1].Highlight(true);

                    // Snap drawing to new point
                    lastPoint = pathPoints[currentIndex].GetComponent<RectTransform>().position;
                }
            }
        }
    }

    bool IsOverPoint(Vector2 pos, int index)
    {
        if (index < 0 || index >= pathPoints.Length) return false;
        RectTransform rt = pathPoints[index].GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(rt, pos, null);
    }

    bool IsNearLine(Vector2 start, Vector2 end, Vector2 p, float tolerance)
    {
        float segLen = Vector2.Distance(start, end);
        if (segLen < 0.001f) return false;

        float distance = Mathf.Abs((end.y - start.y) * p.x - (end.x - start.x) * p.y + end.x * start.y - end.y * start.x) / segLen;
        return distance <= tolerance;
    }

    void CreateLineSegment(Vector2 start, Vector2 end)
    {
        GameObject seg = Instantiate(lineSegmentPrefab, canvasRect);
        RectTransform rt = seg.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(Vector2.Distance(start, end), 6f);
        rt.position = start;

        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}
