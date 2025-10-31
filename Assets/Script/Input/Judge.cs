using UnityEngine;
using System;

public enum HitGrade { Miss, Good, Great, Perfect} //총 4가지 판정으로 구분

public class Judge : MonoBehaviour
{
    [Header("Ref")]
    public Conductor conductor;

    [Header("판정 시간의 구간의 폭 -> 절대값활용")]  //json에 지정해놓은 초와 플레이어가 클릭한 초의 간극을 계산후, 해당 변수와 비교하여 판정 
    public int perfectsMs = 22;
    public int greatMs = 45;
    public int goodMs = 80;

    [Header("롱노트의 판정 시간의 구간읜 폭")]
    public int longStartMs = 80;
    public int longEndMs = 80;

    private NoteData[] _notes;  //현재 진행노트 데이터
    private int _idx;  //현재 진행 포인터

    private bool _holding;
    private NoteData _activeLong;

    public Action<NoteData, HitGrade> OnHit;  //성공 콜백
    public Action<NoteData> OnMiss; // 미스 콜백

    public void LoadChart(NoteData[] notes)
    {
        _notes = notes;
        _idx = 0;
        _holding = false;
    }

    float MsToSec(int ms) => ms * 0.001f;

    HitGrade GradeFromDelta(float dt)
    {
        float adt = Mathf.Abs(dt);
        if (adt<=MsToSec(perfectsMs)) return HitGrade.Perfect;
        if (adt <= MsToSec(greatMs)) return HitGrade.Great;
        if (adt<=MsToSec(goodMs)) return HitGrade.Good;
        return HitGrade.Miss;  // 해당 범위에 없을경우, Miss 판정

    }

    #region 진행 포인터 앞으로 밀기 (지나간 노트 정리)
    void CullPastNotes(float now)
    {
        while (_idx < _notes.Length && _notes[_idx].Timesec < now - MsToSec(goodMs))
        {
            OnMiss?.Invoke(_notes[_idx]);
            _idx++;
        }
    }
    #endregion

    #region - 일반 노트 입력시 판정
    public void TapLeft(double inputDspTime)
    {
        FeedTap(NoteType.NormalNote_L,inputDspTime);
    }

    public void TapRight(double inputDspTime)
    {
        FeedTap(NoteType.NormalNote_R, inputDspTime);
    }

    void FeedTap(NoteType type, double inputDspTime)  //일반노트 -> 마우스 좌, 우 단일 클릭
    {
        float t = (float)(inputDspTime - conductor.dspStart) + conductor.userOffsetms / 1000f;
        float now = conductor.NowSec;
        CullPastNotes(now);

        int best = -1;
        float bestDiff = 999f;

        //(성능상 K=12 정도 권장)
        for (int i = _idx; i < _notes.Length && i < _idx + 16; i++)
        {
            if (_notes[i].type != type) continue;
            float d = Mathf.Abs(_notes[i].Timesec - t);
            if (d < bestDiff) { bestDiff = d; best = i; }
            if (_notes[i].Timesec > t + MsToSec(goodMs)) break;
        }

        if (best < 0 || GradeFromDelta(_notes[best].Timesec - t) == HitGrade.Miss)
        {
            OnMiss?.Invoke(new NoteData { type = type, Timesec = t });
            return;
        }

        var g = GradeFromDelta(_notes[best].Timesec - t);
        OnHit?.Invoke(_notes[best], g);

        // 소비 및 포인터 전진
        if (best == _idx) _idx++;
        // 이미 지난 것들도 정리
        while (_idx < _notes.Length && _notes[_idx].Timesec < t - MsToSec(goodMs)) _idx++;
    }
    #endregion

    #region 롱노트 시작부분
    public void LongNoteStart(double inputDsptime)
    {
        float t = (float)(inputDsptime - conductor.dspStart) + conductor.userOffsetms / 1000f;
        float w = MsToSec(longStartMs);

        int best = -1;
        float bestDiff = 999f;

        for (int i = _idx; i<_notes.Length && i<_idx+24; i++)
        {
            if (_notes[i].type != NoteType.LongNote) continue;
            float d = Mathf.Abs(_notes[i].Timesec - t); 
            if (d < bestDiff) bestDiff = d; best = i;
            if (_notes[i].Timesec > t + w) break;
        }

        if (best < 0 || Mathf.Abs(_notes[best].Timesec - t) > w)
        {
            OnMiss?.Invoke(new NoteData { type = NoteType.LongNote,Timesec = t});
            return;
        }

        _activeLong = _notes[best];
        _holding = true;
        OnHit?.Invoke(_activeLong, GradeFromDelta(_activeLong.Timesec -t));

        if(best == _idx)
        {
            _idx++;
        }

    }
    #endregion

    #region -롱노트 해제부분
    public void LongNoteEnd(double inputDspTime)
    {

    }


    #endregion
}




