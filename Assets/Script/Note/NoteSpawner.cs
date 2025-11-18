using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NoteSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Conductor conductor;               // DSP 시간 기준
    public NoteSprite spriteSet;            // ScriptableObject(= NoteSprite)
    public RectTransform noteLayer;           // 노트들이 붙을 부모(Canvas 하위)

    [Header("Prefabs (UGUI)")]
    public RectTransform singleNotePrefab;    // Image 1장짜리(탭/슬라이드)
    public LongNoteView longNotePrefab;       // Head/Body/Tail 들어있는 프리팹

    [Header("Scroll")]
    public float scrollSpeedPx = 100f;        // px/sec (위에서 아래로)
    public float spawnLeadTimeSec = 2.0f;     // 몇 초 앞의 노트를 미리 생성
    public float despawnLagSec = 1.0f;        // 지나간 후 회수 지연

    [Header("Lanes (px)")]
    public float laneOriginPx = 0f;
    public float laneSpacingPx = 100f;
    public int defaultLane = 0;               // noteData.lane 미사용 시 기본

    [Header("Pooling")]
    public int initialSinglePool = 64;
    public int initialLongPool = 16;

    private NoteData[] _notes;
    private int _nextSpawn; // 다음에 스폰할 노트 인덱스

    private readonly List<ActiveItem> _active = new();
    private readonly Stack<RectTransform> _singlePool = new();
    private readonly Stack<LongNoteView> _longPool = new();

    struct ActiveItem
    {
        public NoteData data;
        public RectTransform rect;  // 싱글
        public LongNoteView longView; // 롱
        public bool isLong;
    }

    public void LoadChart(NoteData[] notes)
    {
        _notes = notes;
        _nextSpawn = 0;

        // 풀 초기화(최초 1회만 하고 싶다면 조건 분기)
        WarmupPools();
    }

    void WarmupPools()
    {
        // 싱글
        for (int i = _singlePool.Count; i < initialSinglePool; i++)
        {
            var inst = Instantiate(singleNotePrefab, noteLayer);
            inst.gameObject.SetActive(false);
            _singlePool.Push(inst);
        }
        // 롱
        for (int i = _longPool.Count; i < initialLongPool; i++)
        {
            var inst = Instantiate(longNotePrefab, noteLayer);
            inst.gameObject.SetActive(false);
            _longPool.Push(inst);
        }
    }

    float LaneToX(int laneIndex) => laneOriginPx + laneIndex * laneSpacingPx;


    void Update()
    {
        if (_notes == null || conductor == null) return;

        float now = conductor.NowSec;

        // 1) 스폰(리드타임)
        while (_nextSpawn < _notes.Length && (_notes[_nextSpawn].Timesec - now) <= spawnLeadTimeSec)
        {
            SpawnOne(_notes[_nextSpawn]);
            _nextSpawn++;
        }

        // 2) 위치 갱신 & 회수
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];

            if (item.isLong)
            {
                // 롱노트는 자체 UpdateVisual 사용
                item.longView.UpdateVisual(now);

                // 회수 시점: 롱노트 끝 + despawnLagSec
                float endTime = item.data.Timesec + item.data.durationSec;
                if (now > endTime + despawnLagSec)
                {
                    RecycleLong(item.longView);
                    _active.RemoveAt(i);
                }
            }
            else
            {
                // 싱글: y = (hitTime - now)*speed
                float y = (item.data.Timesec - now) * scrollSpeedPx;
                item.rect.anchoredPosition = new Vector2(item.rect.anchoredPosition.x, y);

                // 회수: 지나간 후 despawnLagSec
                if (now > item.data.Timesec + despawnLagSec)
                {
                    RecycleSingle(item.rect);
                    _active.RemoveAt(i);
                }
            }
        }
    }

    void SpawnOne(NoteData n)
    {
        bool isLong = (n.type == NoteType.LongNote);

        float x = 0f;

        if (isLong)
        {
            var view = GetLong();
            view.gameObject.SetActive(true);

            // 롱노트 파라미터 주입
            view.spriteSet = spriteSet;
            view.scrollSpeed = scrollSpeedPx;
            view.startTimeSec = n.Timesec;
            view.durationSec = n.durationSec;
            view.SetLaneX(x);

            // 첫 프레임 위치 보정
            view.UpdateVisual(conductor.NowSec);

            _active.Add(new ActiveItem
            {
                data = n,
                longView = view,
                rect = null,
                isLong = true
            });
        }
        else
        {
            var rect = GetSingle();
            rect.gameObject.SetActive(true);

            // 스프라이트 적용
            var img = rect.GetComponent<Image>();
            if (img != null && spriteSet != null)
            {
                img.sprite = spriteSet.GetSprite(n.type);
                // 필요시 고정 폭/높이로: img.SetNativeSize();
            }

            // X/Y 초기 배치
            float y = (n.Timesec - conductor.NowSec) * scrollSpeedPx;
            rect.anchoredPosition = new Vector2(x, y);

            _active.Add(new ActiveItem
            {
                data = n,
                rect = rect,
                longView = null,
                isLong = false
            });
        }
    }


    // ===== Pool =====

    RectTransform GetSingle()
    {
        if (_singlePool.Count > 0) return _singlePool.Pop();
        return Instantiate(singleNotePrefab, noteLayer);
    }
    void RecycleSingle(RectTransform rt)
    {
        rt.gameObject.SetActive(false);
        rt.SetParent(noteLayer, false);
        _singlePool.Push(rt);
    }

    LongNoteView GetLong()
    {
        if (_longPool.Count > 0) return _longPool.Pop();
        return Instantiate(longNotePrefab, noteLayer);
    }
    void RecycleLong(LongNoteView v)
    {
        v.gameObject.SetActive(false);
        v.transform.SetParent(noteLayer, false);
        _longPool.Push(v);
    }
}


