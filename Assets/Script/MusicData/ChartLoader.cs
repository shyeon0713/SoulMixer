using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChartLoader {

    //노트 데이터 파싱
    public static NoteData[] LoadFromJsonText(string json)
    {
        var root = JsonUtility.FromJson<ChartRoot>(json);
       
        if (root == null || root.notes == null)
        {
            Debug.LogError("[ChartLoader] Json 파싱 실패 또는 chart.notes == null");
            return Array.Empty<NoteData>();
        }

        List<NoteData> list = new();

        foreach (var n in root.notes)
        {
            // NoteType 파싱 실패시 기본 Normal 처리
            NoteType noteType = NoteType.Normal;
            if (!Enum.TryParse(n.type.ToString(), out noteType))
                noteType = NoteType.Normal;

            // NoteData로 변환
            NoteData data = new NoteData
            {
                id = n.id,
                type = noteType,
                Timesec = n.Timesec,
                key = n.key,

                // 출발 Edge
                spawnEdge = n.spawnEdge,
                spawnIndex = n.spawnIndex,

                // 경로 길이 범위
                minpath = n.minpath,
                maxpath = n.maxpath,

                // 이동 / 판정 시간
                moveTime = n.moveTime,
                judgeTime = n.judgeTime

            };

            list.Add(data);
        }
        // 시간 기준 정렬
        list.Sort((a, b) => a.Timesec.CompareTo(b.Timesec));

        return list.ToArray();
    }


    // 곡 선택에 따라 해당 json파일 파싱
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