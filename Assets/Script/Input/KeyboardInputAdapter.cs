using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInputAdapter : MonoBehaviour
{
    public Judge judge;
    public Conductor conductor;

    private PlayInput _input;

    private void Awake()
    {
        // 한 번만 생성
        _input = new PlayInput();

        // 액션맵 전체 활성화 (혹은 _input.Player.Enable() 만 써도 됨)
        _input.Enable();

        // 콜백 등록
        _input.Player.Hit.performed += OnHit;

        Debug.Log("[KeyboardInputAdapter] Awake -> PlayInput 생성 및 Enable 완료");

        // 이 오브젝트를 완전히 전역 입력 매니저로 쓴다면:
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Player.Hit.performed -= OnHit;
            _input.Disable();
            _input = null;
        }

        Debug.Log("[KeyboardInputAdapter] OnDestroy -> PlayInput Disable 및 정리");
    }

    // 굳이 액션맵을 끄지 말고, 상태 로그만 남겨두자
    private void OnEnable()
    {
        Debug.Log("[KeyboardInputAdapter] OnEnable (입력은 이미 Enable 상태 유지)");
    }

    private void OnDisable()
    {
        Debug.Log("[KeyboardInputAdapter] OnDisable (더 이상 Player.Disable() 호출하지 않음)");
    }

    private void OnHit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (judge == null || conductor == null) return;

        double dsp = AudioSettings.dspTime;

        string keyName = ctx.control.displayName;
        NoteKey key = ParseKey(keyName);

        Debug.Log($"[KeyboardInputAdapter] OnHit 호출됨, key={key}, phase={ctx.phase}, control={keyName}");

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


