using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayUI : MonoBehaviour
{
    [Header("판정레퍼런스")]
    [SerializeField] private Judge judge;  // 판정

    [Header("UI부분")]
    [SerializeField] private TMP_Text comboText; // 콤보 효과 텍스트
    [SerializeField] private Slider scoreSlider; // 점수 슬라이더
    [SerializeField] private List<Image> customerimg; // 현재 손님반응 이미지
    // 평온 불안 분노로 구분

    private int combo;
    private int maxCombo;

    private int score;

    //판정별 카운트
    private Dictionary<HitGrade, int> _gradeCounts = new Dictionary<HitGrade, int>();

     void Awake()
    {
       if(judge == null)
        {
            Debug.Log("judge 참조가 비어있음");
            return;
        }

      //  judge.OnHit += HandleHit;
     //   judge.OnMiss += HandleMiss;

        foreach (HitGrade g in System.Enum.GetValues(typeof(HitGrade)))
        {
            _gradeCounts[g] = 0;
        }
        // 모든 판정 종류에 대해 _gradeCounts 딕셔너리 안에 판정값을 0으로 초기화
    }

}
