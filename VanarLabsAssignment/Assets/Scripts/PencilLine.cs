using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PencilLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    public float minDistance = 0.05f;
    public PolygonCollider2D letterCollider; // assign in Inspector

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        bool isDrawing = Input.GetMouseButton(0) || Input.touchCount > 0;

        if (isDrawing)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // ✅ Only allow drawing if inside collider
            if (letterCollider != null && letterCollider.OverlapPoint(worldPos))
            {
                if (points.Count == 0 || Vector3.Distance(points[^1], worldPos) > minDistance)
                {
                    points.Add(worldPos);
                    lineRenderer.positionCount = points.Count;
                    lineRenderer.SetPositions(points.ToArray());
                }
            }
        }
    }

    public void ClearLine()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
    }
}
