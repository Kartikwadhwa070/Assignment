using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("References")]
    public LetterTracer tracer;      // Drag your LetterTracer here
    public GameObject currentCanvas; // The tracing canvas
    public GameObject nextCanvas;    // The success / next canvas

    void Start()
    {
        if (tracer != null)
        {
            tracer.OnComplete += SwitchCanvas;
        }

        if (nextCanvas != null)
            nextCanvas.SetActive(false);
    }

    void SwitchCanvas()
    {
        if (currentCanvas != null)
            currentCanvas.SetActive(false);

        if (nextCanvas != null)
            nextCanvas.SetActive(true);

        Debug.Log("✅ Switched to success canvas!");
    }
}
