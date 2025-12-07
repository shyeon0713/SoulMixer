using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public bool IsValidCell(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }

    /// <summary>자식 GridCell들을 rows x cols 배열로 구성</summary>
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
            else
            {
                Debug.LogWarning($"[FieldGrid] 셀 좌표 초과: ({c.row},{c.col})");
            }
        }

        Debug.Log($"[FieldGrid] Initialized rows={rows}, cols={cols}, foundCells={cellList.Length}");
    }

    /// <summary>
    /// 그리드 내부 좌표계(셀 부모 기준 anchoredPosition) 반환.
    /// 노트 이동용으로 사용.
    /// </summary>
    public Vector2 GetCellLocalPos(int r, int c)
    {
        return cells[r, c].rect.anchoredPosition;
    }

    #region 하이라이트 관련

    /// <summary>
    /// 셀 (r,c)의 위치를 highlightPool.parentLayer 기준 local 좌표로 변환
    /// </summary>
    private Vector2 GetCellPosOnHighlightLayer(int r, int c)
    {
        if (cells == null || cells[r, c] == null || highlightPool == null || highlightPool.parentLayer == null)
        {
            Debug.LogError("[FieldGrid] GetCellPosOnHighlightLayer: 참조가 null입니다.");
            return Vector2.zero;
        }

        var cellRect = cells[r, c].rect;
        var parentRect = highlightPool.parentLayer;

        Canvas canvas = parentRect.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null ? canvas.worldCamera : null; // Overlay이면 null 허용

        // 셀의 월드 위치 → 화면 좌표
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, cellRect.position);

        // 화면 좌표 → parentLayer 기준 로컬 좌표
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPos,
            uiCamera,
            out localPos
        );

        return localPos;
    }

    /// <summary>셀 하나에 하이라이트 한 개 출력</summary>
    public void HighlightCell(int r, int c)
    {
        if (cells == null || cells[r, c] == null)
        {
            Debug.LogError($"[FieldGrid] HighlightCell 실패: cells 또는 셀({r},{c})가 null");
            return;
        }
        if (highlightPool == null || highlightPool.parentLayer == null)
        {
            Debug.LogError("[FieldGrid] HighlightCell 실패: highlightPool 또는 parentLayer가 null");
            return;
        }

        Vector2 localPos = GetCellPosOnHighlightLayer(r, c);
        var cellRect = cells[r, c].rect;

        var img = highlightPool.Get();
        img.rectTransform.anchoredPosition = localPos;
        img.rectTransform.sizeDelta = cellRect.sizeDelta;

        Debug.Log($"[FieldGrid] HighlightCell ({r},{c}) local={localPos}");
    }

    /// <summary>경로 전체에 하이라이트 찍기 (디버그용 포함)</summary>
    public void HighlightPath(List<(int r, int c)> path)
    {
        if (path == null) return;

        foreach (var (r, c) in path)
            HighlightCell(r, c);
    }

    /// <summary>모든 하이라이트 제거 (전부 풀로 반환)</summary>
    public void ClearHighlights()
    {
        if (highlightPool != null)
            highlightPool.ClearAll();
    }

    /// <summary>
    /// 목표 셀 하나에만 하이라이트 생성하고 Image 반환
    /// (노트별 타깃 표시용)
    /// </summary>
    public Image HighlightSingleCell(int r, int c)
    {
        if (cells == null || cells[r, c] == null)
        {
            Debug.LogError($"[FieldGrid] HighlightSingleCell 실패: cells 또는 셀({r},{c})가 null");
            return null;
        }
        if (highlightPool == null || highlightPool.parentLayer == null)
        {
            Debug.LogError("[FieldGrid] HighlightSingleCell 실패: highlightPool 또는 parentLayer가 null");
            return null;
        }

        Vector2 localPos = GetCellPosOnHighlightLayer(r, c);
        var cellRect = cells[r, c].rect;

        var img = highlightPool.Get();
        img.rectTransform.anchoredPosition = localPos;
        img.rectTransform.sizeDelta = cellRect.sizeDelta;

        return img;
    }

    /// <summary>특정 하이라이트 하나만 풀로 반환</summary>
    public void ReleaseHighlight(Image img)
    {
        if (highlightPool != null && img != null)
            highlightPool.Release(img);
    }

    #endregion

    #region 그리드 / 엣지 유틸

    /// <summary>JSON에서 오는 edge/index를 셀 인덱스로 변환</summary>
    public (int r, int c) GetEdgeIndexByJson(string edge, int index)
    {
        edge = edge.ToLower();
        index = Mathf.Max(0, index);

        switch (edge)
        {
            case "top":
                if (index >= cols) Debug.LogError("[FieldGrid] top index 범위 초과");
                return (rows - 1, Mathf.Clamp(index, 0, cols - 1));

            case "bottom":
                if (index >= cols) Debug.LogError("[FieldGrid] bottom index 범위 초과");
                return (0, Mathf.Clamp(index, 0, cols - 1));

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

    /// <summary>반대편 edge 리턴</summary>
    public string GetOppositeEdge(string edge)
    {
        return edge switch
        {
            "top" => "bottom",
            "bottom" => "top",
            "left" => "right",
            "right" => "left",
            _ => "bottom"
        };
    }

    #endregion

    #region 랜덤 경로(랜덤 워크) - 반대편 엣지까지

    /// <summary>
    /// 시작 셀(sr,sc)에서 시작해 spawnEdge의 반대편 엣지까지
    /// - 이미 지나온 셀 재방문 X
    /// - 대각선 이동 X (상하좌우만)
    /// - minSteps 이상, maxSteps 이하 길이
    /// 경로를 DFS 기반 랜덤 워크로 생성.
    /// </summary>
    public List<(int r, int c)> GenerateRandomPathToOppositeEdge(
        int sr, int sc,
        string spawnEdge,
        int minSteps,
        int maxSteps)
    {
        string targetEdge = GetOppositeEdge(spawnEdge.ToLower());
        minSteps = Mathf.Max(1, minSteps);
        maxSteps = Mathf.Clamp(maxSteps, minSteps, rows * cols);

        var path = new List<(int r, int c)>();
        bool[,] visited = new bool[rows, cols];

        bool IsOnTargetEdge(int r, int c)
        {
            return targetEdge switch
            {
                "top" => r == rows - 1,
                "bottom" => r == 0,
                "left" => c == 0,
                "right" => c == cols - 1,
                _ => false
            };
        }

        bool DFS(int r, int c)
        {
            path.Add((r, c));
            visited[r, c] = true;

            // 반대편 엣지에 도달했고, 최소 길이를 만족하면 성공
            if (IsOnTargetEdge(r, c) && path.Count >= minSteps)
                return true;

            // 너무 길어지면 백트래킹
            if (path.Count >= maxSteps)
            {
                path.RemoveAt(path.Count - 1);
                visited[r, c] = false;
                return false;
            }

            var neighbors = new List<(int nr, int nc)>();

            // 상하좌우만 (대각선 금지), 아직 방문 안 한 곳만
            if (r + 1 < rows && !visited[r + 1, c]) neighbors.Add((r + 1, c));
            if (r - 1 >= 0 && !visited[r - 1, c]) neighbors.Add((r - 1, c));
            if (c + 1 < cols && !visited[r, c + 1]) neighbors.Add((r, c + 1));
            if (c - 1 >= 0 && !visited[r, c - 1]) neighbors.Add((r, c - 1));

            // 랜덤 순서로 섞기
            for (int i = 0; i < neighbors.Count; i++)
            {
                int j = Random.Range(i, neighbors.Count);
                (neighbors[i], neighbors[j]) = (neighbors[j], neighbors[i]);
            }

            foreach (var (nr, nc) in neighbors)
            {
                if (DFS(nr, nc))
                    return true;
            }

            // 막혀서 실패 → 백트래킹
            path.RemoveAt(path.Count - 1);
            visited[r, c] = false;
            return false;
        }
        //DFS로 반대편 엣지까지 시도
        bool success = DFS(sr, sc);
        // 2) 실패한 경우: 최소 2칸짜리 (시작→반대편 엣지 한 칸) 경로 강제 생성
        if (!success)
        {
            path.Clear();
            path.Add((sr, sc));

            // spawnEdge 방향에 따라 인덱스 결정 (행/열)
            int index = (spawnEdge == "top" || spawnEdge == "bottom") ? sc : sr;
            var (tr, tc) = GetEdgeIndexByJson(targetEdge, index);

            // 혹시라도 같은 셀로 나오면, 인덱스를 한 칸 옆으로 밀어서라도 다르게 만든다
            if (tr == sr && tc == sc)
            {
                if (targetEdge == "top" || targetEdge == "bottom")
                {
                    int newIndex = Mathf.Clamp(index + 1, 0, cols - 1);
                    (tr, tc) = GetEdgeIndexByJson(targetEdge, newIndex);
                }
                else
                {
                    int newIndex = Mathf.Clamp(index + 1, 0, rows - 1);
                    (tr, tc) = GetEdgeIndexByJson(targetEdge, newIndex);
                }
            }

            // 최종적으로 시작과 목표가 다르도록 보장
            if (!(tr == sr && tc == sc) && IsValidCell(tr, tc))
                path.Add((tr, tc));
        }

        return path;
    }
    #endregion
}
