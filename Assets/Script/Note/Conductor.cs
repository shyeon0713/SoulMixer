using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class ChartCounts
{
    public int normalL,normalR,Long,sildeL,sildeR;
}

[Serializable]
public class NoteJson
{
    public int id;
    public string type;  // 노트 타입 명시
    public float timesec; //판정 시간
    public float durationsec; //롱노트만 
}

[Serializable]
public class SongChartJson
{
    public string title; // 악곡제목
    public float totalsec; //전체 시간 
    public ChartCounts counts;
    public float totalLongSec;
    public List<NoteJson> notes;
}
public class Conductor : MonoBehaviour
{
    public AudioSource music;
    public double dspStart;
    public float userOffsetms = 0f;

    public float NowSec => (float)(AudioSettings.dspTime - dspStart) + userOffsetms / 1000f;

    public void PlayScheduled(double lead = 0.010)
    {
        dspStart = AudioSettings.dspTime + lead;
        music.PlayScheduled(dspStart);
    }
}
#region - json파일 불러오기+읽기
public static class ChartLoader
{  //StreamingAssets 경로에서 json을 읽어 notedata[ ]로 변환
   public static NoteData[] LoadFromStreamingAssets(string fileName)
   {
        string path = Path.Combine(Application.streamingAssetsPath, fileName); //경로 만들기
        string json = File.ReadAllText(path);
        var chart = JsonUtility.FromJson<SongChartJson>(json);

        var list = new List<NoteData>(chart.notes.Count);
        foreach (var n in chart.notes)
        {
            NoteType t = ParseType(n.type);
            list.Add(new NoteData
            {
                id = n.id,
                type = t,
                Timesec = n.timesec,
                durationSec = n.durationsec,
                lane = 0  //현재 라인구분은하지 않아 제외

            });
        }
        list.Sort((a,b) => a.Timesec.CompareTo(b.Timesec));
        return list.ToArray();
   }

    static NoteType ParseType(string s)
    {
    if (Enum.TryParse<NoteType>(s,out var t)) return t;  //json 문자열이 enum이름과 같을 경우, 그대로 파싱
        return NoteType.NormalNote_L; //예외나 별칭 대응이 필요할 경우 여기서 매핑 -> 우선 왼쪽 표시 노트로 설정
    }

}
#endregion