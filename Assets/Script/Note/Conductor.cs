using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class NoteJson
{
    public int id;
    public string type;  // 노트 타입 명시
    public float timeSec; //판정 시간
    public float durationSec; //롱노트만 
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
