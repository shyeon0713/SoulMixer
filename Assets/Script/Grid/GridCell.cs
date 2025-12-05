using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    public int row;
    public int col;

    public RectTransform rect;
    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
    }

}
