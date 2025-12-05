using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputManagerEntry;
public enum NoteType  //note타입은 총 5개
{
    Normal = 0  // 추후에 노트 추가 예정

}

public enum NoteKey
{
    W, A, S, D,
    Up, Down, Left, Right,
    Num1, Num2, Num3, Num4
}

[System.Serializable]
public class NoteData
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

