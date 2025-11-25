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
    public float spawnLeadTimeSec = 2.0f;     // 몇 초 앞의 노트를 미리 생성
    public float despawnLagSec = 1.0f;        // 지나간 후 회수 지연

    [Header("Pooling")]
    public int initialSinglePool = 64;
    public int initialLongPool = 16;

    private NoteData[] _notes;
    private int _nextSpawn; // 다음에 스폰할 노트 인덱스

    private readonly List<ActiveItem> _active = new();
    private readonly Stack<RectTransform> _singlePool = new();
    private readonly Stack<LongNoteView> _longPool = new();

    [Header("노트 생성 위치")]
    public float spawnStartX = -380f; // 오른쪽 상단에 생성

    [Header("노트 판정")]
    public float judgex = 1000f;  //노트 판정 선
    public float scrollSpeedPx = 300f;  // 한 초에 몇 px 이동


    struct ActiveItem
    {
        public NoteData data;
        public RectTransform rect;  // 싱글
        public LongNoteView longView; // 롱
        public bool isLong;
    }

    #region Json파일 로드
    public void LoadChart(NoteData[] notes)
    {
        _notes = notes;
        _nextSpawn = 0;

        // 풀 초기화(최초 1회만 하고 싶다면 조건 분기)
        WarmupPools();

        // 디버깅
        Debug.Log($"[NoteSpawner] LoadChart: notes = {_notes.Length}");

    }
    #endregion

    #region - 풀링 
    void WarmupPools()
    {
        //노트 인식이 되는지 확인
        if (singleNotePrefab == null || noteLayer == null)
        {
            Debug.LogWarning("[NoteSpawner] WarmupPools: prefab 또는 noteLayer가 비어 있음");
            return;
        }

        //싱글 노트
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
        if (_notes == null) 
            return;

        //디버깅 확인
        if(conductor == null)
        {
            Debug.LogWarning("[NoteSpawner] Update: conductor가 비어 있어서 스폰 안 함");
            return;
        }
        if (noteLayer == null)
        {
            Debug.LogWarning("[NoteSpawner] Update: noteLayer가 비어 있어서 스폰 안 함");
            return;
        }
        if (singleNotePrefab == null)
        {
            Debug.LogWarning("[NoteSpawner] Update: singleNotePrefab이 비어 있어서 스폰 안 함");
            return;
        }
        //디버깅

        float now = conductor.NowSec;

        
        while (_nextSpawn < _notes.Length && (_notes[_nextSpawn].Timesec - now) <= spawnLeadTimeSec)
        {
            Debug.Log($"[NoteSpawner] 조건 만족 -> index={_nextSpawn}, hit={_notes[_nextSpawn].Timesec}, now={now}");
            SpawnOne(_notes[_nextSpawn]);
            _nextSpawn++;
        }


        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];

            if (item.isLong)  // 롱노트 부분
            {
               
                item.longView.UpdateVisual(now);

                
                float endTime = item.data.Timesec + item.data.durationSec;
                if (now > endTime + despawnLagSec)
                {
                    RecycleLong(item.longView);
                    _active.RemoveAt(i);
                }
            }
            else   //일반 노트 부분 
            {

                float progress = (now - item.data.Timesec) * scrollSpeedPx;
                float x = spawnStartX + progress; // 출발점 → 판정선 도착

                item.rect.anchoredPosition = new Vector2(x,item.rect.anchoredPosition.y);

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
        Debug.Log($"[NoteSpawner] SpawnOne id={n.id}, time={n.Timesec}, type={n.type}");

        bool isLong = (n.type == NoteType.LongNote);

   

        if (isLong)
        {
            var view = GetLong();
            view.gameObject.SetActive(true);

            
            view.spriteSet = spriteSet;
            view.scrollSpeed = scrollSpeedPx;
            view.startTimeSec = n.Timesec;
            view.durationSec = n.durationSec;

          
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
            rect.SetParent(noteLayer, false);

            // 스프라이트 적용
            var img = rect.GetComponentInChildren<Image>();
            if (img != null && spriteSet != null)
            {
                var sp = spriteSet.GetSprite(n.type);
                if (sp == null)
                    Debug.LogWarning($"[NoteSpawner] 스프라이트 없음: {n.type}");
                else
                    img.sprite = sp;
            }
          

            // 노트 생성 후
            float x = judgex + (conductor.NowSec - n.Timesec) * scrollSpeedPx;
            rect.anchoredPosition = new Vector2(x, 0f);

            _active.Add(new ActiveItem
            {
                data = n,
                rect = rect,
                longView = null,
                isLong = false
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


