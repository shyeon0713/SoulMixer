using System.Linq;
using UnityEngine;

public class SetMusic : MonoBehaviour
{
    [Header("곡 리스트")]
    public SongEntry[] songs;

    [Header("게임 실행 엔트리")]
    public GameEntry gameEntry;  // 게임 실행을 위해 엔트리 호출
    // 같은 Title의 SongEntry를 찾은뒤
    // GameEntry에 넘겨서 재생 

    public void PlayByChartJson(TextAsset chartJson)
    {
        if (chartJson == null)   //곡 리스트가 비어있는지 확인
        {
            Debug.LogError("[SongDatabase] Chartjson이 null입니다.");
            return;
        }

        //json 파싱하기
        var chart = JsonUtility.FromJson<SongChartJson>(chartJson.text);
        if (chart == null)
        {
            Debug.LogError("[SongDatabase] JSON 파싱 실패");
            return;
        }


        //Tilte로 songEntry찾기
        var entry = FindSongByTitle(chart.title);
        if (entry == null)
        {
            Debug.LogError($"[SongDatabase] title='{chart.title}'에 해당하는 SongEntry를 찾을 수 없습니다.");
            return;
        }

        //GameEntry에 연결 후 재생
        gameEntry.selectedSongEntry = entry;
        gameEntry.InitAndPlay();
    }


    public SongEntry FindSongByTitle(string title)
    {
        return songs.FirstOrDefault(s => s.title == title);
    }

}
