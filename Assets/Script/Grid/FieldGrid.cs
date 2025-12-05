using UnityEngine;
using System.Linq;

public class FieldGrid : MonoBehaviour
{
    [Header("그리드 크기")]
    public int rows;    // 예: Easy 3, Normal 4, Hard 5
    public int cols;    // 예: Easy 4, Normal 5, Hard 6

    [Header("Cell 오브젝트 (자동 스캔됨)")]
    public GridCell[,] cells;

    private void Awake()
    {
        BuildCellArray();
    }

    /// <summary>
    /// 하위 오브젝트에서 GridCell을 자동 스캔하여
    /// row/col 인덱스에 맞춰 2D 배열로 구성
    /// </summary>
    private void BuildCellArray()
    {
        var cellList = GetComponentsInChildren<GridCell>(true);

        if (cellList.Length != rows * cols)
        {
            Debug.LogWarning($"[FieldGrid] 셀 개수({cellList.Length})가 rows*cols({rows * cols})와 일치하지 않습니다.");
        }

        cells = new GridCell[rows, cols];

        foreach (var cell in cellList)
        {
            if (cell.rect == null)
                cell.rect = cell.GetComponent<RectTransform>();

            cells[cell.row, cell.col] = cell;
        }

        Debug.Log("[FieldGrid] GridCell 2D 배열 구성이 완료되었습니다.");
    }

    /// <summary>
    /// 배치된 오브젝트의 RectTransform 좌표를 직접 반환
    /// </summary>
    public Vector2 GetCellLocalPos(int row, int col)
    {
        return cells[row, col].rect.anchoredPosition;
    }


    // =========================
    // JSON 기반 Edge 인덱스 변환
    // =========================
    public (int row, int col) GetEdgeIndexByJson(string edge, int index)
    {
        edge = edge.ToLower();

        switch (edge)
        {
            case "top":
                return (0, Mathf.Clamp(index, 0, cols - 1));
            case "bottom":
                return (rows - 1, Mathf.Clamp(index, 0, cols - 1));
            case "left":
                return (Mathf.Clamp(index, 0, rows - 1), 0);
            case "right":
                return (Mathf.Clamp(index, 0, rows - 1), cols - 1);
        }

        Debug.LogError($"[FieldGrid] 잘못된 edge: {edge}");
        return (0, 0);
    }

    /// <summary>
    /// 타겟 엣지가 JSON에서 주어졌다면 그 엣지를 기준으로 target 위치를 계산
    /// 없으면 반대편 엣지를 자동 선택
    /// </summary>
    public (int row, int col) GetTargetFromSpawn(string spawnEdge, int spawnIndex, string targetEdge)
    {
        if (!string.IsNullOrEmpty(targetEdge))
            return GetEdgeIndexByJson(targetEdge, spawnIndex);

        // 반대편 자동 계산
        var (sr, sc) = GetEdgeIndexByJson(spawnEdge, spawnIndex);
        return GetOppositeEdge(sr, sc);
    }

    public (int row, int col) GetOppositeEdge(int r, int c)
    {
        if (r == 0) return (rows - 1, c);
        if (r == rows - 1) return (0, c);
        if (c == 0) return (r, cols - 1);
        if (c == cols - 1) return (r, 0);

        // edge가 아닌 경우
        return (r, c);
    }
}
