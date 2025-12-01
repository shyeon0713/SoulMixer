using UnityEngine;

public class MouseInputAdapter : MonoBehaviour
{
    public Judge judge;
    public Conductor conductor;

    [Header("Slide detect")]
    public float slideThresholdPs = 10f;
    public float slideWindowSec = 0.5f;
    public float slideCooldownSec = 0.20f;

    private float _accumX;
    private double _windowStartDsp;
    private double _lastSlideDsp;


    [Header("애니메이션 위치")]
    public Transform animpos;

    void OnEnable()
    {
        _accumX = 0f;
        _windowStartDsp = AudioSettings.dspTime;
        _lastSlideDsp = -9999;
    }

    void Update()
    {
        var dsp = AudioSettings.dspTime;

        //입력 확인
        if (Input.GetMouseButtonDown(2)) Debug.Log(" 휠 버튼 눌림!");
        float testDx = Input.GetAxis("Mouse X");
        if (testDx != 0) Debug.Log($"↔? 마우스 이동중: {testDx}");
        //

        // 탭 입력
        if (Input.GetMouseButtonDown(0))
        {
            judge.TapLeft(dsp);
            PlayAnim("Shake");
        }
        if (Input.GetMouseButtonDown(1))
        {
            judge.TapRight(dsp);
            PlayAnim("Shake");
        }

        // 롱노트 -> 휠버튼 누르는거 유지
        if (Input.GetMouseButtonDown(2))
        {
            judge.LongNoteStart(dsp);
            PlayAnim("Stir");  // 롱노트 애니메이션
        }
        
        if (Input.GetMouseButtonUp(2)) judge.LongNoteEnd(dsp);

        // 슬라이드(좌/우) 감지 
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

                PlayAnim("Pour");

                _accumX = 0f;
                _lastSlideDsp = dsp;
                _windowStartDsp = dsp;
            }
        }
    }


    void PlayAnim(string name)
    {
        // 위치(Glass)가 연결되어 있고, 매니저가 존재할 때만 실행
        if (animpos != null && AnimationManager.instance != null)
        {
            AnimationManager.instance.PlayAnimation(name, animpos.position);
        }
    }
}