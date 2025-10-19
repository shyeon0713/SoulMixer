using UnityEngine;
using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using Rhythm.Charting;

namespace Rhythm.Charting
{
    public enum notetype { tap, hold, slide }  //���ϳ�Ʈ / �ճ�Ʈ/ �����̵� ��Ʈ
    public enum sildedir { none = 0, left = -1, right = 1 }


    [Serializable]
    public struct notedata
    {
        public float timesec; //precomputed -> �ð��� ���� �ɸ��� ����� �̸� �صα� (offset�� bpm�ݿ�)
        public int lane; //���� ����
        public notetype type;
        public float duration;  // hold�� sec.�� tap�̸� 0
        public int sildedir; // �����̵� ����
    }

    public class chartmeta
    {
        public float bpm;
        public float offsetsec = 0f;  //���� ���� ��� ���� ����
        public float beatsperbar = 4;
        public int beatunit = 4;
    }
}
