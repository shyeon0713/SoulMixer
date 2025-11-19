using Unity.VisualScripting;
using UnityEngine;

public enum Difficulty { Easy, Normal, Hard, Expert }

[CreateAssetMenu(fileName = "SongEntry", menuName = "Script/MusicData/Song Entry")]
[System.Serializable]
public class SongEntry : ScriptableObject
{
    [Header("곡 설명부분")]
    public string songId;                
    public string title;
    public AudioClip audioClip;           
                                                      

    [Header("곡 BPM")]
    public float offsetMs = 0f;           // 곡 전체 공통 오프셋
    public float bpm;                     


    [Header("난이도별 채보")]
    public ChartEntry[] charts;

    [System.Serializable]
    public class ChartEntry
    {
        public Difficulty difficulty = Difficulty.Normal; 
        public TextAsset chartJson;                       
        public int level;                                 
        public float offsetMs = 0f;                       
    }

    public ChartEntry GetChart(Difficulty diff)
    {
        if (charts == null) return null;

        foreach (var chart in charts)
        {
            if (chart != null && chart.difficulty == diff)
                return chart;
        }

        return null;
    }
}
