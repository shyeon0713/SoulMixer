using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameEntry : MonoBehaviour
{
    [Header("Refs - UGUI 스포너")]
    public Conductor conductor;
    public Judge judge;
    public NoteSpawner noteSpawner;     // UGUI 스포너
    public AudioSource audioSource;
    public GridManager gridManager; // 그리드 활성화 부분

    [Header("악곡/난이도")]
    public SongEntry selectedSongEntry;
    public Difficulty selectedDifficulty; // 난이도


    public GameObject resultUI;  // 결과창 UI 띄우기
    public GameObject PlayUI;

    private Coroutine _resultRoutine; // 5초뒤에 결과창 UI로 이동하기 위해 코루틴 생성
    public string afterGameDialogueFile;

    // 테스트용: 씬 켜지면 자동 재생
    void Awake()
    {
        Debug.Log("[GameEntry] Start 호출");

        // 다른 스크립트들의 Awake가 끝날 때까지 대기
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        // 1프레임 대기 (모든 Awake 완료 보장)
        yield return null;

        Debug.Log("[GameEntry] 초기화 시작");

        if (gridManager == null)
        {
            Debug.LogError("[GameEntry] GridManager가 할당되지 않았습니다!");
            yield break;
        }

        if (noteSpawner == null)
        {
            Debug.LogError("[GameEntry] NoteSpawner가 할당되지 않았습니다!");
            yield break;
        }

        gridManager.SetGridDiff(selectedDifficulty);

        noteSpawner.difficulty = selectedDifficulty;
        noteSpawner.grid = gridManager.GetCurrentGrid();

        InitAndPlay();
    }
    #region - 버튼에서 호출할 초기화 + 재생
    public void InitAndPlay()
    {
        Debug.Log("[GameEntry] InitAndPlay 시작");

        if (selectedSongEntry == null)
        {
            Debug.LogError("[GameEntry] 선택된 곡이 없습니다!");
            return;
        }

        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // 오디오 연결
        audioSource.clip = selectedSongEntry.audioClip;
       
        // 차트 로드
        var notes = ChartLoader.LoadFromSongEntry(selectedSongEntry, selectedDifficulty);

        Debug.Log($"[GameEntry] 차트 로드 결과 - 노트 개수: {notes?.Length ?? 0}");

        if (notes.Length == 0)
        {
            Debug.LogError("[GameEntry] 차트를 찾지 못했습니다.");
            return;
        }

        Debug.Log("[GameEntry] Judge.LoadChart 호출");
        //판정/ 스폰 세팅
        judge.LoadChart(notes);

        Debug.Log("[GameEntry] NoteSpawner.LoadChart 호출");
        noteSpawner.LoadChart(notes);

        judge.OnAllNotesJudged -= HandleAllnotesJudged;
        judge.OnAllNotesJudged += HandleAllnotesJudged;

        // 오프셋 적용
        var chart = selectedSongEntry.GetChart(selectedDifficulty);
        //  conductor.userOffsetms = selectedSongEntry.offsetMs + chart.offsetMs;

        // 5) 재생
        conductor.music = audioSource;
        conductor.PlayScheduled(0.1);
    }
    #endregion

    #region - judge에서 모든 노트판정 끝남 신호가 오면 콜백
    private void HandleAllnotesJudged()
    {

        Debug.Log("[GameEntry] 모든 노트 판정 완료!");

        if (_resultRoutine != null)
            StopCoroutine(_resultRoutine);

        _resultRoutine = StartCoroutine(GoToResultAfterDelay(5f)); //5초 뒤에 결과창으로 이동
    }

    private IEnumerator GoToResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ShowResult();
    }
    #endregion

    #region - 결과창 이동
    private void ShowResult()
    {

        if (conductor.music != null)
            conductor.music.Stop();


        if (resultUI != null)
        {
            resultUI.SetActive(true);
            PlayUI.SetActive(false);
        }

        if (!string.IsNullOrEmpty(afterGameDialogueFile))
        {
            StartCoroutine(ResumeDialogueAfterDelay(3f));
        }

    }
    #endregion

    private IEnumerator ResumeDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        resultUI.SetActive(false);

        DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI != null)
        {
            dialogueUI.ResumeAfterGame(afterGameDialogueFile);
        }
    }

    public void SelectSong(string songId, string diffText)
    {
        // SongEntry 찾기
        SongEntry found = FindSongEntry(songId);

        if (found == null)
        {
            Debug.LogError($"[GameEntry] SongEntry '{songId}' 를 찾을 수 없음");
            return;
        }

        selectedSongEntry = found;

        // diffText("Easy","Hard" 등)를 Difficulty enum으로 변환
        if (!System.Enum.TryParse(diffText, true, out Difficulty diff))
        {
            Debug.LogWarning($"[GameEntry] 난이도 변환 실패: {diffText}, 기본 Easy로 설정");
            diff = Difficulty.Easy;
        }

        selectedDifficulty = diff;

        gridManager.SetGridDiff(selectedDifficulty);
        noteSpawner.difficulty = selectedDifficulty;
        noteSpawner.grid = gridManager.GetCurrentGrid();

        Debug.Log($"[GameEntry] SelectSong 완료 → Song: {selectedSongEntry.title}, Diff: {selectedDifficulty}");
    }

    public SongEntry[] allSongs;

    private SongEntry FindSongEntry(string songId)
    {
        return allSongs.FirstOrDefault(s => s.songId == songId);
    }

}


