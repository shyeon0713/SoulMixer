using UnityEngine;

public class GameEntry : MonoBehaviour
{
    public Conductor conductor;
    public Judge judge;
    public MouseInputAdapter inputAdapter;
    public AudioSource music;

    void Start()
    {
        // 1) 차트 로드
        var notes = ChartLoader.LoadFromStreamingAssets("MySong.chart.json");
        judge.LoadChart(notes);

        // 2) 콜백 연결(예: 로그/스코어)
        judge.OnHit += (note, grade) => Debug.Log($"HIT {note.type} {grade}");
        judge.OnMiss += note => Debug.Log($"MISS {note.type}");

        // 3) 오디오 시작(DSP 기준)
        conductor.music = music;
        conductor.PlayScheduled(0.10);
    }
}
