using UnityEngine;
using System;
using System.Collections.Generic;

public enum HitGrade { Miss, Good, Great, Perfect }

public class Judge : MonoBehaviour
{
    private int _nextIndex;

    [Header("Ref")]
    public Conductor conductor;

    [Header("판정 시간의 구간의 폭 -> 절대값활용")]
    public int perfectsMs = 100;
    public int greatMs = 200;
    public int goodMs = 300;

    [Header("롱노트의 판정 시간의 구간읜 폭")]
    public int longStartMs = 80;
    public int longEndMs = 80;

    private NoteData[] _notes;
    private HashSet<int> _consumedNotes = new HashSet<int>(); // 소비된 노트 추적
    private int _idx;

    private bool _holding;
    private NoteData _activeLong;
    private int _activeLongIndex = -1;

    public Action<NoteData, HitGrade> OnHit;
    public Action<NoteData> OnMiss;


    public Action OnAllNotesJudged;   // 모든 노트 판정 끝났을 때 호출

    // 새로운 곡 데이터를 로드할 때만 호출 (곡 선택 시)
    public void LoadChart(NoteData[] notes)
    {
        if (notes == null)
        {
       //     Debug.LogError("[Judge] LoadChart: notes == null");
            return;
        }

        // 시간순으로 정렬
        _notes = new NoteData[notes.Length];
        System.Array.Copy(notes, _notes, notes.Length);
        System.Array.Sort(_notes, (a, b) => a.Timesec.CompareTo(b.Timesec));

     //   Debug.Log($"[Judge] LoadChart: {_notes.Length} notes loaded and sorted by time");

        // 첫 5개 노트 시간 확인 (디버깅용)
        for (int i = 0; i < Mathf.Min(5, _notes.Length); i++)
        {
            Debug.Log($"  Note[{i}]: {_notes[i].Timesec:F3}s - {_notes[i].type}");
        }
    }

    // PlayUI 활성화/게임 시작 시 호출 (같은 곡 재시작 포함)
    public void ResetJudgment()
    {
        _idx = 0;
        _holding = false;
        _activeLong = default;
        _activeLongIndex = -1;
        _consumedNotes.Clear();

      //  Debug.Log("[Judge] Judgment reset - ready to play");
    }

    float MsToSec(int ms) => ms * 0.001f;

    HitGrade GradeFromDelta(float dt)
    {
        float adt = Mathf.Abs(dt);
        if (adt <= MsToSec(perfectsMs)) return HitGrade.Perfect;
        if (adt <= MsToSec(greatMs)) return HitGrade.Great;
        if (adt <= MsToSec(goodMs)) return HitGrade.Good;
        return HitGrade.Miss;
    }

    #region 진행 포인터 앞으로 밀기 (지나간 노트 정리)
    void CullPastNotes(float now)
    {
        if (_notes == null || _notes.Length == 0)
            return;

        while (_idx < _notes.Length)
        {
            // 이미 소비된 노트는 스킵
            if (_consumedNotes.Contains(_idx))
            {
                _idx++;
                continue;
            }

            // 판정 가능 시간을 벗어난 노트만 Miss 처리
            if (_notes[_idx].Timesec < now - MsToSec(goodMs))
            {
                // 롱노트가 활성화 중이면 스킵
                if (_holding && _idx == _activeLongIndex)
                {
                    _idx++;
                    continue;
                }

            //    Debug.Log($"[Judge] Miss - Note at {_notes[_idx].Timesec:F3}s, current: {now:F3}s");
                OnMiss?.Invoke(_notes[_idx]);
                _consumedNotes.Add(_idx);
                _idx++;
            }
            else
            {
                break;
            }
        }
    }
    #endregion

    #region - 일반 노트 입력시 판정
    public void TapLeft(double inputDspTime)
    {
        FeedTap(NoteType.NormalNote_L, inputDspTime);
    }

    public void TapRight(double inputDspTime)
    {
        FeedTap(NoteType.NormalNote_R, inputDspTime);
    }

    void FeedTap(NoteType type, double inputDspTime)
    {
        if (_notes == null || _notes.Length == 0 || conductor == null)
            return;

        float t = (float)(inputDspTime - conductor.dspStart) + conductor.userOffsetms / 1000f;

        // 디버그: 입력 시간 확인
      //  Debug.Log($"[Judge] Tap {type} at t={t:F3}s (input={inputDspTime:F3}, dspStart={conductor.dspStart:F3})");

        // ?? 수정: t를 기준으로 정리
        CullPastNotes(t);

        int best = -1;
        float bestDiff = 999f;
        float judgmentWindow = MsToSec(goodMs);

        // ?? 수정: 범위 내 모든 노트 검색
        for (int i = _idx; i < _notes.Length; i++)
        {
            // 이미 소비된 노트는 스킵
            if (_consumedNotes.Contains(i))
                continue;

            // 타입이 다르면 스킵
            if (_notes[i].type != type)
                continue;

            float diff = _notes[i].Timesec - t;
            float absDiff = Mathf.Abs(diff);

            // 판정 범위를 벗어난 미래 노트는 종료
            if (diff > judgmentWindow)
                break;

            // 가장 가까운 노트 찾기
            if (absDiff < bestDiff)
            {
                bestDiff = absDiff;
                best = i;
            }
        }

        // 판정 처리
        if (best < 0)
        {
          //  Debug.Log($"[Judge] No note found for tap at {t:F3}s");
            OnMiss?.Invoke(new NoteData { type = type, Timesec = t });
            return;
        }

        var grade = GradeFromDelta(_notes[best].Timesec - t);
        float deltaMs = (_notes[best].Timesec - t) * 1000f;

        if (grade == HitGrade.Miss)
        {
          //  Debug.Log($"[Judge] Miss - Note[{best}] at {_notes[best].Timesec:F3}s, Delta: {deltaMs:F1}ms (threshold: {goodMs}ms)");
            OnMiss?.Invoke(_notes[best]);
            return;
        }

        //  수정: 성공 판정 시 노트 소비
       // Debug.Log($"[Judge] {grade} - Note[{best}] at {_notes[best].Timesec:F3}s, Delta: {deltaMs:F1}ms");
        OnHit?.Invoke(_notes[best], grade);
        _consumedNotes.Add(best);
    }
    #endregion

    #region 롱노트 시작부분
    public void LongNoteStart(double inputDsptime)
    {
        if (_notes == null || _notes.Length == 0 || conductor == null)
            return;

        float t = (float)(inputDsptime - conductor.dspStart) + conductor.userOffsetms / 1000f;
        float w = MsToSec(longStartMs);

        int best = -1;
        float bestDiff = 999f;

        for (int i = _idx; i < _notes.Length; i++)
        {
            if (_consumedNotes.Contains(i))
                continue;

            if (_notes[i].type != NoteType.LongNote)
                continue;

            float diff = _notes[i].Timesec - t;
            float absDiff = Mathf.Abs(diff);

            if (diff > w)
                break;

            if (absDiff < bestDiff)
            {
                bestDiff = absDiff;
                best = i;
            }
        }

        if (best < 0 || bestDiff > w)
        {
       //    Debug.Log($"[Judge] Long note start miss at {t:F3}s");
            OnMiss?.Invoke(new NoteData { type = NoteType.LongNote, Timesec = t });
            return;
        }

        _activeLong = _notes[best];
        _activeLongIndex = best;
        _holding = true;

        var grade = GradeFromDelta(_activeLong.Timesec - t);
      //  Debug.Log($"[Judge] Long note start {grade} at {t:F3}s");
        OnHit?.Invoke(_activeLong, grade);
        _consumedNotes.Add(best);
    }
    #endregion

    #region -롱노트 해제부분
    public void LongNoteEnd(double inputDspTime)
    {
        if (!_holding || _notes == null || conductor == null)
            return;

        float t = (float)(inputDspTime - conductor.dspStart) + conductor.userOffsetms / 1000f;
        float endTime = _activeLong.Timesec + _activeLong.durationSec;
        float w = MsToSec(longEndMs);

        if (Mathf.Abs(endTime - t) <= w)
        {
            var grade = GradeFromDelta(endTime - t);
         //   Debug.Log($"[Judge] Long note end {grade}, delta: {(endTime - t) * 1000:F1}ms");
            OnHit?.Invoke(_activeLong, grade);
        }
        else
        {
        //    Debug.Log($"[Judge] Long note end miss, delta: {(endTime - t) * 1000:F1}ms");
            OnMiss?.Invoke(_activeLong);
        }

        _holding = false;
        _activeLongIndex = -1;
    }
    #endregion

    #region 슬라이드
    public void FeedSlide(int dir, double inputDspTime)
    {
        if (_notes == null || _notes.Length == 0 || conductor == null)
            return;

        var targetType = (dir < 0) ? NoteType.SlideNote_L : NoteType.SlideNote_R;
        float t = (float)(inputDspTime - conductor.dspStart) + conductor.userOffsetms / 1000f;

        CullPastNotes(t);

        int best = -1;
        float bestDiff = 999f;
        float judgmentWindow = MsToSec(goodMs);

        for (int i = _idx; i < _notes.Length; i++)
        {
            if (_consumedNotes.Contains(i))
                continue;

            if (_notes[i].type != targetType)
                continue;

            float diff = _notes[i].Timesec - t;
            float absDiff = Mathf.Abs(diff);

            if (diff > judgmentWindow)
                break;

            if (absDiff < bestDiff)
            {
                bestDiff = absDiff;
                best = i;
            }
        }

        if (best < 0)
        {
         //   Debug.Log($"[Judge] Slide miss - no note found at {t:F3}s");
            OnMiss?.Invoke(new NoteData { type = targetType, Timesec = t });
            return;
        }

        var grade = GradeFromDelta(_notes[best].Timesec - t);

        if (grade == HitGrade.Miss)
        {
            OnMiss?.Invoke(_notes[best]);
            return;
        }

     //   Debug.Log($"[Judge] Slide {grade} at {t:F3}s");
        OnHit?.Invoke(_notes[best], grade);
        _consumedNotes.Add(best);
    }
    #endregion

    #region - 모든 노트판정이 끝난 뒤, GameEntry에 모든 판정이 끝났음을 전달 
    private void CheckAllNotesJudged()
    {
        if (_notes == null) return;


        //json에서 더이상 가져올 노트가 없을 경우 , 확인
        if (_consumedNotes.Count >= _notes.Length)
        {
            Debug.Log("[Judge] All notes judged!");
            OnAllNotesJudged?.Invoke();
        }
    }

    #endregion
}