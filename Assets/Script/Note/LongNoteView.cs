using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class LongNoteView : MonoBehaviour
{
    public enum LongVisualMode { ThreeParts, SingleSliced }

    [Header("노트 스프라이트")]
    public NoteSprite spriteSet;            

    [Header("롱노트 파트")]
    public RectTransform head;         
    public RectTransform body;              
    public RectTransform tail;            
    
    
    [Header("롱노트 출력 시, 몇 파츠로 나뉘어지는지")]
    public LongVisualMode mode = LongVisualMode.ThreeParts;

    [Header("시간 / 길이")]
    public float scrollSpeed = 300f;     
    public float startTimeSec; // 롱노트 시작 시간
    public float durationSec; // 롱노트 길이


    [Header("Judgex 판정선 기준 (가로 스크롤)")]
    public float judgex = -380f;   // 노트 생성 위치 , 일반 노트도 -380으로 설정
    public float laney = 0f;   //노트 생성/ 이동 시, y축 고정

    public float minBodyPx = 1f;             // Body 최소 길이
    public float minTotalPx = 4f;           // 전체 최소 길이

    // 캐시
    private Image _headImg, _bodyImg, _tailImg;
    private bool _isThreeParts;

    void Awake()
    {
        _headImg = head ? head.GetComponent<Image>() : null;
        _bodyImg = body ? body.GetComponent<Image>() : null;
        _tailImg = tail ? tail.GetComponent<Image>() : null;

        // 피벗  정렬
        if (head) head.pivot = new Vector2(0f, 0.5f);
        if (body) body.pivot = new Vector2(0f, 0.5f);
        if (tail) tail.pivot = new Vector2(0f, 0.5f);

        // 세개의 파츠로 출력
        _isThreeParts = (mode == LongVisualMode.ThreeParts);

        // 스프라이트 적용
        if (spriteSet != null && _bodyImg != null)
        {
            if (_isThreeParts)
            {
                if (_headImg) _headImg.sprite = spriteSet.GetLongHead();
                if (_bodyImg) _bodyImg.sprite = spriteSet.GetLongBody();
                if (_tailImg) _tailImg.sprite = spriteSet.GetLongTail();

                _bodyImg.type = Image.Type.Sliced;

                if (_headImg) _headImg.SetNativeSize();
                if (_tailImg) _tailImg.SetNativeSize();
            }
            else
            {
                if (_bodyImg) _bodyImg.sprite = spriteSet.GetLongBody();
                _bodyImg.type = Image.Type.Sliced;

                if (head) head.gameObject.SetActive(false);
                if (tail) tail.gameObject.SetActive(false);
            }
        }
    }


    #region - 가로 스크롤
    public void UpdateVisual(float nowSec)
    {
        if (body == null) return;

        //롱노트의 시작점 / 끝점 위치
        float xHead = judgex + (nowSec - startTimeSec) * scrollSpeed;
        float xTail = judgex + (nowSec - (startTimeSec + durationSec)) * scrollSpeed;

        // 롱노트 전체 길이(px)
        float width = Mathf.Max(minTotalPx, Mathf.Abs(xHead - xTail));


        // 가운데 위치
        float xCenter = (xHead + xTail) * 0.5f;

        // 전체 루트를 가운데 위치시키기
        var root = (RectTransform)transform;
        root.anchoredPosition = new Vector2(xCenter, laney);

        if (_isThreeParts)
        {
            float headW = (head != null) ? head.sizeDelta.x : 0f;
            float tailW = (tail != null) ? tail.sizeDelta.x : 0f;
            float bodyW = Mathf.Max(minBodyPx, width - headW - tailW);

            if (head)  //머리일 경우
            { head.anchoredPosition = new Vector2(-width * 0.5f, 0f); }


            if (body)  //몸일 경우
            {
                body.anchoredPosition = new Vector2(-width * 0.5f + headW, 0f);
                body.sizeDelta = new Vector2(bodyW, body.sizeDelta.y);
            }

            if (tail) //꼬리일 경우
            { tail.anchoredPosition = new Vector2(width * 0.5f - tailW, 0f); }
           
            else
            {
                // 단일 스프라이트 방식
                body.anchoredPosition = new Vector2(-width * 0.5f, 0f);
                body.sizeDelta = new Vector2(width, body.sizeDelta.y);
            }
        }
    }
    #endregion
}
