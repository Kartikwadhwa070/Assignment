using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [Header("Path Point Settings")]
    public int pointIndex;
    public bool isStartPoint;
    public bool isEndPoint;
    public float activationRadius = 1.0f;

    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D pointCollider;

    [Header("Visual Feedback")]
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;
    public Color completedColor = Color.blue;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pointCollider = GetComponent<CircleCollider2D>();
        spriteRenderer.color = inactiveColor;
        pointCollider.radius = activationRadius;
        if (isStartPoint)
        {
            spriteRenderer.color = activeColor;
            transform.localScale *= 1.2f;
        }
    }

    public void ActivatePoint()
    {
        if (!isActivated)
        {
            isActivated = true;
            spriteRenderer.color = completedColor;
            StartCoroutine(ScaleAnimation());
        }
    }

    private System.Collections.IEnumerator ScaleAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            progress = 1f - (1f - progress) * (1f - progress);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            progress = progress * progress;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void ResetPoint()
    {
        isActivated = false;

        if (isStartPoint)
        {
            spriteRenderer.color = activeColor;
        }
        else
        {
            spriteRenderer.color = inactiveColor;
        }
    }

    public bool IsWithinRadius(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= activationRadius;
    }

    public bool IsActivated => isActivated;
}
