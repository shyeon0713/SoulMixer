using UnityEngine;
using UnityEngine.UI;

public class LongNoteView : MonoBehaviour
{
    public enum LongVisualMode { ThreeParts, SingleSliced }

    [Header("Sprites (ScriptableObject)")]
    public NoteSprite spriteSet;             // 네가 만든 SO (NoteSprite)

    [Header("UGUI Parts")]
    public RectTransform head;               // 자식: RectTransform + Image
    public RectTransform body;               // 자식: RectTransform + Image (Type=Sliced)
    public RectTransform tail;               // 자식: RectTransform + Image

    [Header("Mode")]
    public LongVisualMode mode = LongVisualMode.ThreeParts;

    [Header("Timing / Scroll")]
    public float scrollSpeed = 600f;         // px/sec (Canvas 기준)
    public float startTimeSec;               // 롱노트 시작 시간(초)
    public float durationSec;                // 롱노트 길이(초)

    [Header("Lane X (px)")]
    public float laneX = 0f;
    public void SetLaneX(float x) => laneX = x;

    [Header("Min px")]
    public float minBodyPx = 4f;             // Body 최소 높이
    public float minTotalPx = 12f;           // 전체 최소 높이

    // 캐시
    private Image _headImg, _bodyImg, _tailImg;
    private bool _isThreeParts;

    void Awake()
    {
        // 컴포넌트 캐시
        _headImg = head ? head.GetComponent<Image>() : null;
        _bodyImg = body ? body.GetComponent<Image>() : null;
        _tailImg = tail ? tail.GetComponent<Image>() : null;

        // 피벗 아래(0) 정렬
        if (head) head.pivot = new Vector2(0.5f, 0f);
        if (body) body.pivot = new Vector2(0.5f, 0f);
        if (tail) tail.pivot = new Vector2(0.5f, 0f);

        // 모드 결정
        _isThreeParts = (mode == LongVisualMode.ThreeParts);

        // 스프라이트 적용
        if (spriteSet != null && _bodyImg != null)
        {
            if (_isThreeParts)
            {
                if (_headImg) _headImg.sprite = spriteSet.GetLongHead();
                _bodyImg.sprite = spriteSet.GetLongBody();
                if (_tailImg) _tailImg.sprite = spriteSet.GetLongTail();

                _bodyImg.type = Image.Type.Sliced;

                if (_headImg && _headImg.sprite) _headImg.SetNativeSize();
                if (_tailImg && _tailImg.sprite) _tailImg.SetNativeSize();

                if (head) head.gameObject.SetActive(true);
                if (tail) tail.gameObject.SetActive(true);
            }
            else
            {
                // 단일 스프라이트 1장 + 9-slice
                // 기본은 GetLongBody()를 사용. (너에게 GetLongSingle()가 있다면 아래 한 줄을 바꿔줘)
                _bodyImg.sprite = spriteSet.GetLongBody();
                // _bodyImg.sprite = spriteSet.GetLongSingle(); // ← 너가 구현했다면 이렇게 사용
                _bodyImg.type = Image.Type.Sliced;

                if (head) head.gameObject.SetActive(false);
                if (tail) tail.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 현재 곡 시간(nowSec)에 맞춰 위치/길이를 갱신 (NoteSpawner가 프레임마다 호출)
    /// </summary>
    public void UpdateVisual(float nowSec)
    {
        if (body == null) return;

        // 롱노트 상단 Y와 전체 높이(px)
        float yTop = (startTimeSec - nowSec) * scrollSpeed;
        float height = Mathf.Max(minTotalPx, durationSec * scrollSpeed);

        if (_isThreeParts)
        {
            float headH = (head != null) ? head.sizeDelta.y : 0f; // SetNativeSize 적용 값
            float tailH = (tail != null) ? tail.sizeDelta.y : 0f;
            float bodyH = Mathf.Max(minBodyPx, height - headH - tailH);

            if (head) head.anchoredPosition = new Vector2(laneX, yTop);
            body.anchoredPosition = new Vector2(laneX, yTop - headH);
            if (tail) tail.anchoredPosition = new Vector2(laneX, yTop - height);

            body.sizeDelta = new Vector2(body.sizeDelta.x, bodyH);
        }
        else
        {
            // 단일 스프라이트 1장 (전체 길이 = height)
            body.anchoredPosition = new Vector2(laneX, yTop);
            body.sizeDelta = new Vector2(body.sizeDelta.x, height);
        }
    }
}
