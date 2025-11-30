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
    public float scrollSpeed = 400f;
    public float startTimeSec;  // 롱노트 시작 시간
    public float durationSec;   // 롱노트 지속 시간
    public float spawnTimeSec;  // ?? 추가: 생성된 시간

    [Header("위치 설정")]
    public float spawnStartX = -734f;  // 생성 위치
    public float judgeLineX = 725f;    // 판정선 위치
    public float laneY = 382f;         // Y 위치 (고정)

    public float minBodyPx = 1f;
    public float minTotalPx = 4f;

    // 캐시
    private Image _headImg, _bodyImg, _tailImg;
    private bool _isThreeParts;

    void Awake()
    {
        _headImg = head ? head.GetComponent<Image>() : null;
        _bodyImg = body ? body.GetComponent<Image>() : null;
        _tailImg = tail ? tail.GetComponent<Image>() : null;

        // 피벗 정렬
        if (head) head.pivot = new Vector2(0f, 0.5f);
        if (body) body.pivot = new Vector2(0f, 0.5f);
        if (tail) tail.pivot = new Vector2(0f, 0.5f);

        _isThreeParts = (mode == LongVisualMode.ThreeParts);

        // 스프라이트 적용
        if (spriteSet != null)
        {
            if (_isThreeParts)
            {
                if (_headImg) _headImg.sprite = spriteSet.GetLongHead();
                if (_bodyImg) _bodyImg.sprite = spriteSet.GetLongBody();
                if (_tailImg) _tailImg.sprite = spriteSet.GetLongTail();

                if (_bodyImg) _bodyImg.type = Image.Type.Sliced;

                if (_headImg) _headImg.SetNativeSize();
                if (_tailImg) _tailImg.SetNativeSize();
            }
            else
            {
                if (_bodyImg) _bodyImg.sprite = spriteSet.GetLongBody();
                if (_bodyImg) _bodyImg.type = Image.Type.Sliced;

                if (head) head.gameObject.SetActive(false);
                if (tail) tail.gameObject.SetActive(false);
            }
        }
    }

    #region - 가로 스크롤 (수정됨)
    public void UpdateVisual(float nowSec)
    {
        if (body == null) return;

        // ?? 수정: 생성 시점부터 경과한 시간 기준으로 계산
        float elapsedTime = nowSec - spawnTimeSec;

        // Head 위치: 생성 위치에서 시작해서 이동
        float xHead = spawnStartX + (elapsedTime * scrollSpeed);

        // Tail 위치: Head보다 durationSec만큼 뒤에서 시작
        // Tail이 생성되는 시점 = startTimeSec (Head 판정 시간)
        // Tail이 이동을 시작하는 시점 = nowSec가 startTimeSec에 도달한 후
        float tailElapsedTime = Mathf.Max(0, nowSec - startTimeSec);
        float xTail = spawnStartX + (tailElapsedTime * scrollSpeed);

        // 롱노트 전체 길이(px)
        float width = Mathf.Max(minTotalPx, Mathf.Abs(xHead - xTail));

        // 가운데 위치
        float xCenter = (xHead + xTail) * 0.5f;

        // 전체 루트를 가운데 위치시키기
        var root = (RectTransform)transform;
        root.anchoredPosition = new Vector2(xCenter, laneY);

        if (_isThreeParts)
        {
            float headW = (head != null) ? head.sizeDelta.x : 0f;
            float tailW = (tail != null) ? tail.sizeDelta.x : 0f;
            float bodyW = Mathf.Max(minBodyPx, width - headW - tailW);

            if (head)
            {
                head.anchoredPosition = new Vector2(-width * 0.5f, 0f);
            }

            if (body)
            {
                body.anchoredPosition = new Vector2(-width * 0.5f + headW, 0f);
                body.sizeDelta = new Vector2(bodyW, body.sizeDelta.y);
            }

            if (tail)
            {
                tail.anchoredPosition = new Vector2(width * 0.5f - tailW, 0f);
            }
        }
        else
        {
            // 단일 스프라이트 방식
            if (body)
            {
                body.anchoredPosition = new Vector2(-width * 0.5f, 0f);
                body.sizeDelta = new Vector2(width, body.sizeDelta.y);
            }
        }
    }
    #endregion
}