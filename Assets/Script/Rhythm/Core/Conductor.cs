using UnityEngine;
using System;
using Rhythm.Charting;

namespace Rhythm.Core
{
    public class Conductor : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource music;
        [Tooltip("���� ���� ��������� ����(��)")]
        public double startDelay = 1.0;

        [Header("Chart Meta")]
        public chartmeta meta;

        [Header("Offsets (user/auto)")]
        [Tooltip("�����/���� ���� ������(ms). + �� �� ������ �ʰ� ����")]
        public float audioOffsetMs = 0f; //����� ����

        [Tooltip("�Է� Ÿ�ӽ����� ����(ms). +�� �Է��� �ʰ� �����ٰ� ����")]
        public float inputOffsetMs = 0f; //�ڵ� ���� Ȥ�� �ɼ�

        private double _dspSongStart;
        private bool _started; // �ǰ� ���� ����

        public double DSPNow => AudioSettings.dspTime;
        void Start()
        {
            if (!music) { Debug.LogError("AudioSource missing on Conductor"); return; }
            _dspSongStart = DSPNow + startDelay;
            music.PlayScheduled(_dspSongStart);
            _started = true;
        }

        public float NowSongSec()
        {
            if (!_started) return 0f;
            double sec = DSPNow - _dspSongStart;
            sec -= audioOffsetMs / 1000.0;
            return (float)Math.Max(0.0, sec);
        }

        public void Schedule(AudioClip clip, double dspWhen)
        {
            if (clip != null) music.clip = clip;
            _dspSongStart = dspWhen;
            music.PlayScheduled(_dspSongStart);
            _started = true;
        }

        public float NowBeats() => SecToBeats(NowSongSec());
        public float SecToBeats(float sec) => sec * (meta.bpm / 60f);
        public float BeatsToSec(float beats) => beats * (60f / meta.bpm);


        public double SongStartDSP => _dspSongStart;

    }
}
