using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    public int row;
    public int col;

    public RectTransform rect;
    public Image highlight;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
    }

    public void SetHighlight(bool on)
    {
        if (highlight != null)
            highlight.enabled = on;
    }
}
