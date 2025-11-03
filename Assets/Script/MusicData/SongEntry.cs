using Unity.VisualScripting;
using UnityEngine;

public enum Difficulty { Easy, Normal, Hard, Expert }

[CreateAssetMenu(fileName = "SongEntry", menuName = "Script/MusicData/Song Entry")]
[System.Serializable]
class Musicdata{
    string title;
    float totalSec;
    int counts;
    float totalLongSec;
    NoteType notetype;

}
public class SongEntry : ScriptableObject
{
    public string songId;                 // 내부 키(폴더명 등)
    public string title;
    public string artist;
    public AudioClip audioClip;           // 오디오 직접 참조
  //  public Sprite jacket;                 // 자켓 이미지(선택)

    [Header("Timing")]
    public float offsetMs = 0f;           // 유저/채보 보정용(옵션)
    public float bpm;                     // 메타(옵션)

    [Header("Charts")]
    public TextAsset chartJson;           // 채보(TextAsset)
    public Difficulty difficulty = Difficulty.Normal;
}
