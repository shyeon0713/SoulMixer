using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldGrid : MonoBehaviour
{
    [Header("그리드 크기")]
    public int rows;    // 예: Easy 3, Normal 4, Hard 5
    public int cols;    // 예: Easy 4, Normal 5, Hard 6

    [Header("참조")]
    public HighlightPool highlightPool;
    public GridCell[,] cells;

    private void Awake()
    {
        BuildCellArray();
    }


    // 셀을 2차원 배열 방식으로 구성
    private void BuildCellArray()
    {
        var cellList = GetComponentsInChildren<GridCell>(true);
        cells = new GridCell[rows, cols];

        foreach (var c in cellList)
        {
            if (c.row < rows && c.col < cols)
            {
                cells[c.row, c.col] = c;
            }

            else  // 셀 좌표가 해당 그리드보다 초과될 경우
                Debug.LogWarning($"[FieldGrid] 셀 좌표 초과: ({c.row},{c.col})");
        }
        Debug.Log($"[FieldGrid] Initialized rows={rows}, cols={cols}, foundCells={cellList.Length}");
    }

    //셀 위치 반환
    public Vector2 GetCellLocalPos(int r, int c)
    {
        return cells[r, c].rect.anchoredPosition;
    }

    #region - 하이라이트 부분 출력
    //셀 하나의 하이라이트 출력
    public void HighlightCell(int r, int c)
    {
        if (cells[r, c] == null)
        {
            Debug.LogError($"[FieldGrid] HighlightCell 실패: ({r},{c}) 셀이 null");
            return;
        }

        var cell = cells[r, c];
        var img = highlightPool.Get();

        img.rectTransform.anchoredPosition = cell.rect.anchoredPosition;
        img.rectTransform.sizeDelta = cell.rect.sizeDelta;
    }

    //경로 전체를 표시
    public void HighlightPath(List<(int r, int c)> path)
    {
        foreach (var (r, c) in path)
        {
            HighlightCell(r, c);
        }
    }

    //모든 하이라이트 제거
    public void ClearHighlights()
    {
        highlightPool.ClearAll();
    }

    #endregion

    //Json 기반 가장자리 
    public (int r, int c) GetEdgeIndexByJson(string edge, int index)
    {
        edge = edge.ToLower();

        switch (edge)
        {
            case "top":
                if (index >= cols) Debug.LogError("[FieldGrid] top index 범위 초과"); 
                return (0, Mathf.Clamp(index, 0, cols - 1));

            case "bottom":
                if (index >= cols) Debug.LogError("[FieldGrid] bottom index 범위 초과");
                return (rows - 1, Mathf.Clamp(index, 0, cols - 1));

            case "left":
                if (index >= rows) Debug.LogError("[FieldGrid] left index 범위 초과");
                return (Mathf.Clamp(index, 0, rows - 1), 0);

            case "right":
                if (index >= rows) Debug.LogError("[FieldGrid] right index 범위 초과");
                return (Mathf.Clamp(index, 0, rows - 1), cols - 1);
        }

        Debug.LogError($"[FieldGrid] Unknown edge: {edge}");
        return (0, 0);
    }



    // 랜덤 워크 기반 경로 생성 -> 랜덤 위크 알고리즘 활용
    public List<(int r, int c)> GenerateRandomWalk(int sr, int sc, int steps)
    {
        List<(int r, int c)> path = new();

        int cr = sr;
        int cc = sc;

        for (int i = 0; i < steps; i++)
        {
            List<(int nr, int nc)> candidates = new();

            if (cr + 1 < rows) candidates.Add((cr + 1, cc)); // down
            if (cr - 1 >= 0) candidates.Add((cr - 1, cc)); // up
            if (cc - 1 >= 0) candidates.Add((cr, cc - 1)); // left
            if (cc + 1 < cols) candidates.Add((cr, cc + 1)); // right

            if (candidates.Count == 0)
                break;

            // 직전 셀과 중복 이동 피하기
            if (path.Count > 1)
            {
                var prev = path[path.Count - 2];
                candidates.Remove(prev);
            }

            if (candidates.Count == 0)
                break;

            (cr, cc) = candidates[Random.Range(0, candidates.Count)];
            path.Add((cr, cc));
        }

        return path;
    }
}