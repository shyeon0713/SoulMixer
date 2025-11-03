using UnityEngine;

public class MusicPlayController : MonoBehaviour
{
    public GameEntry entry;

    public void OnClickPlay(SongEntry music)
    {
        entry.selectedSongEntry = music;
        entry.InitAndPlay();

    }
}
