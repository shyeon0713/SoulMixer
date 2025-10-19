using UnityEngine;
using System;
using Rhythm.Charting;

namespace Rhythm.Core
{
    public class Conductor : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource music;
        [Tooltip("음악 시작 예약까지의 지연(초)")]
        public double startDelay = 1.0;

        [Header("Chart Meta")]
        public chartmeta meta;

        [Header("Offsets (user/auto)")]
        [Tooltip("오디오/판정 기준 오프셋(ms). + 면 곡 기준이 늦게 잡힘")]
        public float audioOffsetMs = 0f; //사용자 설정

        [Tooltip("입력 타임스탬프 보정(ms). +면 입력이 늦게 잡힌다고 가정")]
        public float inputOffsetMs = 0f; //자동 추정 혹은 옵션

        private double _dspSongStart;
        private bool _started; // 악곡 시작 여부

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
