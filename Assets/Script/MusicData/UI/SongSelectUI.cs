using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongSelectUI: MonoBehaviour
{
    public MusicDatabase database;
    //SongEntry 리스트 기반으로 UI 버튼 목록 생성

    [Header("노래 선택 버튼 프리팹 및 정렬 설정")]
    public GameObject songbutpre; // 노래 목록 버튼 프리팹
    public Transform songListRoot;  // 버튼들이 정렬될 부모 컴포넌트

    [Header("노래 리스트 선택")]
    public TMP_Text songtitle;
    public Button prebut;  // 곡 선택 버튼 ->이전
    public Button nextbut;  //곡 선택 버튼 -> 이후

    private int currentIndex = 0;  // 곡 선택 index ,초기 0번으로 설정

    public SelectDifficultyUI difficultyUI;
    // 난이도 선택 UI

    private void Start()
    {
        // 노래 리스트 존재하는지 디버깅 확인
        if (database == null || database.songs == null || database.songs.Count == 0)
        {
            Debug.LogError("[SongSelectUI] MusicDatabase가 비어 있습니다.");
            return;
        }

        CreatSongButtons();  // 노래 리스트 버튼 배치

        prebut.onClick.AddListener(PreSongselect);  // 이전 버튼 리스너 추가
        nextbut.onClick.AddListener(NextSongselect); //다음 버튼 리스너 추가

        UpdateSelectedSongUI();  // 중앙 영역에 있는 곡 정보 갱신
    }



    #region - 리스너 연결 -> 버튼에 따라 노래 리스트 버튼 이동
    void PreSongselect()
    {
        int count = songListRoot.childCount;   // .childCount -> 현재 부모가 가지고 있는 자식 개수 
        if (count <= 1) return;

        Transform last = songListRoot.GetChild(count - 1);   // songListRoot의 count -1 번째 인덱스의 자식을 가져오는 함수
        last.SetSiblingIndex(0);  // SetSiblingIndex( ) : 부모의 자식 컴포넌트 순서를 바꾸는 함수
        // 0번 인덱스를 맨 뒤로 이동

        UpdateSelectedSongUI();
    }

    void NextSongselect()
    {
        int count = songListRoot.childCount;
        if(count <= 1) return;

        Transform first = songListRoot.GetChild(0);  //songListRoot의 0번째 인덱스의 자식을 가져오는 함수
        first.SetSiblingIndex(count - 1);
        // 마지막 인덱스를 맨 앞으로 이동

        UpdateSelectedSongUI();
    }

    #endregion


    #region - 노래 리스트 버튼 생성 -> SongEntry 갯수에 맞춰 프리팹을 생성 및 배치 
    void CreatSongButtons()
    {
        foreach(var song in database.songs)
        {
            var btnObj = Instantiate(songbutpre, songListRoot);  // 리스트 버튼 배치

            var songBtn = btnObj.GetComponent<SongListUI>();
            if (songBtn != null)
                songBtn.Setup(song);
            //추후에 자켓 이미지도 추가하는 방향 고려
            //
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                    difficultyUI.Open(song); //곡 하나 선택 → 난이도 창으로 넘기기
            });
           
        }
    }

    #endregion

    #region- 선택 영역에 있는 노래 표시 
    SongEntry GetCenterSong()
    {
        int count = songListRoot.childCount;
        if (count == 0) return null;

        int centerIndex = count / 2;
        var t = songListRoot.GetChild(centerIndex);
        var btn = t.GetComponent<SongListUI>();

        return btn != null ? btn.song : null;
    }
    #endregion

    #region - 노래 정보 표시
    void UpdateSelectedSongUI()
    {
        var song = GetCenterSong();  // 선택 영역에 있는 노래 선택
        if (song == null)
        {
            if (songtitle != null) songtitle.text = "-";
            return;
        }

        if (songtitle != null)
            songtitle.text = song.title;
    }
    #endregion
}
