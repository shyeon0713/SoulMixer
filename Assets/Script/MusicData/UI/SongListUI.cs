using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 선택한 곡 리스트를 보여주기 위한 컴포넌트 UI코드 
// 곡을 선택할 때, 해당 에셋의 텍스트 및 이미지 내용을 변경하는 방식이 아닌
// 선택영역을 지정해두고, 버튼 리스트가 이동하는 방식으로 UI 제작 -> 새로운 방식으로 버튼 선택 UI를 구현하고 싶었음.

public class SongListUI : MonoBehaviour
{
    public SongEntry song;     // 이 버튼이 들고 있는 곡
    
    public TMP_Text title; // 버튼에 표시될 곡 제목
    [SerializeField] private Image menuImage;
    public void Setup(SongEntry songEntry, Sprite menuSprite)
    {
        song = songEntry;

        if (title != null)  // 타이틀 정보가 있을 경우.
            title.text = songEntry.title;  //타이틀 정보 출력

        if (menuImage != null)
            menuImage.sprite = menuSprite;

    }
}
