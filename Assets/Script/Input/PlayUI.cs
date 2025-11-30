using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayUI : MonoBehaviour
{
    [Header("판정레퍼런스")]
    [SerializeField] private Judge judge;  // 판정

    [Header("UI부분")]
    [SerializeField] private TMP_Text comboText; // 콤보 효과 텍스트
    [SerializeField] private TMP_Text scoreText; // 점수 텍스트
    [SerializeField] private Slider scoreSlider; // 점수 슬라이더
    [SerializeField] private Image judgeimage;


    [SerializeField] private List<Sprite> customerimg; // 현재 손님반응 이미지
    // 평온 불안 분노로 구분

    [Header("게이지 증가/감소량")]
    [SerializeField] private float perfectGain = 0.08f;
    [SerializeField] private float greatGain = 0.05f;
    [SerializeField] private float goodGain = 0.02f;
    [SerializeField] private float missLoss = 0.10f;

    private float sildervalue = 0f;
    private Coroutine fillRoutine;

    private int combo;
    private int maxCombo;


    //판정별 카운트
    private Dictionary<HitGrade, int> _gradeCounts = new Dictionary<HitGrade, int>();

     void OnEnable()
    {
        if(judge != null)
        {
            judge.OnHit += HandleHit;
            judge.OnMiss += HandleMiss;
        }

        if (scoreSlider != null)
        {
            scoreSlider.value = sildervalue;
        }
            
    }
     void OnDisable()
    {
        if (judge != null)
        {
            judge.OnHit -= HandleHit;
            judge.OnMiss -= HandleMiss;
        }
        
    }


    #region -  // 판정 UI 처리

    void HandleHit(NoteData note, HitGrade grade)
    {
       switch (grade)
        {
            case HitGrade.Perfect:
                Addscore(perfectGain); // 게이지 점수 추가

                combo++; // 점수 추가
                scoreText.text = combo.ToString(); // 점수 텍스트 출력

                comboText.text = "Perfect!";  // Good 텍스트 출력

                judgeimage.sprite = customerimg[0]; // 이미지 변경 -> 파란색 NPC 이미지 출력
                break;

            case HitGrade.Great:
                Addscore(greatGain); // 게이지 점수 추가

                combo++; // 점수 추가
                scoreText.text = combo.ToString(); // 점수 텍스트 출력

                comboText.text = "Great!";  // Good 텍스트 출력

                judgeimage.sprite = customerimg[0]; // 이미지 변경 -> 파란색 NPC 이미지 출력
                break;

            case HitGrade.Good:
                Addscore(goodGain); // 게이지 점수 추가

                combo++; // 점수 추가
                scoreText.text = combo.ToString(); // 점수 텍스트 출력

                comboText.text = "Good!";  // Good 텍스트 출력

                judgeimage.sprite = customerimg[2]; // 이미지 변경 -> normal NPC 이미지 출력
                break;

           // case HitGrade.Miss:  // miss인 경우에는 HandleMiss에서 처리해야하는거 아닌가?
           //     break; 
        }
    }

    #endregion


    #region - // 미스 UI 처리
    void HandleMiss(NoteData note)
    {
        combo = 0; // 콤보 점수 리셋
        comboText.text = "Miss";  // Miss 출력

        scoreText.text = combo.ToString();

        judgeimage.sprite = customerimg[1]; // 이미지 변경  -> 빨간색 NPC 이미지로 변경

        //카메라 쉐이크 -> 싱글톤을 만들어서 필요할 때 호출 : 근데 Miss때만 사용할 것 같음
        if (CameraShaker.instance != null)
            CameraShaker.instance.Shake(0.15f, 0.2f);

        if (sildervalue > 0)  // 게이지 점수가 0일때만 변화
        {
            Addscore(-missLoss); // 게이지 점수 추가
        }

    }
    #endregion


    #region - 게이지 업데이트 함수 / 코루틴 기반으로 게이지 채움
    private void Addscore(float delta)
    {
        sildervalue += delta;
        sildervalue = Mathf.Clamp01(sildervalue);    // 0에서 1사이의 값으로 제한

        if(fillRoutine != null)
        {
            StopCoroutine(fillRoutine);
        }

        fillRoutine = StartCoroutine(Fill());
    }
    #endregion

    #region - 게이지 채우는 코루틴
    private IEnumerator Fill()
    {
        while (Mathf.Abs(scoreSlider.value - sildervalue) > 0.001f)
        {
            scoreSlider.value = Mathf.Lerp(scoreSlider.value, sildervalue, Time.deltaTime * 6f);
            yield return null;
        }

        scoreSlider.value = sildervalue;
    }

    #endregion
}
