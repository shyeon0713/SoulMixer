using UnityEngine;
using System;
using System.Collections.Generic;

public enum HitGrade { Miss, Good, Great, Perfect }

public class Judge : MonoBehaviour
{
    //private int _nextIndex;

    [Header("Ref")]
    public Conductor conductor;

    [Header("판정 시간의 구간의 폭 -> 절대값활용")]
    public int perfectsMs = 100;
    public int greatMs = 200;
    public int goodMs = 300;



    private NoteData[] _notes;
    private HashSet<int> _consumedNotes = new HashSet<int>(); // 소비된 노트 추적
    private int _idx;


    public Action<NoteData, HitGrade> OnHit;
    public Action<NoteData> OnMiss;


    public Action OnAllNotesJudged;   // 모든 노트 판정 끝났을 때 호출



    // 새로운 곡 데이터를 로드할 때만 호출 (곡 선택 시)
    public void LoadChart(NoteData[] notes)
    {
        if (notes == null)
        {
            _notes = Array.Empty<NoteData>();
            return;
        }
        // 시간순으로 정렬
        _notes = new NoteData[notes.Length];
         Array.Copy(notes, _notes, notes.Length);
         Array.Sort(_notes, (a, b) => a.Timesec.CompareTo(b.Timesec));

    }

    // PlayUI 활성화/게임 시작 시 호출 (같은 곡 재시작 포함)
    public void ResetJudgment()
    {
        _idx = 0;
        _consumedNotes.Clear();

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

    #region - 키보드 입력 
    public void OnKeyHit(NoteKey key, double inputDspTime)
    {
        if (_notes == null || _notes.Length == 0 || conductor == null)
            return;

        // DSP 기준 현재 시간(초) 계산
        float t = (float)(inputDspTime - conductor.dspStart) + conductor.userOffsetms / 1000f;
        Debug.Log($"[Judge] OnKeyHit key={key}, t={t}");


        // 현재 시각 기준으로 지나간 노트 정리
        CullPastNotes(t);

        // 실제 판정
        FeedTap(key, t);
    }
    #endregion

    #region - 특정 키를 기준으로 가장 가까운 노트를 찾아 판정
    void FeedTap(NoteKey key, float nowSec)
    {

        int bestIndex = -1;
        float judgmentWindow = MsToSec(goodMs);

        Debug.Log($"[Judge] OnKeyHit key={key}, now={nowSec:F3}");

        // _idx부터 앞으로 보면서 "같은 키" + "시간차가 가장 작은 노트" 찾기
        for (int i = _idx; i < _notes.Length; i++)
        {
            // 이미 소비된 노트는 스킵
            if (_consumedNotes.Contains(i))
                continue;

            // 키가 다르면 스킵 -> 자료형이 맞지 않아 해당 방식으로 수정
            if (!string.Equals(_notes[i].key, key.ToString(), StringComparison.OrdinalIgnoreCase))
                continue;

            float diff = _notes[i].Timesec - nowSec;
            float absDiff = Mathf.Abs(diff);

            // 너무 미래 노트(현재 시각 기준 good 윈도우 밖)는 더 이상 볼 필요 없음
            if (diff > judgmentWindow)
                break;

            Debug.Log(
             $"[Judge]  candidate idx={i}, noteTime={_notes[i].Timesec:F3}, " +
            $"diff={diff:F3}s ({diff * 1000f:F1}ms)");

            // good 윈도우 안에 들어온 "첫 번째" 노트 하나만 선택
            if (absDiff <= judgmentWindow)
            {
                bestIndex = i;
                break;
            }
        }

        // 매칭되는 노트를 못 찾았다면 Miss
        if (bestIndex < 0)
        {
            Debug.Log($"[Judge] MISS(no match) key={key}, now={nowSec:F3}");
            // 어떤 노트와도 매칭 안 된 Miss. 필요하다면 가짜 NoteData 만들어 전달
            OnMiss?.Invoke(new NoteData
            {
                id = -1,
                Timesec = nowSec,
                key = key.ToString()
            });
            return;
        }

        // 판정 등급 계산
        float deltaSec = _notes[bestIndex].Timesec - nowSec;
        float deltaMs = deltaSec * 1000f;
        var grade = GradeFromDelta(deltaSec);

        Debug.Log($"[Judge] key={key}, noteId={_notes[bestIndex].id}, " +
          $"noteTime={_notes[bestIndex].Timesec:F3}, now={nowSec:F3}, " +
          $"delta={deltaSec * 1000f:F1}ms, grade={grade}");


        if (grade == HitGrade.Miss)
        {
            OnMiss?.Invoke(_notes[bestIndex]);
            return;
        }

        // 성공 판정
        OnHit?.Invoke(_notes[bestIndex], grade);

        // 소비 처리
        _consumedNotes.Add(bestIndex);

        // 모든 노트 판정 완료 체크
        CheckAllNotesJudged();
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

    private void Update()
    {
        if (conductor == null || _notes == null || _notes.Length == 0)
            return;

        // 자동 Miss 정리
        CullPastNotes(conductor.NowSec);

        // 자동 Miss만으로도 모든 노트가 소비되면 결과창으로 넘어갈 수 있게
        CheckAllNotesJudged();
    }
}