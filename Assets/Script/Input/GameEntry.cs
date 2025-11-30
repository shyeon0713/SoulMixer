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

    [Header("악곡/난이도")]
    public SongEntry selectedSongEntry;
    public Difficulty selectedDifficulty; // 난이도


    public GameObject resultUI;  // 결과창 UI 띄우기
    public GameObject PlayUI;

    private Coroutine _resultRoutine; // 5초뒤에 결과창 UI로 이동하기 위해 코루틴 생성

    // 테스트용: 씬 켜지면 자동 재생
    void Start()
    {
        Debug.Log("[GameEntry] Start 호출");
        // InitAndPlay();
    }

    #region - 버튼에서 호출할 초기화 + 재생
    public void InitAndPlay()
    {
        if (selectedSongEntry == null)
        {
            Debug.LogError("[GameEntry] selectedSongEntry가 비었습니다.");
            return;
        }


        if (!audioSource) audioSource = GetComponent<AudioSource>();



        // 1) 오디오 연결
        audioSource.clip = selectedSongEntry.audioClip;
        Debug.Log($"[GameEntry] AudioClip = {audioSource.clip?.name}");



        // 2) 차트 파싱 → NoteData[]
        var notes = ChartLoader.LoadFromSongEntry(selectedSongEntry, selectedDifficulty);
        if (notes.Length == 0)
        {
            Debug.LogError("[GameEntry] LoadFromSongEntry 결과가 비었습니다.");
            return;
        }

        //판정/ 스폰 세팅
        judge.LoadChart(notes);
        noteSpawner.LoadChart(notes);


        // 오프셋 적용
        var chart = selectedSongEntry.GetChart(selectedDifficulty);
        //  conductor.userOffsetms = selectedSongEntry.offsetMs + chart.offsetMs;

        // 5) 재생
        conductor.music = audioSource;
        conductor.PlayScheduled(0.10);
    }
    #endregion

    #region - judge에서 모든 노트판정 끝남 신호가 오면 콜백
    private void HandleAllnotesJudged()
    {
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

        if (resultUI != null)
        {
            resultUI.SetActive(true);
            PlayUI.SetActive(false);
        }

    }
    #endregion
}


