using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LetterTracer : MonoBehaviour
{
    public PathPoint[] pathPoints; // Assign in inspector, in order
    public RectTransform canvasRect;
    public GameObject lineSegmentPrefab; // 1x1 pixel Image for line

    private int currentIndex = 0;
    private Vector2 lastPoint;
    private bool isDrawing = false;

    public float minDistance = 5f; // Minimum pixel distance for line segment

    void Start()
    {
        currentIndex = 0;

        // Activate the first point immediately
        pathPoints[currentIndex].Activate();  // Turn it green
        pathPoints[currentIndex].SetTargetHighlight(false); // Remove yellow if needed

        // Highlight the next point (index 1) as the target
        if (pathPoints.Length > 1)
            pathPoints[1].SetTargetHighlight(true);
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            // Start drawing only if touching the current point or the next target
            if (IsOverPoint(mousePos, currentIndex) ||
                (currentIndex + 1 < pathPoints.Length && IsOverPoint(mousePos, currentIndex + 1)))
            {
                isDrawing = true;
                lastPoint = mousePos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false; // Stop drawing when finger is lifted
        }

        if (isDrawing)
        {
            Vector2 currentPos = Input.mousePosition;

            // Only allow drawing towards next point
            if (currentIndex + 1 < pathPoints.Length)
            {
                Vector2 start = pathPoints[currentIndex].GetComponent<RectTransform>().position;
                Vector2 end = pathPoints[currentIndex + 1].GetComponent<RectTransform>().position;

                if (IsNearLine(start, end, currentPos, 50f))
                {
                    if (Vector2.Distance(lastPoint, currentPos) >= minDistance)
                    {
                        CreateLineSegment(lastPoint, currentPos);
                        lastPoint = currentPos;
                    }

                    // Activate next point if reached
                    if (IsOverPoint(currentPos, currentIndex + 1))
                    {
                        pathPoints[currentIndex + 1].Activate();
                        pathPoints[currentIndex + 1].SetTargetHighlight(false);
                        currentIndex++;

                        // Highlight the next point if it exists
                        if (currentIndex + 1 < pathPoints.Length)
                            pathPoints[currentIndex + 1].SetTargetHighlight(true);

                        lastPoint = pathPoints[currentIndex].GetComponent<RectTransform>().position;
                    }
                }
            }
        }
    }



    bool IsOverPoint(Vector2 pos, int pointIndex)
    {
        RectTransform rt = pathPoints[pointIndex].GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(rt, pos);
    }

    bool IsNearLine(Vector2 start, Vector2 end, Vector2 point, float tolerance)
    {
        float distance = Mathf.Abs((end.y - start.y) * point.x - (end.x - start.x) * point.y + end.x * start.y - end.y * start.x)
                         / Vector2.Distance(start, end);
        return distance <= tolerance;
    }

    void CreateLineSegment(Vector2 start, Vector2 end)
    {
        GameObject segment = Instantiate(lineSegmentPrefab, canvasRect);
        RectTransform rt = segment.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
        rt.sizeDelta = new Vector2(Vector2.Distance(start, end), 5f);
        rt.position = start;

        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}
