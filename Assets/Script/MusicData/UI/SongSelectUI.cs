using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongSelectUI: MonoBehaviour
{
    public MusicDatabase database;
    //SongEntry 리스트 기반으로 UI 버튼 목록 생성

    [Header("노래 선택 영역")]
    [SerializeField]
    private int centerChildIndex = 2;

    [Header("노래 선택 버튼 프리팹 및 정렬 설정")]
    public GameObject songbutpre; // 노래 목록 버튼 프리팹
    public Transform songListRoot;  // 버튼들이 정렬될 부모 컴포넌트

    [Header("노래 리스트 선택")]
    public TMP_Text songtitle;
    public Image menuImage;
    public Button prebut;  // 곡 선택 버튼 ->이전
    public Button nextbut;  //곡 선택 버튼 -> 이후

    public SelectDifficultyUI difficultyUI;
    // 난이도 선택 UI

    private readonly List<Button> _songbuttons = new();
    private readonly List<SongEntry> _songentries = new();

    private SongListUI _currentCenterBtn;
    public List<Sprite> songSprites;


    void Start()
    {
        if (database == null || database.songs == null || database.songs.Count == 0)
        {
            Debug.LogError("[SongSelectUI] MusicDatabase가 비어 있습니다.");
            return;
        }

        CreatSongButtons();         // 버튼 생성
        UpdateSelectedSongUI();     // 중앙 버튼 한 번 계산

        prebut.onClick.AddListener(PreSongselect);
        nextbut.onClick.AddListener(NextSongselect);
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

    #region - 중앙에 있는 곡 리스트만 클릭가능
    void OnSongButtonClicked(SongListUI clicked)
    {
        if (clicked == null) return;

        int siblingIndex = clicked.transform.GetSiblingIndex();

        Debug.Log($"[SongSelectUI] Clicked={clicked.name}, index={siblingIndex}, centerIndex={centerChildIndex}");

        // 선택 영역 안에 있는 버튼만 허용
        if (clicked != _currentCenterBtn)
        {
            Debug.Log("[SongSelectUI] 선택 영역 안에 있는 곡만 선택 가능합니다.");
            return;
        }

        if (clicked.song == null)
        {
            Debug.LogError("[SongSelectUI] 클릭된 버튼에 song 데이터가 없습니다.");
            return;
        }
        //선택된 곡에서 난이도UI 열기
        difficultyUI.Open(clicked.song);
    }
    #endregion


    #region - 선택 영역에 버튼이 들어왔는지 판정
    SongListUI GetCenterButtonByIndex()
    {
        int count = songListRoot.childCount;
        if (count == 0) return null;

        int idx = Mathf.Clamp(centerChildIndex, 0, count - 1);

        var t = songListRoot.GetChild(idx);
        return t.GetComponent<SongListUI>();
    }
    #endregion


    #region - 노래 리스트 버튼 생성 -> SongEntry 갯수에 맞춰 프리팹을 생성 및 배치 
    void CreatSongButtons()
    {
        int count = database.songs.Count;
        for (int i = 0; i < count; i++)
        {
            var song = database.songs[i];

            var btnObj = Instantiate(songbutpre, songListRoot);
            var songBtn = btnObj.GetComponent<SongListUI>();
            var uiButton = btnObj.GetComponent<Button>();

            Sprite cover = null;
            if (i < songSprites.Count)
                cover = songSprites[i];

            if (songBtn != null)
            {
                songBtn.Setup(song, cover);   // ?? 스프라이트까지 같이 넘기기
            }

            var localSongBtn = songBtn;
            uiButton.onClick.AddListener(() =>
            {
                OnSongButtonClicked(localSongBtn);
            });
        }
    }

    #endregion


    #region - 노래 정보 표시
    void UpdateSelectedSongUI()
    {
        _currentCenterBtn = GetCenterButtonByIndex();  // ← 인덱스로 중앙 버튼 찾기

        if (_currentCenterBtn == null)
        {
            Debug.LogWarning("[SongSelectUI] 중앙 버튼을 찾지 못했습니다.");
            return;
        }

        if (_currentCenterBtn.song == null)
        {
            Debug.LogError("[SongSelectUI] 중앙 버튼에 Song 데이터가 없습니다.");
            return;
        }

        var song = _currentCenterBtn.song;

        // 곡 제목 출력
        if (songtitle != null)
            songtitle.text = song.title;

        // 곡 메뉴이미지 출력
        if (menuImage != null)
        { int idx = database.songs.IndexOf(song);
            if (idx >= 0 && idx < songSprites.Count)
            {
                menuImage.sprite = songSprites[idx];
                // 필요하면 크기 맞추기
                // if (menuImage.sprite != null) menuImage.SetNativeSize();
            }
            else
            {
                Debug.LogWarning($"[SongSelectUI] 메뉴 스프라이트 인덱스 범위 밖: idx={idx}, spritesCount={songSprites.Count}");
            }
        }

        int count = songListRoot.childCount;

        for (int i = 0; i < count; i++)
        {
            var t = songListRoot.GetChild(i);
            var btn = t.GetComponent<Button>();
            var img = t.GetComponent<Image>();
            var ui = t.GetComponent<SongListUI>();

            bool isSelected = (ui == _currentCenterBtn);

            if (btn != null)
                btn.interactable = true;
           // btn.interactable = isSelected; // 중앙만 클릭 가능

            if (img != null)
                img.color = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }

        if (difficultyUI != null)
            difficultyUI.ResetDifficulty();
    }
    #endregion
}
