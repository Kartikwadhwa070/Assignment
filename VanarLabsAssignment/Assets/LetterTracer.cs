using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterTracer : MonoBehaviour
{
    [Header("Tracing Settings")]
    public List<PathPoint> pathPoints = new List<PathPoint>();
    public LineRenderer tracingLine;
    public float pathTolerance = 0.5f;
    public float minimumTraceDistance = 0.1f;

    [Header("Input Settings")]
    public LayerMask tracingLayer = -1;
    public Camera tracingCamera;

    [Header("Feedback Settings")]
    public ParticleSystem correctTraceEffect;
    public ParticleSystem incorrectTraceEffect;
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip completionSound;

    private bool isTracing = false;
    private int currentPathIndex = 0;
    private List<Vector3> currentTracePath = new List<Vector3>();
    private Vector3 lastTracePosition;
    private bool letterCompleted = false;

    private UIManager uiManager;
    private GameManager gameManager;

    void Start()
    {
        InitializeComponents();
        SetupPathPoints();
        ResetLetter();
    }

    void Update()
    {
        if (letterCompleted) return;
        HandleInput();
    }

    void InitializeComponents()
    {
        if (tracingCamera == null)
            tracingCamera = Camera.main;

        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameManager>();

        if (tracingLine == null)
        {
            tracingLine = gameObject.AddComponent<LineRenderer>();
            SetupLineRenderer();
        }
    }

    void SetupLineRenderer()
    {
        tracingLine.material = Resources.Load<Material>("TracingLineMaterial");
        tracingLine.startWidth = 0.1f;
        tracingLine.endWidth = 0.1f;
        tracingLine.positionCount = 0;
        tracingLine.useWorldSpace = true;
        tracingLine.sortingOrder = 10;
    }

    void SetupPathPoints()
    {
        pathPoints.Sort((a, b) => a.pointIndex.CompareTo(b.pointIndex));

        if (pathPoints.Count == 0)
        {
            Debug.LogError("No path points assigned to LetterTracer!");
            return;
        }

        bool hasStart = false, hasEnd = false;
        foreach (var point in pathPoints)
        {
            if (point.isStartPoint) hasStart = true;
            if (point.isEndPoint) hasEnd = true;
        }

        if (!hasStart || !hasEnd)
            Debug.LogWarning("Path should have designated start and end points!");
    }

    void HandleInput()
    {
        Vector3 inputPosition = Vector3.zero;
        bool inputDown = false;
        bool inputHeld = false;
        bool inputUp = false;

        if (Input.GetMouseButtonDown(0))
        {
            inputDown = true;
            inputPosition = tracingCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            inputHeld = true;
            inputPosition = tracingCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            inputUp = true;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = tracingCamera.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
                inputDown = true;
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                inputHeld = true;
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                inputUp = true;
        }

        inputPosition.z = 0;

        if (inputDown)
        {
            StartTracing(inputPosition);
        }
        else if (inputHeld && isTracing)
        {
            UpdateTracing(inputPosition);
        }
        else if (inputUp)
        {
            EndTracing();
        }
    }

    void StartTracing(Vector3 position)
    {
        if (currentPathIndex < pathPoints.Count)
        {
            PathPoint targetPoint = pathPoints[currentPathIndex];

            if (targetPoint.IsWithinRadius(position))
            {
                isTracing = true;
                currentTracePath.Clear();
                currentTracePath.Add(position);
                lastTracePosition = position;

                targetPoint.ActivatePoint();
                UpdateLineRenderer();
                PlayCorrectFeedback(position);
            }
            else
            {
                PlayIncorrectFeedback(position);
                uiManager?.ShowMessage("Start tracing from the highlighted point!");
            }
        }
    }

    void UpdateTracing(Vector3 position)
    {
        if (!isTracing) return;

        if (Vector3.Distance(position, lastTracePosition) < minimumTraceDistance)
            return;

        bool withinPath = IsPositionWithinPath(position);

        if (withinPath)
        {
            currentTracePath.Add(position);
            lastTracePosition = position;
            UpdateLineRenderer();
            CheckPathPointReached(position);
        }
        else
        {
            PlayIncorrectFeedback(position);
            EndTracing();
        }
    }

    bool IsPositionWithinPath(Vector3 position)
    {
        if (currentPathIndex >= pathPoints.Count - 1) return true;

        PathPoint currentPoint = pathPoints[currentPathIndex];
        PathPoint nextPoint = pathPoints[currentPathIndex + 1];

        float distanceToPath = DistancePointToLineSegment(position, currentPoint.transform.position, nextPoint.transform.position);
        return distanceToPath <= pathTolerance;
    }

    float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        float lineLength = lineDir.magnitude;
        lineDir /= lineLength;

        Vector3 pointDir = point - lineStart;
        float dot = Vector3.Dot(pointDir, lineDir);

        Vector3 closestPoint;
        if (dot <= 0)
            closestPoint = lineStart;
        else if (dot >= lineLength)
            closestPoint = lineEnd;
        else
            closestPoint = lineStart + lineDir * dot;

        return Vector3.Distance(point, closestPoint);
    }

    void CheckPathPointReached(Vector3 position)
    {
        if (currentPathIndex < pathPoints.Count - 1)
        {
            PathPoint nextPoint = pathPoints[currentPathIndex + 1];

            if (nextPoint.IsWithinRadius(position))
            {
                nextPoint.ActivatePoint();
                currentPathIndex++;
                PlayCorrectFeedback(position);

                if (currentPathIndex >= pathPoints.Count - 1)
                {
                    CompleteLetter();
                }
            }
        }
    }

    void EndTracing()
    {
        isTracing = false;

        if (currentTracePath.Count > 0)
        {
            StartCoroutine(FadeTraceLine());
        }
    }

    void CompleteLetter()
    {
        letterCompleted = true;
        isTracing = false;

        if (correctTraceEffect != null)
            correctTraceEffect.Play();

        if (audioSource != null && completionSound != null)
            audioSource.PlayOneShot(completionSound);

        uiManager?.ShowCompletionMessage();
        gameManager?.OnLetterCompleted();

        foreach (var point in pathPoints)
        {
            point.ActivatePoint();
        }
    }

    public void ResetLetter()
    {
        letterCompleted = false;
        isTracing = false;
        currentPathIndex = 0;
        currentTracePath.Clear();

        foreach (var point in pathPoints) { }

        tracingLine.positionCount = 0;
        uiManager?.ShowInstructions();
    }

    void UpdateLineRenderer()
    {
        tracingLine.positionCount = currentTracePath.Count;
        tracingLine.SetPositions(currentTracePath.ToArray());
    }

    System.Collections.IEnumerator FadeTraceLine()
    {
        yield return new WaitForSeconds(0.5f);

        float fadeTime = 1.0f;
        float elapsedTime = 0;
        Color originalColor = tracingLine.material.color;

        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);
            Color newColor = originalColor;
            newColor.a = alpha;
            tracingLine.material.color = newColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        tracingLine.positionCount = 0;
        tracingLine.material.color = originalColor;
    }

    void PlayCorrectFeedback(Vector3 position)
    {
        if (correctTraceEffect != null)
        {
            correctTraceEffect.transform.position = position;
            correctTraceEffect.Play();
        }

        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);
    }

    void PlayIncorrectFeedback(Vector3 position)
    {
        if (incorrectTraceEffect != null)
        {
            incorrectTraceEffect.transform.position = position;
            incorrectTraceEffect.Play();
        }

        if (audioSource != null && incorrectSound != null)
            audioSource.PlayOneShot(incorrectSound);
    }

    public bool IsTracing => isTracing;
    public bool IsLetterCompleted => letterCompleted;
    public float CompletionPercentage => (float)currentPathIndex / Mathf.Max(1, pathPoints.Count - 1);
}
