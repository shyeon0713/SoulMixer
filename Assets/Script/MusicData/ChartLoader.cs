using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChartLoader {


    public static NoteData[] LoadFromJsonText(string json)
    {
        var chart = JsonUtility.FromJson<SongChartJson>(json);
        if (chart == null || chart.notes == null)
        {
            Debug.LogError("[ChartLoader] Json 파싱 실패 또는 chart.notes == null");
            return Array.Empty<NoteData>();
        }

        var list = new List<NoteData>(chart.notes.Count);
        foreach (var n in chart.notes)
        {
            if (!Enum.TryParse(n.type, out NoteType t))
                t = NoteType.NormalNote_L;

            list.Add(new NoteData
            {
                id = n.id,
                type = t,
                Timesec = n.timeSec,
                durationSec = n.durationSec
            });
        }

        list.Sort((a, b) => a.Timesec.CompareTo(b.Timesec));
        return list.ToArray();
    }


    public static NoteData[] LoadFromSongEntry(SongEntry songEntry, Difficulty difficulty)
    {
        if (songEntry == null)
        {
            Debug.LogError("[ChartLoader] songEntry가 null 입니다.");
            return Array.Empty<NoteData>();
        }

        var chartEntry = songEntry.GetChart(difficulty);
        if (chartEntry == null)
        {
            Debug.LogError($"[ChartLoader] {songEntry.title} - {difficulty} 차트를 찾을 수 없습니다.");
            return Array.Empty<NoteData>();
        }

        if (chartEntry.chartJson == null)
        {
            Debug.LogError($"[ChartLoader] {songEntry.title} - {difficulty} chartJson(TextAsset)이 비어 있습니다.");
            return Array.Empty<NoteData>();
        }

        string json = chartEntry.chartJson.text;
        return LoadFromJsonText(json);
    }
}