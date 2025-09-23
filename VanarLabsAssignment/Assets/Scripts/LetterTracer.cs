using UnityEngine;
using UnityEngine.UI;

public class LetterTracer : MonoBehaviour
{
    [Header("Setup")]
    public PathPoint[] pathPoints;
    public RectTransform canvasRect;
    public GameObject lineSegmentPrefab;

    [Header("Settings")]
    public float minDistance = 5f;
    public float tolerance = 50f;
    public float lineThickness = 6f;

    private int currentIndex = 0;
    private Vector2 lastPoint;
    private bool isDrawing = false;
    private bool hasTracedCurrentStroke = false;

    // ✅ Events for UI scripts
    public delegate void ProgressChanged(int current, int total);
    public event ProgressChanged OnProgress;

    public delegate void LetterCompleted();
    public event LetterCompleted OnComplete;

    void Start()
    {
        ResetProgress();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;

            if (IsOverPoint(pos, currentIndex) ||
                (currentIndex + 1 < pathPoints.Length && IsOverPoint(pos, currentIndex + 1)))
            {
                isDrawing = true;
                lastPoint = pos;
                hasTracedCurrentStroke = false;
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

            if (IsNearLine(start, end, pos, tolerance))
            {
                if (Vector2.Distance(lastPoint, pos) >= minDistance)
                {
                    CreateLineSegment(lastPoint, pos);
                    lastPoint = pos;
                }

                hasTracedCurrentStroke = true;

                if (IsOverPoint(pos, currentIndex + 1) && hasTracedCurrentStroke)
                {
                    pathPoints[currentIndex + 1].Activate();
                    pathPoints[currentIndex + 1].Highlight(false);
                    currentIndex++;

                    // fire progress event
                    OnProgress?.Invoke(currentIndex, pathPoints.Length - 1);

                    if (currentIndex == pathPoints.Length - 1)
                    {
                        OnComplete?.Invoke();
                    }
                    else
                    {
                        pathPoints[currentIndex + 1].Highlight(true);
                    }

                    lastPoint = pathPoints[currentIndex].GetComponent<RectTransform>().position;
                    hasTracedCurrentStroke = false;
                }
            }
            else
            {
                // 🚨 Out of bounds → reset
                Debug.Log("Out of bounds! Restarting...");
                ResetProgress();
            }
        }
    }

    void ResetProgress()
    {
        // Clear lines
        ClearLines();

        // Reset points
        foreach (var p in pathPoints)
            p.ResetPoint();

        currentIndex = 0;
        pathPoints[0].Activate();

        if (pathPoints.Length > 1)
            pathPoints[1].Highlight(true);

        isDrawing = false;
        hasTracedCurrentStroke = false;

        // reset progress event
        OnProgress?.Invoke(currentIndex, pathPoints.Length - 1);
    }

    void ClearLines()
    {
        for (int i = canvasRect.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasRect.GetChild(i);
            if (child.CompareTag("Line"))
            {
                Destroy(child.gameObject);
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
        seg.tag = "Line";

        RectTransform rt = seg.GetComponent<RectTransform>();
        int pointIndex = pathPoints[0].transform.GetSiblingIndex();
        seg.transform.SetSiblingIndex(pointIndex);

        rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(Vector2.Distance(start, end), lineThickness);
        rt.position = start;

        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}
