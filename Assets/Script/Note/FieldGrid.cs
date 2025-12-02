using UnityEngine;

public class FieldGrid : MonoBehaviour
{
    [Header("그리드 설정")]
    public RectTransform board;
    public int cols = 6; // 가로
    public int rows = 5; // 세로

    public Vector2[,] cellLocalPos;   // board 기준 로컬 좌표

    private void Awake()
    {
        cellLocalPos = new Vector2[rows, cols];

        Vector2 size = board.rect.size;    // 판의 픽셀 크기
        float dx = size.x / cols;
        float dy = size.y / rows;

        // RectTransform의 로컬 좌표는 중심이 (0,0)
        float left = -size.x * 0.5f;
        float bottom = -size.y * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = left + dx * (c + 0.5f);  // 칸 중앙
                float y = bottom + dy * (r + 0.5f);

                cellLocalPos[r, c] = new Vector2(x, y);
            }
        }
    }

    public Vector2 GetCellLocalPos(int row, int col)
    {
        return cellLocalPos[row, col];
    }

    // 가장자리 셀 인덱스 랜덤
    public (int row, int col) GetRandomEdgeIndex()
    {
        int r, c;
        int side = Random.Range(0, 4); // 0=위,1=아래,2=왼,3=오른

        switch (side)
        {
            case 0: // 위
                r = 0;
                c = Random.Range(0, cols);
                break;
            case 1: // 아래
                r = rows - 1;
                c = Random.Range(0, cols);
                break;
            case 2: // 왼
                r = Random.Range(0, rows);
                c = 0;
                break;
            default: // 오른
                r = Random.Range(0, rows);
                c = cols - 1;
                break;
        }
        return (r, c);
    }

    // 내부(가장자리 제외) 셀 인덱스 랜덤
    public (int row, int col) GetRandomInnerIndex()
    {
        int r = Random.Range(1, rows - 1);
        int c = Random.Range(1, cols - 1);
        return (r, c);
    }
}


