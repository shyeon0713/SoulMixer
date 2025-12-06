using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class NoteSpawner : MonoBehaviour
{
    [Header("참조 오브젝트")]
    public Conductor conductor;
    public NoteSprite spriteSet;
    public FieldGrid grid;
    public RectTransform noteLayer;   //그리드 보드와 같은 오브젝트

    [Header("프리팹")]
    public UINoteView notePrefab;
    [Header("경로 하이라이트 프리팹 이미지")]
    public float pathPreviewTime = 2f;

    [Header("Pooling")]
    public int initialPoolSize = 64;

    private NoteData[] _notes;
    private int _nextSpawn;

    private readonly List<ActiveItem> _active = new();
    private readonly Stack<UINoteView> _pool = new();
    private readonly List<PendingPreview> _pendingPreviews = new();

    #region - 내부 구조 정의
    struct ActiveItem
    {
        public NoteData data;
        public UINoteView view;

    }
    private class PendingPreview    // 노트가 이동할 경로를 2초전에 화면에 출력
    {
        public float showTime;
        public List<(int r, int c)> path;
        public bool shown;
    }

    #endregion 


    #region Json파일 로드 / 리셋
    public void LoadChart(NoteData[] notes)
    {
        _notes = notes;
        _nextSpawn = 0;

        WarmupPool();

        //  Debug.Log($"[NoteSpawner] LoadChart: {_notes?.Length ?? 0} notes loaded");
    }

    public void ResetSpawner()
    {
        _nextSpawn = 0;

        foreach (var n in _active)
            Recycle(n.view);

        _active.Clear();
        _pendingPreviews.Clear();
        grid.ClearHighlights();
    }

    #endregion

    #region - 풀링 
    void WarmupPool()
    {
        for (int i = _pool.Count; i < initialPoolSize; i++)
        {
            var inst = Instantiate(notePrefab, noteLayer);
            inst.gameObject.SetActive(false);
            _pool.Push(inst);
        }
    }

    private UINoteView GetNote()
    {
        if (_pool.Count > 0)
            return _pool.Pop();

        return Instantiate(notePrefab, noteLayer);
    }


    // 노트 재활용
    private void Recycle(UINoteView view)
    {
        view.gameObject.SetActive(false);
        view.transform.SetParent(noteLayer, false);
        _pool.Push(view);
    }

    #endregion


    private void Update()
    {
        if (_notes == null || _notes.Length == 0)
            return;

        float now = conductor.NowSec;

        ProcessSpawn(now);
        ProcessPreviews(now);
        ProcessDespawn(now);
    }

    #region - 노트 스폰 및 하이라이트 생성 + 제거
    // 노트 스폰
    private void ProcessSpawn(float now)
    {

        int safety = 0;

        while (_nextSpawn < _notes.Length)
        {
            if (safety++ > 2000)
            {
                Debug.LogError("Spawn Loop Safety Break!");
                break;
            }

            var note = _notes[_nextSpawn];
            // 스폰타임 = 판정시간 - 이동시간 + 미리보기 시간
            float spawnTime = note.Timesec - note.moveTime;

            if (now >= spawnTime)
            {
                SpawnOne(note);
                _nextSpawn++;
            }
            else break;
        }
    }

    // 경로 하이라이트 출력
    private void ProcessPreviews(float now)
    {
        foreach (var preview in _pendingPreviews)
        {
            if (!preview.shown && now >= preview.showTime)
            {
                grid.HighlightPath(preview.path);
                preview.shown = true;
            }
        }
    }


    // 노트 및 경로 하이라이트 제거
    private void ProcessDespawn(float now)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var item = _active[i];

            if (now > item.data.Timesec + item.data.judgeTime + 1.0f)   // 판정 후 제거
            {
                Recycle(item.view);
                _active.RemoveAt(i);

                // 경로 하이라이트도 지워준다
                grid.ClearHighlights();
            }
        }
    }

    #endregion

    #region - 노트 스폰 처리
    void SpawnOne(NoteData n)
    {
        // 시작 칸 랜덤 선택
        string edge = GetRandomEdge();
        int index = GetRandomEdgeIndex(edge);
        var (sr, sc) = grid.GetEdgeIndexByJson(edge, index);

        // 랜덤 워크 활용 -> 경로 생성 
        int pathLength = Random.Range(n.minpath, n.maxpath + 1);
        var path = grid.GenerateRandomWalk(sr, sc, pathLength);

        // 목적지
        var (tr, tc) = path[path.Count - 1];

        // 실제 스폰 + 노트 이동 설정 
        Vector2 posStart = grid.GetCellLocalPos(sr, sc);
        Vector2 posTarget = grid.GetCellLocalPos(tr, tc);

        var view = GetNote();
        view.gameObject.SetActive(true);

        view.Init(
           noteLayer,
           posStart,
           posTarget,
           conductor,
           n.Timesec
       );

        // 스프라이트 적용
        var img = view.GetComponentInChildren<Image>();
        if (img != null)
        {
            Sprite sp = spriteSet.GetSpriteByKeyString(n.key);
            img.sprite = sp;
        }


        // 경로 하이라이트
        _pendingPreviews.Add(new PendingPreview
        {
            showTime = n.Timesec - n.moveTime - pathPreviewTime,
            path = path,
            shown = false
        });

        // 활성 리스트 등록
        _active.Add(new ActiveItem
        {
            data = n,
            view = view
        });

        // 
        Debug.Log($"[NoteSpawner] Spawn Note {n.id} edge={edge}, steps={pathLength}");
    }
    #endregion


    #region -  랜덤 엣지(가장자리 좌표)설정

    private string GetRandomEdge()
    {
        int v = Random.Range(0, 4);
        return v switch
        {
            0 => "top",
            1 => "bottom",
            2 => "left",
            _ => "right",
        };
    }

    private int GetRandomEdgeIndex(string edge)
    {
        return edge switch
        {
            "top" => Random.Range(0, grid.cols),
            "bottom" => Random.Range(0, grid.cols),
            "left" => Random.Range(0, grid.rows),
            "right" => Random.Range(0, grid.rows),
            _ => 0
        };
    }
    #endregion

}