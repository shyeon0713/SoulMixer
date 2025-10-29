using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputManagerEntry;
public enum NoteType  //note타입은 총 5개
{
    NormalNote_R, //마우스 오른쪽 클릭_일반노트
    NormalNote_L, //마우스 왼쪽 클릭_일반노트
    LongNote, //마우스 휠 클릭 _ 롱노트
    SlideNote_R, //마우스 오른쪽 슬라이드_슬라이드노트
    SlideNote_L //마우스 왼쪽 슬라이드_슬라이드노트

}

[System.Serializable]
public class NoteData
{
    public int id;
    public NoteType type;
    public float Timesec; //판정시간(초단위)

    [Tooltip("Long 노트일 때만 사용 (초)")]
    public float durationSec = 0f;

    [Header("Optional")]
    public float lane = 0; // 추후에 레인개념 추가 -> 현재는 레인 사용안함

}
