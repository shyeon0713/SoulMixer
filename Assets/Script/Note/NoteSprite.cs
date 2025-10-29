using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoteSprite", menuName = "Scriptable Objects/Note Sprite Set")]
public class NoteSprite : ScriptableObject
{
    [Header("��� ��Ʈ/�� ���� ��������Ʈ�� �� ����Ʈ�� ����")]
    public List<Sprite> noteSprites = new();

    [Header("��Ʈ Ÿ�� �� ��������Ʈ �ε��� ���� �� 4����")]
    public int[] indexMap = new int[4] { 0, 1, 2, 3 };

    [Header("�ճ�Ʈ ���� �ε��� (noteSprites ����)")]
    public int longHeadIndex = 5;
    public int longBodyIndex = 6; // 9-slice �Ǵ� Ÿ�ϸ� ����
    public int longTailIndex = 7;


    [Header("��ȿ���� ���� �� ��ü ��������Ʈ(����)")]
    public Sprite fallbackSprite;

    public Sprite GetSprite(NoteType type)
    {
        if (indexMap == null || indexMap.Length < 5 || noteSprites == null || noteSprites.Count == 0)
            return fallbackSprite;

        int idx = indexMap[(int)type];
        return (idx >= 0 && idx < noteSprites.Count) ? noteSprites[idx] : fallbackSprite;
    }

    #region- �ճ�Ʈ
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

        // ���� ����
        longHeadIndex = Mathf.Max(0, longHeadIndex);
        longBodyIndex = Mathf.Max(0, longBodyIndex);
        longTailIndex = Mathf.Max(0, longTailIndex);

        // ���� ����
        int max = (noteSprites != null && noteSprites.Count > 0) ? noteSprites.Count - 1 : 0;
        longHeadIndex = Mathf.Min(longHeadIndex, max);
        longBodyIndex = Mathf.Min(longBodyIndex, max);
        longTailIndex = Mathf.Min(longTailIndex, max);
    }
#endif
}