using UnityEngine;
using UnityEngine.UI;

public class PathPoint : MonoBehaviour
{
    public int index; // Order in the letter
    public bool isActive = false;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
        image.color = Color.gray; // Inactive color
    }

    public void Activate()
    {
        isActive = true;
        image.color = Color.green; // Activated color
    }

    public void Highlight()
    {
        if (!isActive)
            image.color = Color.yellow; // Only for next target
    }


    public void ResetPoint()
    {
        isActive = false;
        image.color = Color.gray;
    }

    public void SetTargetHighlight(bool active)
    {
        if (active)
            image.color = Color.yellow; // Target glow color
        else if (!isActive)
            image.color = Color.gray;   // Reset back to normal
    }
}
