using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Main component handling letter tracing mechanics with multi-stroke support
/// </summary>
public class LetterTracer : MonoBehaviour
{
    [Header("Tracing Settings")]
    public List<PathPoint> pathPoints = new List<PathPoint>();
    public LineRenderer tracingLine;
    public float pathTolerance = 0.5f;
    public float minimumTraceDistance = 0.1f;

    [Header("Multi-Stroke Settings")]
    public bool allowMultipleStrokes = true;
    public float strokeTimeout = 3.0f; // Time before stroke resets

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

    // Private variables
    private bool isTracing = false;
    private int currentPathIndex = 0;
    private List<Vector3> currentTracePath = new List<Vector3>();
    private Vector3 lastTracePosition;
    private bool letterCompleted = false;

    // Multi-stroke variables
    private List<LineRenderer> completedStrokes = new List<LineRenderer>();
    private float lastInputTime;
    private bool waitingForNextStroke = false;

    // UI References
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
        CheckStrokeTimeout();
    }

    /// <summary>
    /// Initialize required components and references
    /// </summary>
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

    /// <summary>
    /// Configure the line renderer for tracing visualization
    /// </summary>
    void SetupLineRenderer()
    {
        tracingLine.material = Resources.Load<Material>("TracingLineMaterial");
        if (tracingLine.material == null)
        {
            // Create a basic material if none found
            tracingLine.material = new Material(Shader.Find("Sprites/Default"));
            tracingLine.material.color = Color.blue;
        }
        tracingLine.startWidth = 0.1f;
        tracingLine.endWidth = 0.1f;
        tracingLine.positionCount = 0;
        tracingLine.useWorldSpace = true;
        tracingLine.sortingOrder = 10;
    }

    /// <summary>
    /// Setup and validate path points
    /// </summary>
    void SetupPathPoints()
    {
        // Sort path points by index
        pathPoints.Sort((a, b) => a.pointIndex.CompareTo(b.pointIndex));

        // Validate path setup
        if (pathPoints.Count == 0)
        {
            Debug.LogError("No path points assigned to LetterTracer!");
            return;
        }

        // Ensure we have start and end points
        bool hasStart = false, hasEnd = false;
        foreach (var point in pathPoints)
        {
            if (point.isStartPoint) hasStart = true;
            if (point.isEndPoint) hasEnd = true;
        }

        if (!hasStart || !hasEnd)
        {
            Debug.LogWarning("Path should have designated start and end points!");
        }
    }

    /// <summary>
    /// Check if stroke has timed out
    /// </summary>
    void CheckStrokeTimeout()
    {
        if (waitingForNextStroke && Time.time - lastInputTime > strokeTimeout)
        {
            Debug.Log("Stroke timeout - resetting letter");
            ResetLetter();
        }
    }

    /// <summary>
    /// Handle mouse and touch input for tracing
    /// </summary>
    void HandleInput()
    {
        Vector3 inputPosition = Vector3.zero;
        bool inputDown = false;
        bool inputHeld = false;
        bool inputUp = false;

        // Handle mouse input
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

        // Handle touch input
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

        inputPosition.z = 0; // Ensure 2D positioning

        // Process input
        if (inputDown)
        {
            StartTracing(inputPosition);
            lastInputTime = Time.time;
        }
        else if (inputHeld && isTracing)
        {
            UpdateTracing(inputPosition);
            lastInputTime = Time.time;
        }
        else if (inputUp)
        {
            EndTracing();
            lastInputTime = Time.time;
        }
    }

    /// <summary>
    /// Begin tracing from the input position
    /// </summary>
    void StartTracing(Vector3 position)
    {
        // Check if we can start from current position
        if (CanStartFromPosition(position))
        {
            isTracing = true;
            waitingForNextStroke = false;
            currentTracePath.Clear();
            currentTracePath.Add(position);
            lastTracePosition = position;

            // Activate the starting point
            PathPoint targetPoint = GetNearestValidStartPoint(position);
            if (targetPoint != null)
            {
                targetPoint.ActivatePoint();
            }

            // Update line renderer
            UpdateLineRenderer();

            // Provide feedback
            PlayCorrectFeedback(position);
        }
        else
        {
            // Starting in wrong location
            PlayIncorrectFeedback(position);
            ShowHint();
        }
    }

    /// <summary>
    /// Check if we can start tracing from this position
    /// </summary>
    bool CanStartFromPosition(Vector3 position)
    {
        // If just starting, must be near a start point
        if (currentPathIndex == 0)
        {
            foreach (var point in pathPoints)
            {
                if (point.isStartPoint && point.IsWithinRadius(position))
                    return true;
            }
            return false;
        }

        // If continuing, check if we're near the next expected point
        // or if we can start a new stroke from an untraced point
        return GetNearestValidStartPoint(position) != null;
    }

    /// <summary>
    /// Get the nearest valid point to start from
    /// </summary>
    PathPoint GetNearestValidStartPoint(Vector3 position)
    {
        // First check for the next expected point in sequence
        if (currentPathIndex < pathPoints.Count)
        {
            PathPoint expectedNext = pathPoints[currentPathIndex];
            if (expectedNext.IsWithinRadius(position))
                return expectedNext;
        }

        // For multi-stroke letters, allow starting from any unactivated point
        // that comes after already activated points
        foreach (var point in pathPoints)
        {
            if (point.pointIndex >= currentPathIndex &&
                !point.IsActivated &&
                point.IsWithinRadius(position))
            {
                return point;
            }
        }

        return null;
    }

    /// <summary>
    /// Update tracing as user moves input
    /// </summary>
    void UpdateTracing(Vector3 position)
    {
        if (!isTracing) return;

        // Check minimum distance to avoid too many points
        if (Vector3.Distance(position, lastTracePosition) < minimumTraceDistance)
            return;

        // Validate if position is within acceptable path
        bool withinPath = IsPositionWithinPath(position);

        if (withinPath)
        {
            // Add to trace path
            currentTracePath.Add(position);
            lastTracePosition = position;
            UpdateLineRenderer();

            // Check if we've reached the next path point
            CheckPathPointReached(position);
        }
        else
        {
            // Strayed too far from path - but don't end stroke immediately
            // Just don't add the point and provide subtle feedback
            PlayIncorrectFeedback(position);
        }
    }

    /// <summary>
    /// Check if current position is within acceptable tracing path
    /// </summary>
    bool IsPositionWithinPath(Vector3 position)
    {
        if (currentPathIndex >= pathPoints.Count - 1) return true;

        // Check distance to the line between current and next path points
        for (int i = currentPathIndex; i < pathPoints.Count - 1; i++)
        {
            PathPoint currentPoint = pathPoints[i];
            PathPoint nextPoint = pathPoints[i + 1];

            float distanceToPath = DistancePointToLineSegment(position,
                currentPoint.transform.position, nextPoint.transform.position);

            if (distanceToPath <= pathTolerance)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Calculate distance from point to line segment
    /// </summary>
    float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        float lineLength = lineDir.magnitude;

        if (lineLength < 0.001f) return Vector3.Distance(point, lineStart);

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

    /// <summary>
    /// Check if we've reached the next path point
    /// </summary>
    void CheckPathPointReached(Vector3 position)
    {
        // Check all remaining points to see if any are reached
        for (int i = currentPathIndex; i < pathPoints.Count; i++)
        {
            PathPoint point = pathPoints[i];

            if (!point.IsActivated && point.IsWithinRadius(position))
            {
                // Activate the reached point
                point.ActivatePoint();
                currentPathIndex = Mathf.Max(currentPathIndex, i + 1);

                PlayCorrectFeedback(position);

                // Check if letter is complete
                if (currentPathIndex >= pathPoints.Count)
                {
                    CompleteLetter();
                    return;
                }

                // Check if this was an endpoint (end of stroke)
                if (point.isEndPoint)
                {
                    EndTracing();
                    return;
                }

                break; // Only activate one point per frame
            }
        }
    }

    /// <summary>
    /// End the current tracing session
    /// </summary>
    void EndTracing()
    {
        if (!isTracing) return;

        isTracing = false;

        // Save the current stroke
        if (currentTracePath.Count > 1)
        {
            SaveCurrentStroke();
        }

        // Clear current tracing line
        tracingLine.positionCount = 0;

        // If not completed, prepare for next stroke
        if (!letterCompleted && currentPathIndex < pathPoints.Count)
        {
            waitingForNextStroke = true;
            ShowNextStrokeHint();
        }
    }

    /// <summary>
    /// Save the current stroke as a permanent line
    /// </summary>
    void SaveCurrentStroke()
    {
        // Create a new LineRenderer for this completed stroke
        GameObject strokeObject = new GameObject($"Stroke_{completedStrokes.Count}");
        strokeObject.transform.SetParent(transform);

        LineRenderer strokeLine = strokeObject.AddComponent<LineRenderer>();
        strokeLine.material = tracingLine.material;
        strokeLine.startWidth = tracingLine.startWidth;
        strokeLine.endWidth = tracingLine.endWidth;
        strokeLine.useWorldSpace = true;
        strokeLine.sortingOrder = 5; // Below current tracing line
        strokeLine.color = Color.green; // Completed stroke color

        strokeLine.positionCount = currentTracePath.Count;
        strokeLine.SetPositions(currentTracePath.ToArray());

        completedStrokes.Add(strokeLine);
    }

    /// <summary>
    /// Show hint for next stroke
    /// </summary>
    void ShowNextStrokeHint()
    {
        if (currentPathIndex < pathPoints.Count)
        {
            PathPoint nextPoint = pathPoints[currentPathIndex];

            // Highlight the next point to trace
            if (nextPoint != null)
            {
                StartCoroutine(HighlightPoint(nextPoint));
            }

            uiManager?.ShowMessage("Great! Now trace the next part.", 2.0f);
        }
    }

    /// <summary>
    /// Highlight a point to show where to start next
    /// </summary>
    System.Collections.IEnumerator HighlightPoint(PathPoint point)
    {
        SpriteRenderer sr = point.GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;

        // Flash the point
        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.yellow;
            yield return new WaitForSeconds(0.3f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Show hint about where to start
    /// </summary>
    void ShowHint()
    {
        if (currentPathIndex == 0)
        {
            uiManager?.ShowMessage("Start tracing from the green dot!");
        }
        else if (currentPathIndex < pathPoints.Count)
        {
            uiManager?.ShowMessage("Continue from the highlighted point.");
        }
    }

    /// <summary>
    /// Complete the letter tracing
    /// </summary>
    void CompleteLetter()
    {
        letterCompleted = true;
        isTracing = false;
        waitingForNextStroke = false;

        // Play completion effects
        if (correctTraceEffect != null)
            correctTraceEffect.Play();

        if (audioSource != null && completionSound != null)
            audioSource.PlayOneShot(completionSound);

        // Notify UI and game manager
        uiManager?.ShowCompletionMessage();
        gameManager?.OnLetterCompleted();

        // Celebrate all path points
        foreach (var point in pathPoints)
        {
            if (!point.IsActivated)
                point.ActivatePoint();
        }
    }

    /// <summary>
    /// Reset the letter for a new attempt
    /// </summary>
    public void ResetLetter()
    {
        letterCompleted = false;
        isTracing = false;
        waitingForNextStroke = false;
        currentPathIndex = 0;
        currentTracePath.Clear();

        // Reset all path points
        foreach (var point in pathPoints)
        {
            point.ResetPoint();
        }

        // Clear current line renderer
        tracingLine.positionCount = 0;

        // Clear completed strokes
        foreach (var stroke in completedStrokes)
        {
            if (stroke != null)
                DestroyImmediate(stroke.gameObject);
        }
        completedStrokes.Clear();

        uiManager?.ShowInstructions();
    }

    /// <summary>
    /// Update the line renderer with current trace path
    /// </summary>
    void UpdateLineRenderer()
    {
        tracingLine.positionCount = currentTracePath.Count;
        tracingLine.SetPositions(currentTracePath.ToArray());
    }

    /// <summary>
    /// Play correct tracing feedback
    /// </summary>
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

    /// <summary>
    /// Play incorrect tracing feedback
    /// </summary>
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

    // Public properties for external access
    public bool IsTracing => isTracing;
    public bool IsLetterCompleted => letterCompleted;
    public float CompletionPercentage => (float)currentPathIndex / Mathf.Max(1, pathPoints.Count);
}