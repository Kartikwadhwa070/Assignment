using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PencilLineUI : MonoBehaviour
{
    public RectTransform canvasRect;
    public GameObject lineSegmentPrefab; // 1x1 white pixel Image
    public float minDistance = 5f; // pixels

    private Vector2 lastPoint;
    private bool isDrawing = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            lastPoint = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing)
        {
            Vector2 currentPoint = Input.mousePosition;
            if (Vector2.Distance(lastPoint, currentPoint) >= minDistance)
            {
                CreateLineSegment(lastPoint, currentPoint);
                lastPoint = currentPoint;
            }
        }
    }

    void CreateLineSegment(Vector2 start, Vector2 end)
    {
        GameObject segment = Instantiate(lineSegmentPrefab, canvasRect);
        RectTransform rt = segment.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0); // Use absolute position
        rt.sizeDelta = new Vector2(Vector2.Distance(start, end), 5f); // 5 pixels thick
        rt.position = start;

        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void ClearLine()
    {
        foreach (Transform child in canvasRect)
        {
            Destroy(child.gameObject);
        }
    }
}
