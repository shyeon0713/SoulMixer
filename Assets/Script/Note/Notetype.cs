using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputManagerEntry;
public enum NoteType  //noteŸ���� �� 5��
{
    NormalNote_R, //���콺 ������ Ŭ��_�Ϲݳ�Ʈ
    NormalNote_L, //���콺 ���� Ŭ��_�Ϲݳ�Ʈ
    LongNote, //���콺 �� Ŭ�� _ �ճ�Ʈ
    SlideNote_R, //���콺 ������ �����̵�_�����̵��Ʈ
    SlideNote_L //���콺 ���� �����̵�_�����̵��Ʈ

}

[System.Serializable]
public class NoteData
{
    public int id;
    public NoteType type;
    public float Timesec; //�����ð�(�ʴ���)

    [Tooltip("Long ��Ʈ�� ���� ��� (��)")]
    public float durationSec = 0f;

    [Header("Optional")]
    public float lane = 0; // ���Ŀ� ���ΰ��� �߰� -> ����� ���� ������

}
