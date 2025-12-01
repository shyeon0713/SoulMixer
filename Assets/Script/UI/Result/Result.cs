using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Result : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image rankImage;
    [SerializeField] private TMP_Text rankComment;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Slider resultGauge;

    [System.Serializable]
    public struct RankData
    {
        public string rankName;    

        [Range(0f, 1f)]
        public float minGauge;     // 최소 달성 게이지 (0.0 ~ 1.0)

        public Sprite rankSprite;  
        [TextArea]
        public string comment;     // 평가 멘트
    }

    public List<RankData> rankList;


    #region - 게임이 끝났을 때 GameEntry가 해당 메서드를 호출
    public void OpenResult(float finalGauge, int maxCombo)
    {
        gameObject.SetActive(true); // 결과창 켜기

        finalScoreText.text = $"Max Combo: {maxCombo}";
        resultGauge.value = finalGauge;

        // 등급 계산 로직
        RankData finalRank = CalculateRank(finalGauge);

        //  UI 적용
        if (rankImage != null) rankImage.sprite = finalRank.rankSprite;
        if (rankComment != null) rankComment.text = finalRank.comment;

        // 추후 결과 애니메이션 추가 (보류사항)
      
    }

    #endregion

    #region - 게이지 수치에 따라 등급을 결정하는 메서드

    private RankData CalculateRank(float gauge)
    {
        // 리스트를 돌면서 조건에 맞는 등급을 찾음
        foreach (var data in rankList)
        {
            if (gauge >= data.minGauge)
            {
                return data;
            }
        }

        
        if (rankList.Count > 0)
            return rankList[rankList.Count - 1];

        return new RankData(); // 에러 방지용 빈 값
    }

    #endregion
}
