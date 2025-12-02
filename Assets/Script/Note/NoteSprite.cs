using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoteSprite", menuName = "Scriptable Objects/Note Sprite Set")]
public class NoteSprite : ScriptableObject
{
    [Header("입력 키별 노트 스프라이트")]
    [Tooltip("인덱스 순서: 0=W, 1=A, 2=S, 3=D, 4=Up, 5=Down, 6=Left, 7=Right, 8= 1, 9= 2 ...")]
    public Sprite[] keySprites = new Sprite[12];

    [Header("유효하지 않을 때 대체 스프라이트(선택)")]
    public Sprite fallbackSprite;

    /// NoteKey enum 기준으로 스프라이트 반환
    public Sprite GetSprite(NoteKey key)
    {
        int idx = (int)key;

        if (keySprites == null || idx < 0 || idx >= keySprites.Length)
            return fallbackSprite;

        var sp = keySprites[idx];
        return sp != null ? sp : fallbackSprite;
    }


    /// JSON에서 온 string key(W, A, S, D, Up, Down, Left, Right, Num 1 ~6)를 받아서 스프라이트 반환
   
    public Sprite GetSpriteByKeyString(string keyString)
    {
        if (string.IsNullOrEmpty(keyString))
            return fallbackSprite;

        // 문자열을 NoteKey enum으로 변환 시도
        if (!System.Enum.TryParse(keyString, ignoreCase: true, out NoteKey keyEnum))
            return fallbackSprite;

        return GetSprite(keyEnum);
    }
}
