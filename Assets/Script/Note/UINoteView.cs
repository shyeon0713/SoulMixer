using UnityEngine;

public class UINoteView : MonoBehaviour
{
    public RectTransform rect;   // 자기 자신의 RectTransform

    public void Init(RectTransform board, Vector2 startLocalPos, Vector2 targetLocalPos)
    {
        rect.SetParent(board, worldPositionStays: false);
        rect.anchoredPosition = startLocalPos;
        _start = startLocalPos;
        _target = targetLocalPos;
    }

    Vector2 _start, _target;
    float _startSec, _hitSec;
    public Conductor conductor;
    public float appearLeadSec = 1f;

    public void SetTiming(float hitSec)
    {
        _hitSec = hitSec;
        _startSec = hitSec - appearLeadSec;
    }

    void Update()
    {
        float now = conductor.NowSec;

        if (now <= _startSec)
        {
            rect.anchoredPosition = _start;
            return;
        }

        float t = Mathf.InverseLerp(_startSec, _hitSec, now);
        t = Mathf.Clamp01(t);

        rect.anchoredPosition = Vector2.Lerp(_start, _target, t);
    }
}
