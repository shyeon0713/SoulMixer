using Rhythm.Charting;
using Rhythm.Core;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static TreeEditor.TreeEditorHelper;

namespace Rhythm.Gameplay
{
    public class Judge : MonoBehaviour
    {
        [Header("Refs")]
        public Conductor conductor;
        public Chart chart;
        public Spawner spawner;

        [Header("Windows (sec)")]
        public float perfect = 0.033f; // ��33ms
        public float great = 0.066f; // ��66ms
        public float good = 0.100f; // ��100ms

        // ���� ����: ���� �ε��� �ϳ��� ����
        private int _nextIdx = 0;

        // �ܺ� �Է� �����
        public void FireTap(double dspAtPress) => FireCommon(dspAtPress, notetype.tap, 0);
        public void FireHoldStart(double dspAtPress) => FireCommon(dspAtPress, notetype.hold, 0);
        public void FireHoldEnd(double dspAtRelease) { /* ����/���� Ȯ�� ����Ʈ */ }
        public void FireSlide(double dspAtEvent, sildedir dir) => FireCommon(dspAtEvent, notetype.slide, (int)dir);

        public event System.Action<int, string, float> Judged;                 // (chartIndex, grade, dt)
        public event System.Action<int, string, float, notetype> JudgedEx;      // Ÿ�� ����
        private void FireCommon(double dspTime, notetype type, int inputSlideDir)
        {
            float songSec = (float)(dspTime - conductor.SongStartDSP) - conductor.audioOffsetMs / 1000f - conductor.inputOffsetMs / 1000f;
            TryJudge(songSec, requiredType: type, inputSlideDir: inputSlideDir);
        }

        private void TryJudge(float inputTimeSec, notetype? requiredType = null, int inputSlideDir = 0)
        {
            if (_nextIdx >= chart.notes.Count) return;
            var note = chart.notes[_nextIdx];
            if (requiredType.HasValue && note.type != requiredType.Value) return;

            if (note.type == notetype.slide)
            {
                int need = note.sildeDir; // 0=any, -1/1 ���� �䱸
                if (need != 0 && System.Math.Sign(need) != System.Math.Sign(inputSlideDir)) return;
            }

            float dt = inputTimeSec - note.timesec;
            float adt = Mathf.Abs(dt);

            if (adt <= perfect) OnHit("Perfect", _nextIdx, dt);
            else if (adt <= great) OnHit("Great", _nextIdx, dt);
            else if (adt <= good) OnHit("Good", _nextIdx, dt);
            else { /* miss window ó�� ���� */ }
        }

        private void OnHit(string grade, int chartIndex, float delta)
        {
            _nextIdx = chartIndex + 1;
            Judged?.Invoke(chartIndex, grade, delta);
            var t = chart.notes[chartIndex].type;
            JudgedEx?.Invoke(chartIndex, grade, delta, t);
        }
    }
}
