using UnityEngine;

public class TraceZone : MonoBehaviour
{
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public bool Contains(Vector2 screenPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null);
    }
}
