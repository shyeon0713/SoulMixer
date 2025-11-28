using UnityEngine;
using UnityEngine.UI;

// 난이도 설정 코드 - UI
public class SelectDifficultyUI : MonoBehaviour
{

    [Header("난이도 버튼모음")]
    public Button easybutton;
    public Button normalbutton;
    public Button hardbutton;
    public Button expertbutton;

    [Header("현재 선택한 곡 설정 저장 변수")]
    private SongEntry currentSong;
    public GameEntry gameEntry;

    [Header("UI 활성화부분")]
    public GameObject SelectUI;  // 세팅창
    public GameObject PlayUI;  // 플레이창


     void Start()
    {
        DisableAll();
    }

    #region - 버튼 전부 비활성화
    void DisableAll() 
    {
        currentSong = null;

        easybutton.interactable = false;  //interactable = false 클릭불가 상태 설정
        normalbutton.interactable = false;
        hardbutton.interactable = false;
        expertbutton.interactable = false;

        easybutton.image.color = Color.gray;   // 비활성화 및 회색으로 설정
        normalbutton.image.color = Color.gray;
        hardbutton.image.color = Color.gray;
        expertbutton.image.color = Color.gray;

    }
    #endregion


    #region - 선택 곡목록이 바뀔 경우, 이전에 선택한 설정 전부 비활성화
    public void ResetDifficulty()
    {
        DisableAll();   
        
    }
    #endregion


    #region  - 시작 설정 + 난이도 설정도 추가하는 방식으로 수정 
    public void Open(SongEntry song)
    {
        currentSong = song;
        gameObject.SetActive(true);

        easybutton.interactable = currentSong.GetChart(Difficulty.Easy) != null;
        normalbutton.interactable = currentSong.GetChart(Difficulty.Normal) != null;
        hardbutton.interactable = currentSong.GetChart(Difficulty.Hard) != null;
        expertbutton.interactable = currentSong.GetChart(Difficulty.Expert) != null;

        easybutton.image.color = easybutton.interactable ? Color.white : Color.gray;
        normalbutton.image.color = normalbutton.interactable ? Color.white : Color.gray;
        hardbutton.image.color = hardbutton.interactable ? Color.white : Color.gray;
        expertbutton.image.color = expertbutton.interactable ? Color.white : Color.gray;

        // 리스너 중복 제거
        easybutton.onClick.RemoveAllListeners();
        normalbutton.onClick.RemoveAllListeners();
        hardbutton.onClick.RemoveAllListeners();
        expertbutton.onClick.RemoveAllListeners();


        // 난이도별로 StartGame 연결
        easybutton.onClick.AddListener(() => StartGame(Difficulty.Easy));
        normalbutton.onClick.AddListener(() => StartGame(Difficulty.Normal));
        hardbutton.onClick.AddListener(() => StartGame(Difficulty.Hard));
        expertbutton.onClick.AddListener(() => StartGame(Difficulty.Expert));
    }
    #endregion

    void StartGame(Difficulty diff)
    {
        gameEntry.selectedSongEntry = currentSong;
        gameEntry.selectedDifficulty = diff;

        gameEntry.InitAndPlay();  // 해당 난이도로 게임 시작

        SelectUI.SetActive(false);  // 선택창 비활성화
        PlayUI.SetActive(true);  // 플레이창으로 이동하여 플레이
    }
}
