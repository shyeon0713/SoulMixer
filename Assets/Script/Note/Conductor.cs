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

    public string key;
   
}


public class Conductor : MonoBehaviour
{
    public AudioSource music;
    public double dspStart;
   public float userOffsetms = 0f;

    public float startDelaySec = 3f;  //곡 시작 딜레이 -> 3초 뒤 시작

    public float NowSec => (float)(AudioSettings.dspTime - dspStart) + userOffsetms / 1000f;

    public void PlayScheduled(double lead = 0.010)
    {
        double totalLead = lead + startDelaySec;  // 딜레이추가

        dspStart = AudioSettings.dspTime + totalLead;
        music.PlayScheduled(dspStart);
    }
}
