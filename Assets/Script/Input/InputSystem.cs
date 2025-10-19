using UnityEngine;
using UnityEngine.InputSystem;
using Rhythm.Gameplay;
using Rhythm.Charting;


namespace Rhythm.Inputs
{
    public class InputSystem : MonoBehaviour
    {
        [Header("Refs")]
        public Judge judge;

        public InputActionReference leftClick;
        public InputActionReference rightClick;
        public InputActionReference middleClick;
        public InputActionReference pointerDelta; // Vector2, Pass Through

        [Header("Slide Detect")]
        public float slideThresholdPixels = 120f;
        public float slideWindowSec = 0.25f;
        public float slideCooldownSec = 0.20f;


        private float _accumX;
        private double _windowStartDsp;
        private double _lastSlideDsp;

        void OnEnable() => ResetWindow();

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null ||  judge == null) return;

            // Tap (좌/우 클릭 모두 단일노트)
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
                judge.FireTap(AudioSettings.dspTime);

            // Hold (Wheel)
            if (mouse.middleButton.wasPressedThisFrame)
                judge.FireHoldStart(AudioSettings.dspTime);
            if (mouse.middleButton.wasReleasedThisFrame)
                judge.FireHoldEnd(AudioSettings.dspTime);

            // Slide 방향 감지
            float dx = mouse.delta.ReadValue().x;
            _accumX += dx;
            double nowDsp = AudioSettings.dspTime;

            if (nowDsp - _windowStartDsp > slideWindowSec) ResetWindow();
            if (nowDsp - _lastSlideDsp < slideCooldownSec) return;

            if (Mathf.Abs(_accumX) >= slideThresholdPixels)
            {
                var dir = _accumX > 0 ? sildedir.right : sildedir.left;
                judge.FireSlide(nowDsp, dir);
                _lastSlideDsp = nowDsp;
                ResetWindow();
            }
        }

        private void ResetWindow()
        {
            _accumX = 0f;
            _windowStartDsp = AudioSettings.dspTime;
        }
    }
}
