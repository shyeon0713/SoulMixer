using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Conductor conductor;
    public NoteSprite spriteSet;
    public RectTransform noteLayer;

    [Header("Prefabs (UGUI)")]
    public RectTransform singleNotePrefab;


    [Header("Scroll")]
    public float spawnLeadTimeSec = 2.0f;
    public float despawnLagSec = 1.0f;

    [Header("Pooling")]
    public int initialSinglePool = 64;
    public int initialLongPool = 16;

    private NoteData[] _notes;
    private int _nextSpawn;

    private readonly List<ActiveItem> _active = new();
    private readonly Stack<RectTransform> _singlePool = new();
    private readonly Stack<LongNoteView> _longPool = new();

    [Header("노트 위치 설정")]
    public float judgeLineX = 725f;  // 판정선 위치 
    public float spawnStartX = -734f;  // 노트 생성 위치 
    public float noteY = 382f;  // 노트 Y 위치
    public float scrollSpeedPx = 400f;  // 왼쪽에서 오른쪽 이동

    struct ActiveItem
    {
        public NoteData data;
        public RectTransform rect;
        public LongNoteView longView;
        public bool isLong;
        public float spawnTime;  // 노트가 생성된 시간
    }

    #region Json파일 로드
    public void LoadChart(NoteData[] notes)
    {
        _notes = notes;
        _nextSpawn = 0;

        WarmupPools();

      //  Debug.Log($"[NoteSpawner] LoadChart: {_notes?.Length ?? 0} notes loaded");
    }

    // 스폰 상태 리셋 (곡 재시작 시)
    public void ResetSpawner()
    {
        _nextSpawn = 0;

        // 활성 노트 모두 회수
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];
            if (item.isLong)
                RecycleLong(item.longView);
            else
                RecycleSingle(item.rect);
        }
        _active.Clear();

        Debug.Log("[NoteSpawner] Spawner reset");
    }
    #endregion

    #region - 풀링 
    void WarmupPools()
    {
        if (singleNotePrefab == null || noteLayer == null)
        {
            Debug.LogWarning("[NoteSpawner] WarmupPools: prefab 또는 noteLayer가 비어 있음");
            return;
        }

        // 싱글 노트
        for (int i = _singlePool.Count; i < initialSinglePool; i++)
        {
            var inst = Instantiate(singleNotePrefab, noteLayer);
            inst.gameObject.SetActive(false);
            _singlePool.Push(inst);
        }

        // 롱 노트
        if (longNotePrefab != null)
        {
            for (int i = _longPool.Count; i < initialLongPool; i++)
            {
                var inst = Instantiate(longNotePrefab, noteLayer);
                inst.gameObject.SetActive(false);
                _longPool.Push(inst);
            }
        }
    }
    #endregion

    void Update()
    {
        if (_notes == null || conductor == null || noteLayer == null)
            return;

        float now = conductor.NowSec;

        // 판정 시간까지 남은 시간이 spawnLeadTimeSec 이하
        while (_nextSpawn < _notes.Length)
        {
            float timeUntilHit = _notes[_nextSpawn].Timesec - now;

            if (timeUntilHit <= spawnLeadTimeSec)
            {

                SpawnOne(_notes[_nextSpawn]);
                _nextSpawn++;
            }
            else
            {
                break; // 아직 스폰할 시간이 아님
            }
        }

        // 노트 업데이트
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];

            if (item.isLong)
            {
                item.longView.UpdateVisual(now);

                float endTime = item.data.Timesec + item.data.durationSec;
                if (now > endTime + despawnLagSec)
                {
                    RecycleLong(item.longView);
                    _active.RemoveAt(i);
                }
            }
            else
            {
                //  생성 시점부터 경과한 시간 기준으로 이동
                float elapsedTime = now - item.spawnTime;
                float x = spawnStartX + (elapsedTime * scrollSpeedPx);

                item.rect.anchoredPosition = new Vector2(x, item.rect.anchoredPosition.y);

                // 판정선을 지나고 일정 시간 후 회수
                if (now > item.data.Timesec + despawnLagSec)
                {
                    RecycleSingle(item.rect);
                    _active.RemoveAt(i);
                }
            }
        }
    }

    #region - 노트 생성
    void SpawnOne(NoteData n)
    {
        bool isLong = (n.type == NoteType.LongNote);

        if (isLong)
        {
            var view = GetLong();
            view.gameObject.SetActive(true);

            view.spriteSet = spriteSet;
            view.scrollSpeed = scrollSpeedPx;

            view.startTimeSec = n.Timesec;
            view.durationSec = n.durationSec;

            view.spawnTimeSec = conductor.NowSec;

            view.UpdateVisual(conductor.NowSec);

            _active.Add(new ActiveItem
            {
                data = n,
                longView = view,
                rect = null,
                isLong = true,
                spawnTime = conductor.NowSec  
            });
        }
        else
        {
            var rect = GetSingle();
            rect.gameObject.SetActive(true);
            rect.SetParent(noteLayer, false);

            // 스프라이트 적용
            var img = rect.GetComponentInChildren<Image>();
            if (img != null && spriteSet != null)
            {
                var sp = spriteSet.GetSprite(n.type);
                if (sp == null) { }
                 
                else
                    img.sprite = sp;
            }

            // 노트는 왼쪽 spawnStartX에서 생성, Y는 고정
            float x = spawnStartX;
            float y = noteY;  // 고정된 Y 위치

            rect.anchoredPosition = new Vector2(x, y);
            

            //노트 디버깅
            var d = _notes[_nextSpawn];
            Debug.Log($"[NoteSpawner] Spawn Note -> id={d.id}, type={d.type}, timeSec={d.Timesec:F3}");

            _active.Add(new ActiveItem
            {
                data = n,
                rect = rect,
                longView = null,
                isLong = false,
                spawnTime = conductor.NowSec  
            });
        }
    }
    #endregion

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
        if (longNotePrefab == null) return null;
        if (_longPool.Count > 0) return _longPool.Pop();
        return Instantiate(longNotePrefab, noteLayer);
    }

    void RecycleLong(LongNoteView v)
    {
        if (v == null) return;
        v.gameObject.SetActive(false);
        v.transform.SetParent(noteLayer, false);
        _longPool.Push(v);
    }
}