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


    [Header("난이도별 채보")]
    public ChartEntry[] charts;

    [System.Serializable]
    public class ChartEntry
    {
        public Difficulty difficulty; 
        public TextAsset chartJson;                                                      
        public float offsetMs = 0f;
        public float bpm;
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
