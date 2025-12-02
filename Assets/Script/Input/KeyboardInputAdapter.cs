using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInputAdapter : MonoBehaviour
{
    public Judge judge;
    public Conductor conductor;

    private PlayInput _input;

    private void Awake()
    {
        _input = new PlayInput(); // Player.inputactions 에서 자동 생성된 C# 클래스

        _input.Player.Hit.performed += OnHit;
    }

    private void OnEnable()
    {
        // 액션맵 활성화
        _input.Player.Enable();
    }

    private void OnDisable()
    {
        // 액션맵 비활성화 + 콜백 해제
        _input.Player.Disable();
        _input.Player.Hit.performed -= OnHit;
    }

    private void OnHit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        double dsp = AudioSettings.dspTime;

        // 어떤 키가 눌렸는지
        string keyName = ctx.control.displayName;   // "W", "A", "1" 등

        // NoteKey enum으로 변환 (네가 만든 enum 기준)
        NoteKey key = ParseKey(keyName);

        // Judge 쪽에 키 입력 전달 (Judge에 OnKeyHit 메서드 추가해둔 기준)
        judge.OnKeyHit(key, dsp);
    }

    private NoteKey ParseKey(string keyName)
    {
        switch (keyName)
        {
            case "W": return NoteKey.W;
            case "A": return NoteKey.A;
            case "S": return NoteKey.S;
            case "D": return NoteKey.D;

            case "UpArrow": return NoteKey.Up;
            case "DownArrow": return NoteKey.Down;
            case "LeftArrow": return NoteKey.Left;
            case "RightArrow": return NoteKey.Right;

            case "1": return NoteKey.Num1;
            case "2": return NoteKey.Num2;
            case "3": return NoteKey.Num3;
            case "4": return NoteKey.Num4;

            default: return NoteKey.W;
        }
    }
}

