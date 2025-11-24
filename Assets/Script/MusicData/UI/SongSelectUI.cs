using UnityEngine;
using UnityEngine.UI;

public class SongSelectUI: MonoBehaviour
{
    public MusicDatabase database;
    //SongEntry 리스트 기반으로 UI 버튼 목록 생성

    public GameObject songbutpre;
    // 노래 목록 버튼 프리팹

    public Transform songListRoot;
    //버튼 위치 배치

    public SelectDifficultyUI difficultyUI;
    // 난이도 선택 스트립트 참조

    private void Start()
    {
        CreatSongButtons();
    }

    void CreatSongButtons()
    {
        foreach(var song in database.songs)
        {
            var btnObj = Instantiate(songbutpre, songListRoot);
            btnObj.GetComponentInChildren<Text>().text = song.title;

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                difficultyUI.Open(song); //곡 하나 선택 → 난이도 창으로 넘기기
            });
        }
    }




}
