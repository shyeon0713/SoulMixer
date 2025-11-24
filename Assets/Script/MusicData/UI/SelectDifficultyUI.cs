using UnityEngine;
using UnityEngine.UI;

// 난이도 설정 코드 - UI
public class SelectDifficultyUI : MonoBehaviour
{
    public Button easybutton;
    public Button normalbutton;
    public Button hardbutton;
    public Button expertbutton;

    private SongEntry currentSong;
    public GameEntry gameEntry;


    public void Start()
    {
        easybutton.interactable = false;  //interactable = false 클릭불가 상태 설정
        normalbutton.interactable = false;
        hardbutton.interactable = false;
        expertbutton.interactable = false;
         
        easybutton.image.color = Color.gray;   // 비활성화 및 회색으로 설정
        normalbutton.image.color = Color.gray;
        hardbutton.image.color = Color.gray;
        expertbutton.image.color = Color.gray;
    }
    #region  - 시작 설정
    public void Open(SongEntry song)
    {
        currentSong = song;
        gameObject.SetActive(true);

        easybutton.interactable = true;  //interactable = false 클릭불가 상태 설정
        normalbutton.interactable = true;
        hardbutton.interactable = true;
        expertbutton.interactable = true;

        easybutton.image.color = Color.white;   // 비활성화 및 회색으로 설정
        normalbutton.image.color = Color.white;
        hardbutton.image.color = Color.white;
        expertbutton.image.color = Color.white;

        SetButton();  // 버튼 선택
    }
    #endregion
    #region - 난이도 버튼 설정
    void SetButton()
    {
        easybutton.interactable = currentSong.GetChart(Difficulty.Easy) != null;
        normalbutton.interactable = currentSong.GetChart(Difficulty.Normal) != null;
        hardbutton.interactable = currentSong.GetChart(Difficulty.Hard) != null;
        expertbutton.interactable = currentSong.GetChart(Difficulty.Expert) != null;

        easybutton.onClick.AddListener(() => StartGame(Difficulty.Easy));
        normalbutton.onClick.AddListener(() => StartGame(Difficulty.Easy));

    }

    #endregion
    void StartGame(Difficulty diff)
    {
        gameEntry.selectedSongEntry = currentSong;
        gameEntry.selectedDifficulty = diff;

        gameEntry.InitAndPlay();  // 해당 난이도로 게임 시작
    }
}
