using UnityEngine;
using System;

public enum HitGrade { Miss, Good, Great, Perfect} //�� 4���� �������� ����

public class Judge : MonoBehaviour
{
    [Header("Ref")]
    public Conductor conductor;

    [Header("���� �ð��� ������ �� -> ���밪Ȱ��")]  //json�� �����س��� �ʿ� �÷��̾ Ŭ���� ���� ������ �����, �ش� ������ ���Ͽ� ���� 
    public int perfectsMs = 22;
    public int greatMs = 45;
    public int goodMs = 80;

    [Header("�ճ�Ʈ�� ���� �ð��� ������ ��")]
    public int longStartMs = 80;
    public int longEndMs = 80;

    private NoteData[] _notes;  //���� �����Ʈ ������
    private int _idx;  //���� ���� ������

    private bool _holding;
    private NoteData _activeLong;

    public Action<NoteData, HitGrade> OnHit;  //���� �ݹ�
    public Action<NoteData> OnMiss; // �̽� �ݹ�

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
        return HitGrade.Miss;  // �ش� ������ �������, Miss ����

    }

    #region ���� ������ ������ �б� (������ ��Ʈ ����)
    void CullPastNotes(float now)
    {
        while (_idx < _notes.Length && _notes[_idx].Timesec < now - MsToSec(goodMs))
        {
            OnMiss?.Invoke(_notes[_idx]);
            _idx++;
        }
    }
    #endregion

    #region - �Ϲ� ��Ʈ �Է½� ����
    public void TapLeft(double inputDspTime)
    {
        FeedTap(NoteType.NormalNote_L,inputDspTime);
    }

    public void TapRight(double inputDspTime)
    {
        FeedTap(NoteType.NormalNote_R, inputDspTime);
    }

    void FeedTap(NoteType type, double inputDspTime)  //�Ϲݳ�Ʈ -> ���콺 ��, �� ���� Ŭ��
    {
        float t = (float)(inputDspTime - conductor.dspStart) + conductor.userOffsetms / 1000f;
        float now = conductor.NowSec;
        CullPastNotes(now);

        int best = -1;
        float bestDiff = 999f;

        //(���ɻ� K=12 ���� ����)
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

        // �Һ� �� ������ ����
        if (best == _idx) _idx++;
        // �̹� ���� �͵鵵 ����
        while (_idx < _notes.Length && _notes[_idx].Timesec < t - MsToSec(goodMs)) _idx++;
    }
    #endregion

    #region �ճ�Ʈ ���ۺκ�
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

    #region -�ճ�Ʈ �����κ�
    public void LongNoteEnd(double inputDspTime)
    {

    }


    #endregion
}




