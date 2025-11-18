using UnityEngine;

public class GameEntry : MonoBehaviour
{
    [Header("Refs")]
    public Conductor conductor;
    public Judge judge;
    public NoteSpawner noteSpawner;     // UGUI 스포너
    public AudioSource audioSource;

    [Header("Selected Song (선택 결과가 들어오는 곳)")]
    public SongEntry selectedSongEntry; // ← 이 필드가 꼭 있어야 함!

    // 버튼에서 호출할 초기화 + 재생
    public void InitAndPlay()
    {
        if (selectedSongEntry == null)
        {
            Debug.LogError("[GameEntry] selectedSongEntry가 비었습니다.");
            return;
        }
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // 1) 오디오 연결
        audioSource.clip = selectedSongEntry.audioClip;

        // 2) 차트 파싱 → NoteData[]
        var chart = JsonUtility.FromJson<SongChartJson>(selectedSongEntry.chartJson.text);
        var notes = ConvertToNoteData(chart);

        // 3) 판정/스폰에 전달
        judge.LoadChart(notes);
        noteSpawner.LoadChart(notes);

        // 4) 오프셋(선택)
        conductor.userOffsetms = selectedSongEntry.offsetMs;

        // 5) 재생
        conductor.music = audioSource;
        conductor.PlayScheduled(0.10);
    }

    // JSON → NoteData[] 변환 (네 프로젝트 필드명에 맞게)
    private NoteData[] ConvertToNoteData(SongChartJson chart)
    {
        var list = new System.Collections.Generic.List<NoteData>(chart.notes.Count);
        foreach (var n in chart.notes)
        {
            if (!System.Enum.TryParse(n.type, out NoteType t))
                t = NoteType.NormalNote_L;

            list.Add(new NoteData
            {
                id = n.id,
                type = t,
                Timesec = n.timesec,      
                durationSec = n.durationsec
               
            });
        }
        list.Sort((a, b) => a.Timesec.CompareTo(b.Timesec));
        return list.ToArray();
    }
}
