using UnityEngine;
using UnityEngine.EventSystems;

public class LetterTracer : MonoBehaviour
{
    public PathPoint[] pathPoints;   // Assign all points in the Inspector
    private int currentIndex = 0;

    void Update()
    {
        if (Input.GetMouseButton(0)) // For touch use Input.touchCount > 0
        {
            Vector2 pos = Input.mousePosition;
            DetectPoint(pos);
        }
    }

    void DetectPoint(Vector2 screenPos)
    {
        if (currentIndex >= pathPoints.Length) return;

        RectTransform pointRect = pathPoints[currentIndex].GetComponent<RectTransform>();

        if (RectTransformUtility.RectangleContainsScreenPoint(pointRect, screenPos))
        {
            pathPoints[currentIndex].Activate();
            currentIndex++;
        }
    }
}

