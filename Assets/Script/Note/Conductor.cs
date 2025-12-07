using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class NoteJson  
{
    public int id;

    public NoteType type;
    public float Timesec; //판정시간(초단위)

    public string key; // 입력받을 키보드 키

    // 출발 위치(엣지)
    public string spawnEdge;
    public int spawnIndex;

    // 경로 길이 범위
    public int minpath;
    public int maxpath;

    public float moveTime;   // 이동 시간
    public float judgeTime;  // 판정 유지 시간
}


public class Conductor : MonoBehaviour
{
    public AudioSource music;
    public double dspStart;
   public float userOffsetms = 0f;

    public float startDelaySec = 1f;  //곡 시작 딜레이 -> 1초 뒤 시작

    public float NowSec => (float)(AudioSettings.dspTime - dspStart) + (userOffsetms / 1000f);

    public void PlayScheduled(double lead = 0.010)
    {
        double totalLead = lead + startDelaySec;  // 딜레이추가

        dspStart = AudioSettings.dspTime + totalLead;
        music.PlayScheduled(dspStart);

        // 시작하는 초 알기
        Debug.Log($"[Conductor] PlayScheduled - dspStart: {dspStart}, AudioSettings.dspTime: {AudioSettings.dspTime}, totalLead: {totalLead}");
        Debug.Log($"[Conductor] 현재 NowSec: {NowSec}");
    }
}
