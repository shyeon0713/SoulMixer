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
    public string type;  // ��Ʈ Ÿ�� ���
    public float timesec; //���� �ð�
    public float durationsec; //�ճ�Ʈ�� 
}

[Serializable]
public class SongChartJson
{
    public string title; // �ǰ�����
    public float totalsec; //��ü �ð� 
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
#region - json���� �ҷ�����+�б�
public static class ChartLoader
{  //StreamingAssets ��ο��� json�� �о� notedata[ ]�� ��ȯ
   public static NoteData[] LoadFromStreamingAssets(string fileName)
   {
        string path = Path.Combine(Application.streamingAssetsPath, fileName); //��� �����
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
                lane = 0  //���� ���α��������� �ʾ� ����

            });
        }
        list.Sort((a,b) => a.Timesec.CompareTo(b.Timesec));
        return list.ToArray();
   }

    static NoteType ParseType(string s)
    {
    if (Enum.TryParse<NoteType>(s,out var t)) return t;  //json ���ڿ��� enum�̸��� ���� ���, �״�� �Ľ�
        return NoteType.NormalNote_L; //���ܳ� ��Ī ������ �ʿ��� ��� ���⼭ ���� -> �켱 ���� ǥ�� ��Ʈ�� ����
    }

}
#endregion