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
            // 1) 타입 파싱 (스프라이트용, 실패하면 Normal로 기본값)
            NoteType t;
            if (!Enum.TryParse(n.type, out t))
                t = NoteType.Normal; // 혹은 Normal 같은 공통 타입으로

            // 2) NoteData 채우기
            list.Add(new NoteData
            {
                id = n.id,
                type = t,
                Timesec = n.timeSec,
                // durationSec는 안 쓰니까 생략 가능
                // durationSec = n.durationSec,

                // 새 필드: 키보드 키 정보
                key = n.key    // string
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