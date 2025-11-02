using UnityEngine;

public class MouseInputAdapter : MonoBehaviour
{
    public Judge judge;
    public Conductor conductor;

    [Header("Slide detect")]
    public float slideThresholdPs = 120f;
    public float slideWindowSec = 0.25f;
    public float slideCooldownSec = 0.20f;

    private float _accumX;
    private double _windowStartDsp;
    private double _lastSlideDsp;

    void OnEnable()
    {
        _accumX = 0f;
        _windowStartDsp = AudioSettings.dspTime;
        _lastSlideDsp = -9999;
    }

    void Update()
    {
        var dsp = AudioSettings.dspTime;

        // ÅÇ ÀÔ·Â
        if (Input.GetMouseButtonDown(0)) judge.TapLeft(dsp);
        if (Input.GetMouseButtonDown(1)) judge.TapRight(dsp);

        // ·Õ³ëÆ®
        if (Input.GetMouseButtonDown(2)) judge.LongNoteStart(dsp);
        if (Input.GetMouseButtonUp(2)) judge.LongNoteEnd(dsp);

        // ½½¶óÀÌµå(ÁÂ/¿ì) °¨Áö 
        float dx = Input.GetAxis("Mouse X");
        _accumX += dx;
        if ((dsp - _windowStartDsp) > slideWindowSec)
        {
            _accumX = 0f;
            _windowStartDsp = dsp;
        }

        if (dsp - _lastSlideDsp > slideCooldownSec)
        {
            if (Mathf.Abs(_accumX) >= slideThresholdPs)
            {
                int dir = _accumX > 0 ? +1 : -1;
                judge.FeedSlide(dir, dsp);
                _accumX = 0f;
                _lastSlideDsp = dsp;
                _windowStartDsp = dsp;
            }
        }
    }
}