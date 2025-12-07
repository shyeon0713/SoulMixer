using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSpawner : MonoBehaviour
{
    [Header("참조 오브젝트")]
    public Conductor conductor;
    public NoteSprite spriteSet;
    public NoteSprite highlightSpriteSet;
    public RectTransform noteLayer;

    public FieldGrid[] grids;  // [0]=easy, [1]=normal, [2]=hard
    public FieldGrid grid;
    public Difficulty difficulty = Difficulty.Easy;

    [Header("프리팹")]
    public UINoteView notePrefab;

    [Header("경로생성 초")]
    public float pathPreviewTime = 2f;

    [Header("Pooling")]
    public int initialPoolSize = 64;

    private NoteData[] _notes;
    private int _nextSpawn;

    private List<ActiveItem> _active;
    private Stack<UINoteView> _pool;
    private List<PendingPreview> _pendingPreviews;

    private const int MAX_SPAWN_PER_FRAME = 3;
    private const int MAX_PREVIEW_BUFFER = 30;

    struct ActiveItem
    {
        public NoteData data;
        public UINoteView view;
        public Image highlight;
    }

    private class PendingPreview
    {
        public float showTime;
        public List<(int r, int c)> path;
        public bool shown;
    }

    void Awake()
    {
        Debug.Log("[NoteSpawner] Awake called");

        // 컬렉션 초기화 (Awake에서)
        _active = new List<ActiveItem>();
        _pool = new Stack<UINoteView>();
        _pendingPreviews = new List<PendingPreview>();

        if (conductor == null)
        {
            Debug.LogError("[NoteSpawner] Conductor is not assigned! DISABLING SPAWNER");
            enabled = false;
            return;
        }

        if (noteLayer == null)
        {
            Debug.LogError("[NoteSpawner] NoteLayer is not assigned! DISABLING SPAWNER");
            enabled = false;
            return;
        }

        if (noteLayer == null)
        {
            Debug.LogError("[NoteSpawner] NoteLayer is not assigned! DISABLING SPAWNER");
            enabled = false;
            return;
        }
        if (notePrefab == null)
        {
            Debug.LogError("[NoteSpawner] NotePrefab is not assigned! DISABLING SPAWNER");
            enabled = false;
            return;
        }

        Debug.Log("[NoteSpawner] All references OK");
    }

    public void LoadChart(NoteData[] notes)
    {
        if (notes == null || notes.Length == 0)
        {
            Debug.LogWarning("[NoteSpawner] LoadChart: notes is null or empty");
            return;
        }

        _notes = notes;
        _nextSpawn = 0;
        WarmupPool();

        Debug.Log($"[NoteSpawner] Chart loaded with {notes.Length} notes");
    }

    public void ResetSpawner()
    {
        _nextSpawn = 0;

        foreach (var n in _active)
        {
            if (n.view != null)
                Recycle(n.view);
        }

        _active.Clear();
        _pendingPreviews.Clear();

        if (grid != null)
            grid.ClearHighlights();
    }

    private void WarmupPool()
    {
        if (notePrefab == null || noteLayer == null)
        {
            Debug.LogWarning("[NoteSpawner] Prefab or noteLayer is null");
            return;
        }

        for (int i = _pool.Count; i < initialPoolSize; i++)
        {
            var inst = Instantiate(notePrefab, noteLayer);
            inst.gameObject.SetActive(false);
            _pool.Push(inst);
        }

        Debug.Log($"[NoteSpawner] Pool warmed up: {_pool.Count} notes");
    }

    private UINoteView GetNote()
    {
        if (_pool.Count > 0)
            return _pool.Pop();

        Debug.LogWarning("[NoteSpawner] Pool exhausted, creating new instance");
        return Instantiate(notePrefab, noteLayer);
    }

    private void Recycle(UINoteView view)
    {
        if (view == null) return;

        view.gameObject.SetActive(false);
        view.transform.SetParent(noteLayer, false);
        _pool.Push(view);
    }

    private void Update()
    {

        if (_notes == null || _notes.Length == 0)
        {
            Debug.LogWarning("[NoteSpawner] Update: _notes가 null이거나 비어있음");
            return;
        }

        if (conductor == null)
        {
            Debug.LogWarning("[NoteSpawner] Conductor is null in Update");
            return;
        }

        float now = conductor.NowSec;
  

        try
        {
            ProcessSpawn(now);
            ProcessPreviews(now);
            ProcessDespawn(now);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NoteSpawner] Update error: {ex.Message}\n{ex.StackTrace}");
            enabled = false;
        }
    }

    private void ProcessSpawn(float now)
    {
        if (_notes == null || _notes.Length == 0)
        {
            Debug.LogWarning("[NoteSpawner] ProcessSpawn: _notes가 null이거나 비어있음");
            return;
        }

        int spawnCount = 0;
        int safetyCounter = 0;
        const int MAX_SAFETY = 100;

        // 첫 노트 정보 로그
        if (_nextSpawn < _notes.Length)
        {
            var nextNote = _notes[_nextSpawn];
            float nextSpawnTime = Mathf.Max(0f, nextNote.Timesec - Mathf.Max(0.1f, nextNote.moveTime));
         //   Debug.Log($"[NoteSpawner] ProcessSpawn - now: {now:F3}, _nextSpawn: {_nextSpawn}, 다음 노트 spawnTime: {nextSpawnTime:F3}");
        }

        while (_nextSpawn < _notes.Length && safetyCounter < MAX_SAFETY)
        {
            safetyCounter++;

            if (spawnCount >= MAX_SPAWN_PER_FRAME)
                break;

            var note = _notes[_nextSpawn];

            if (note == null)
            {
                Debug.LogError($"[NoteSpawner] Note at index {_nextSpawn} is null");
                _nextSpawn++;
                continue;
            }

            float moveTime = Mathf.Max(0.1f, note.moveTime);
            float spawnTime = Mathf.Max(0f, note.Timesec - moveTime);

        //    Debug.Log($"[NoteSpawner] 노트 {note.id} 체크 - timeSec: {note.Timesec:F3}, moveTime: {moveTime:F3}, spawnTime: {spawnTime:F3}, now: {now:F3}, 차이: {(spawnTime - now):F3}");

            if (now < spawnTime)
                break;

            try
            {
                SpawnOne(note);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NoteSpawner] SpawnOne ERROR noteId={note.id}: {ex.Message}\n{ex.StackTrace}");
            }

            _nextSpawn++;
            spawnCount++;
        }

        if (safetyCounter >= MAX_SAFETY)
        {
            Debug.LogError("[NoteSpawner] Safety counter triggered! Possible infinite loop prevented.");
        }
    }

    private void ProcessPreviews(float now)
    {
        if (grid == null) return;

        for (int i = 0; i < _pendingPreviews.Count; i++)
        {
            var p = _pendingPreviews[i];
            if (p == null) continue;

            if (!p.shown && now >= p.showTime)
            {
                if (p.path != null && p.path.Count > 0)
                {
                    try
                    {
                        grid.HighlightPath(p.path);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[NoteSpawner] HighlightPath error: {ex.Message}");
                    }
                }

                p.shown = true;
            }
        }
    }

    private void ProcessDespawn(float now)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];

            float despawnTime = item.data.Timesec + item.data.judgeTime ;

          // 판정 시간 +여유 0.5초 지난 뒤에만 회수할려고 했는데 0.5f 텀이 너무 김

        if (now > despawnTime)
            {
                // 노트 반환
                Recycle(item.view);

                // 이 노트의 하이라이트만 반환
                if (item.highlight != null)
                    grid.ReleaseHighlight(item.highlight);

                _active.RemoveAt(i);
            }
        }
    }

    private void SpawnOne(NoteData n)
    {
        if (grid == null)
        {
            Debug.LogError("[NoteSpawner] Grid is null in SpawnOne");
            return;
        }

      //  Debug.Log($"[NoteSpawner] SpawnOne 시작 - noteId: {n.id}, key: {n.key}");

        // 1. 시작 위치 계산
        string edge = string.IsNullOrEmpty(n.spawnEdge) ? "top" : n.spawnEdge.ToLower();
        int index = Mathf.Max(0, n.spawnIndex);

        var (sr, sc) = grid.GetEdgeIndexByJson(edge, index);

        if (!grid.IsValidCell(sr, sc))
        {
            Debug.LogError($"[NoteSpawner] Invalid start cell ({sr},{sc}) for edge={edge}, index={index}");
            return;
        }

        
        //  랜덤 워크로 목표 셀까지 경로 생성
        int minSteps = Mathf.Max(1, n.minpath);
        int maxSteps = Mathf.Max(minSteps, n.maxpath);

        var path = grid.GenerateRandomPathToOppositeEdge(sr, sc, edge, minSteps, maxSteps);
        var (tr, tc) = path[path.Count - 1];

       // Debug.Log($"[NoteSpawner] RandomPath len={path.Count} start=({sr},{sc}) target=({tr},{tc})");

        // 3. 셀 → NoteLayer 좌표 변환 (지금 쓰는 방식 그대로)
        Vector2 posStart = grid.GetCellLocalPos(sr, sc);
        Vector2 posTarget = grid.GetCellLocalPos(tr, tc);

        var view = GetNote();
        if (view == null)
        {
            Debug.LogError("[NoteSpawner] Failed to get note view from pool");
            return;
        }

        view.gameObject.SetActive(true);
        float arriveSec = conductor.NowSec + n.moveTime;

        view.Init(
            noteLayer,
            posStart,
            posTarget,
            conductor,
            arriveSec
        );

        // 키에 따른 스프라이트 지정
        var img = view.GetComponentInChildren<Image>();
        Sprite noteSprite = null;

        if(img != null && spriteSet != null)
{
            noteSprite = spriteSet.GetSpriteByKeyString(n.key);   // ★ noteSprite에 저장
            if (noteSprite != null)
                img.sprite = noteSprite;
        }

        // 5. 목표 지점에 하이라이트 1개 생성
        Image highlightImg = grid.HighlightSingleCell(tr, tc);


        if (highlightImg != null)
        {
            Sprite hlSprite = null;

            if (highlightSpriteSet != null)
                hlSprite = highlightSpriteSet.GetSpriteByKeyString(n.key); // 전용 세트
            else
                hlSprite = noteSprite; // 없으면 노트 스프라이트 재사용

            if (hlSprite != null)
                highlightImg.sprite = hlSprite;
        }


        // 6. 활성 목록에 등록
        _active.Add(new ActiveItem
        {
            data = n,
            view = view,
            highlight = highlightImg
        });

        Debug.Log(
            $"[NoteSpawner] ✓ Spawn {n.id} from {edge}[{index}]({sr},{sc}) → target({tr},{tc})"
        );
    }

    public void HandleJudgeHit(NoteData note, HitGrade grade)
    {
        DespawnNoteById(note.id);
    }

    public void HandleJudgeMiss(NoteData note)
    {
        // note.id == -1 인 가짜 Miss(아무 노트 못 찾음)는 무시
        if (note.id < 0)
            return;

        DespawnNoteById(note.id);
    }

    private void DespawnNoteById(int noteId)
    {
        // _active 리스트에서 해당 노트를 찾아서 제거
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (_active[i].data.id == noteId)
            {
                // 노트 오브젝트 풀로 되돌리기
                Recycle(_active[i].view);
                _active.RemoveAt(i);
                break;
            }
        }
        // 하이라이트를 노트별로 관리하고 있지 않으면,
        // 일단 전체 Clear로 처리 (나중에 per-note 로 바꾸고 싶으면 구조 확장)
        if (grid != null)
            grid.ClearHighlights();
    }


}