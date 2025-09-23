using UnityEngine;
using UnityEngine.UI;

public class PathPoint : MonoBehaviour
{
    public bool isActivated = false;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Activate()
    {
        if (!isActivated)
        {
            isActivated = true;
            image.color = Color.green; 
        }
    }
}
