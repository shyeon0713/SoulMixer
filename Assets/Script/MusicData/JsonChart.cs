using System;
using UnityEngine;

[System.Serializable]
public class ChartRoot
{
    public string title;
    public float totalSec;
    public NoteJson[] notes;
}

public class ScenarioRoot
{
    public DialogueLine[] scenario;     // 대사 배열
    public string nextSongTitle;        // 다음 곡 제목
    public string nextDifficulty;       // 다음 난이도 (Easy/Normal/Hard)
}

