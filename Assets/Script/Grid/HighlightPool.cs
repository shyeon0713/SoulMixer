using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightPool : MonoBehaviour
{
    [Header("프리팹 및 레이어")]
    public GameObject highlightPrefab;      // 하이라이터 프리팹
    public RectTransform parentLayer;      //  Grid 내부에서 하이라이트가 표시

    [Header("초기 풀 개수")]
    public int initialCount = 20;

    private readonly Stack<Image> pool = new();
    private readonly List<Image> activeList = new();
    void Awake()
    {
        Warmup();
    }

    void Warmup()
    {
        if (highlightPrefab == null || parentLayer == null)
        {
            Debug.LogError("[HighlightPool] highlightPrefab 또는 parentLayer가 설정되지 않음.");
            return;
        }

        for (int i = 0; i < initialCount; i++)
        {
            var inst = Instantiate(highlightPrefab, parentLayer);
            inst.SetActive(false);

            var img = inst.GetComponent<Image>();
            pool.Push(img);
        }
    }

    public Image Get()
    {
        Image img;

        if (pool.Count > 0)
            img = pool.Pop();
        else
            img = Instantiate(highlightPrefab, parentLayer).GetComponent<Image>();

        img.gameObject.SetActive(true);
        activeList.Add(img);
        return img;
    }

    public void ClearAll()
    {
        foreach (var img in activeList)
        {
            img.gameObject.SetActive(false);
            pool.Push(img);
        }
        activeList.Clear();
    }
}
