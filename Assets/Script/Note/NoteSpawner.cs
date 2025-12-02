using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class NoteSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Conductor conductor;
    public NoteSprite spriteSet;
    public FieldGrid grid;
    public RectTransform noteLayer;   //그리드 보드와 같은 오브젝트

    [Header("Prefabs (UGUI)")]
    public UINoteView notePrefab;


    [Header("Time")]
    public float spawnLeadTimeSec = 1.0f;  // 판정  몇 초 전에 스폰할지
    public float despawnLagSec = 1.0f;  // 판정 후 몇 초 뒤에 회수할지

    [Header("Pooling")]
    public int initialPoolSize = 64;

    private NoteData[] _notes;
    private int _nextSpawn;

    private readonly List<ActiveItem> _active = new();
    private readonly Stack<UINoteView> _pool = new();

    struct ActiveItem
    {
        public NoteData data;
        public UINoteView view;

    }

    #region Json파일 로드 / 리셋
    public void LoadChart(NoteData[] notes)
    {
        _notes = notes;
        _nextSpawn = 0;

        WarmupPool();

        //  Debug.Log($"[NoteSpawner] LoadChart: {_notes?.Length ?? 0} notes loaded");
    }

    // 스폰 상태 리셋 (곡 재시작 시)
    public void ResetSpawner()
    {
        _nextSpawn = 0;

        for (int i = _active.Count - 1; i >= 0; i--)
        {
            Recycle(_active[i].view);
        }
        _active.Clear();

        Debug.Log("[NoteSpawner] Spawner reset");
    }

    #endregion

 

    #region - 풀링 
    void WarmupPool()
    {
        if (notePrefab == null || noteLayer == null)
        {
            Debug.LogWarning("[NoteSpawner] WarmupPool: prefab 또는 noteLayer가 비어 있음");
            return;
        }

        for (int i = _pool.Count; i < initialPoolSize; i++)
        {
            var inst = Instantiate(notePrefab, noteLayer);
            inst.gameObject.SetActive(false);
            _pool.Push(inst);
        }
    }

    UINoteView GetNote()
    {
        if (_pool.Count > 0)
            return _pool.Pop();

        return Instantiate(notePrefab, noteLayer);
    }

    void Recycle(UINoteView view)
    {
        if (view == null) return;
        view.gameObject.SetActive(false);
        view.transform.SetParent(noteLayer, false);
        _pool.Push(view);
    }
    #endregion



    void Update()
    {
        if (_notes == null || conductor == null || noteLayer == null || grid == null)
            return;

        float now = conductor.NowSec;

        // 스폰 조건: 판정 시간까지 남은 시간이 spawnLeadTimeSec 이하
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
                break;
            }
        }

        // Despawn: 판정 시간 + 여유 시간 지났으면 회수
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];

            if (now > item.data.Timesec + despawnLagSec)
            {
                Recycle(item.view);
                _active.RemoveAt(i);
            }

        }

    }


    #region - 노트 생성
    void SpawnOne(NoteData n)
    {
        var view = GetNote();
        view.gameObject.SetActive(true);

        // 1) 그리드에서 시작/목표 셀 인덱스 뽑기
        var (sr, sc) = grid.GetRandomEdgeIndex();
        var (tr, tc) = grid.GetRandomInnerIndex();

        Vector2 startLocal = grid.GetCellLocalPos(sr, sc);
        Vector2 targetLocal = grid.GetCellLocalPos(tr, tc);

        // 2) UI 노트 초기화 (이동/시간은 UINoteView가 처리)
        view.Init(noteLayer, startLocal, targetLocal, conductor, n.Timesec);

        // 3) 스프라이트 적용 (기존 spriteSet 그대로 사용)
        var img = view.GetComponentInChildren<Image>();
        if (img != null && spriteSet != null)
        {
            var sp = spriteSet.GetSpriteByKeyString(n.key); // 수정 - 노트가 아닌 NoteData.key기반으로 스프라이트를 가져와야하므로 수정
            if (sp != null)
                img.sprite = sp;
        }

        // 디버그 로그
        Debug.Log($"[NoteSpawner] Spawn Note -> id={n.id}, type={n.type}, timeSec={n.Timesec:F3}");

        // 4) 활성 리스트에 등록 (Despwn 관리)
        _active.Add(new ActiveItem
        {
            data = n,
            view = view
        });
    }
    #endregion

}