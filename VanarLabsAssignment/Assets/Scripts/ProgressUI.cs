using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    public LetterTracer tracer;   // Drag your LetterTracer here
    public Slider progressSlider; // Assign Slider in inspector

    void Start()
    {
        if (tracer != null)
        {
            tracer.OnProgress += UpdateProgress;
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.value = 0;
        }
    }

    void UpdateProgress(int current, int total)
    {
        if (progressSlider != null)
        {
            progressSlider.maxValue = total;
            progressSlider.value = current;
        }
    }
}
