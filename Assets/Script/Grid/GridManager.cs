using UnityEngine;

public class GridManager : MonoBehaviour
{

    [Header("난이도별 Grid 프리팹")]
    public FieldGrid easyGrid; // 4*3
    public FieldGrid normalGrid; // 5*4
    public FieldGrid hardGrid; // 6*5

    [Header("현재 활성 그리드")]
    public FieldGrid currentGrid;

    ///<summary
    /// 난이도에 맞게 그리드를 활성화하고 currentGrid를 설정
    /// </summary>

    public void SetGridDiff(Difficulty diff)
    {

        //모든 그리드 비활성화 
        if (easyGrid) { easyGrid.gameObject.SetActive(false); }
        if (normalGrid) { normalGrid.gameObject.SetActive(false); }
        if (hardGrid) { hardGrid.gameObject.SetActive(false); }

        //난이도 할당
        switch (diff)
        {
            case Difficulty.Easy:
                currentGrid = easyGrid;
                break;

            case Difficulty.Normal:
                currentGrid = normalGrid;
                break;

            case Difficulty.Hard:
                currentGrid = hardGrid;
                break;


            default:
                Debug.Log("해당 난이도가 전부입니다. -> expert는 구현 안한 상황");
                  currentGrid = easyGrid;  // 기본값을 easy그리드로 설정
                break;
        }

        if(currentGrid != null)
        {  // 현재 그리드 활성화 시키기
            currentGrid.gameObject.SetActive(true);

        } else  // 그리드 설정이 없을 경우, 
        {
            Debug.Log("현재 활성화 될 그리드가 없음");
        }
    
    }

    //노트 스포너에서 그리드를 가져갈 때 사용
    public FieldGrid GetCurrentGrid()   
    {
        return currentGrid;
    }

}
