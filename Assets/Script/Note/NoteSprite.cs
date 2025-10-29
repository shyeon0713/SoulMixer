using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoteSprite", menuName = "Scriptable Objects/Note Sprite Set")]
public class NoteSprite : ScriptableObject
{
    [Header("모든 노트/롱 파츠 스프라이트를 한 리스트에 관리")]
    public List<Sprite> noteSprites = new();

    [Header("노트 타입 → 스프라이트 인덱스 매핑 총 4가지")]
    public int[] indexMap = new int[4] { 0, 1, 2, 3 };

    [Header("롱노트 파츠 인덱스 (noteSprites 기준)")]
    public int longHeadIndex = 5;
    public int longBodyIndex = 6; // 9-slice 또는 타일링 권장
    public int longTailIndex = 7;


    [Header("유효하지 않을 때 대체 스프라이트(선택)")]
    public Sprite fallbackSprite;

    public Sprite GetSprite(NoteType type)
    {
        if (indexMap == null || indexMap.Length < 5 || noteSprites == null || noteSprites.Count == 0)
            return fallbackSprite;

        int idx = indexMap[(int)type];
        return (idx >= 0 && idx < noteSprites.Count) ? noteSprites[idx] : fallbackSprite;
    }

    #region- 롱노트
    public Sprite GetLongHead() => GetAt(longHeadIndex);
    public Sprite GetLongBody() => GetAt(longBodyIndex);
    public Sprite GetLongTail() => GetAt(longTailIndex);


    private Sprite GetAt(int i)
    {
        if (noteSprites == null || noteSprites.Count == 0) return fallbackSprite;
        return (i >= 0 && i < noteSprites.Count) ? noteSprites[i] : fallbackSprite;
    }
    #endregion

#if UNITY_EDITOR
    private void UpdateNote()
    {
        
        if (indexMap == null || indexMap.Length != 4)
        {
            var old = indexMap;
            indexMap = new int[4] { 0, 1, 2, 3};
            if (old != null)
            {
                for (int i = 0; i < Mathf.Min(old.Length, 5); i++)
                    indexMap[i] = old[i];
            }
        }

        // 음수 방지
        longHeadIndex = Mathf.Max(0, longHeadIndex);
        longBodyIndex = Mathf.Max(0, longBodyIndex);
        longTailIndex = Mathf.Max(0, longTailIndex);

        // 상한 보정
        int max = (noteSprites != null && noteSprites.Count > 0) ? noteSprites.Count - 1 : 0;
        longHeadIndex = Mathf.Min(longHeadIndex, max);
        longBodyIndex = Mathf.Min(longBodyIndex, max);
        longTailIndex = Mathf.Min(longTailIndex, max);
    }
#endif
}