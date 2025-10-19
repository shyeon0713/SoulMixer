using UnityEngine;
using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using Rhythm.Charting;

namespace Rhythm.Charting
{
    public enum notetype { tap, hold, slide }  //단일노트 / 롱노트/ 슬라이드 노트
    public enum sildedir { none = 0, left = -1, right = 1 }


    [Serializable]
    public struct notedata
    {
        public float timesec; //precomputed -> 시간이 오래 걸리는 계산을 미리 해두기 (offset과 bpm반영)
        public int lane; //단일 라인
        public notetype type;
        public float duration;  // hold는 sec.로 tap이면 0
        public int sildedir; // 슬라이드 방향
    }

    public class chartmeta
    {
        public float bpm;
        public float offsetsec = 0f;  //응식 시작 대비 판정 기준
        public float beatsperbar = 4;
        public int beatunit = 4;
    }
}
