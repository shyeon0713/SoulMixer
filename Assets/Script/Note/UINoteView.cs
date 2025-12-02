using UnityEngine;

public class UINoteView : MonoBehaviour
{
    public RectTransform rect;   // 자기 자신의 RectTransform
    public Conductor conductor;

    private Vector2 _start;
    private Vector2 _target;
    private float _startSec;
    private float _hitSec;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
    }

    #region -  UI 노트 초기화, 스폰 시점에 한 번만 호출
    public void Init(RectTransform parent, Vector2 startLocalPos, Vector2 targetLocalPos,
                     Conductor c, float hitSec)
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.anchoredPosition = startLocalPos;

        _start = startLocalPos;
        _target = targetLocalPos;

        conductor = c;
        _hitSec = hitSec;
        _startSec = c.NowSec;   // 지금 시점부터 hitSec까지 이동
    }
    #endregion
    private void Update()
    {
        if (conductor == null) return;

        float now = conductor.NowSec;

        // 진행 비율 계산 (0~1)
        float t = Mathf.InverseLerp(_startSec, _hitSec, now);
        t = Mathf.Clamp01(t);

        rect.anchoredPosition = Vector2.Lerp(_start, _target, t);
    }
}

