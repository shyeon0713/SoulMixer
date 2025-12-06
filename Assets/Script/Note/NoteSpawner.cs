using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSpawner : MonoBehaviour
{
    [Header("참조 오브젝트")]
    public Conductor conductor;
    public NoteSprite spriteSet;
    public FieldGrid grid;
    public RectTransform noteLayer;

    [Header("프리팹")]
    public UINoteView notePrefab;

    [Header("프리뷰")]
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
        if (grid == null)
        {
            Debug.LogError("[NoteSpawner] FieldGrid is not assigned! DISABLING SPAWNER");
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
            return;

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
        int spawnCount = 0;
        int safetyCounter = 0;
        const int MAX_SAFETY = 100;

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

            float despawnTime = item.data.Timesec + item.data.judgeTime + 0.5f;

            if (now > despawnTime)
            {
                Recycle(item.view);
                _active.RemoveAt(i);

                if (grid != null)
                    grid.ClearHighlights();
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

        // 1. 시작 위치 계산
        string edge = string.IsNullOrEmpty(n.spawnEdge) ? "top" : n.spawnEdge.ToLower();
        int index = Mathf.Max(0, n.spawnIndex);

        var (sr, sc) = grid.GetEdgeIndexByJson(edge, index);

        if (!grid.IsValidCell(sr, sc))
        {
            Debug.LogError($"[NoteSpawner] Invalid start cell ({sr},{sc}) for edge={edge}, index={index}");
            return;
        }

        // 2. 목표 위치 계산 (반대편 edge의 같은 index)
        string targetEdge = grid.GetOppositeEdge(edge);
        var (tr, tc) = grid.GetEdgeIndexByJson(targetEdge, index);

        if (!grid.IsValidCell(tr, tc))
        {
            Debug.LogWarning($"[NoteSpawner] Invalid target cell ({tr},{tc}), using start position");
            tr = sr;
            tc = sc;
        }

        Vector2 posStart = grid.GetCellLocalPos(sr, sc);
        Vector2 posTarget = grid.GetCellLocalPos(tr, tc);

        var view = GetNote();
        if (view == null)
        {
            Debug.LogError("[NoteSpawner] Failed to get note view from pool");
            return;
        }

        view.gameObject.SetActive(true);

        try
        {
            view.Init(
                noteLayer,
                posStart,
                posTarget,
                conductor,
                n.Timesec
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NoteSpawner] view.Init error: {ex.Message}");
            Recycle(view);
            return;
        }

        var img = view.GetComponentInChildren<Image>();
        if (img != null && spriteSet != null)
        {
            var sp = spriteSet.GetSpriteByKeyString(n.key);
            if (sp != null)
            {
                img.sprite = sp;
            }
            else
            {
                Debug.LogWarning($"[NoteSpawner] Sprite not found for key: {n.key}");
            }
        }

        // 직선 경로 (시작점 → 목표점)
        var path = new List<(int, int)>
        {
            (sr, sc),
            (tr, tc)
        };

        float previewShowTime = Mathf.Max(0f, n.Timesec - n.moveTime - pathPreviewTime);

        if (_pendingPreviews.Count > MAX_PREVIEW_BUFFER)
            _pendingPreviews.RemoveAt(0);

        _pendingPreviews.Add(new PendingPreview
        {
            showTime = previewShowTime,
            path = path,
            shown = false
        });

        _active.Add(new ActiveItem
        {
            data = n,
            view = view
        });

        Debug.Log($"[NoteSpawner] ✓ Spawn {n.id} from {edge}[{index}]({sr},{sc}) → {targetEdge}[{index}]({tr},{tc})");
    }
}