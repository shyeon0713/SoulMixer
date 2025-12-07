using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScenarioInfo
{
    public string scenarioId;        // 고유 ID (예: "story_intro", "tutorial_basic")
    public TextAsset jsonFile;       // JSON 파일 직접 할당
    [TextArea]
    public string description;       // 설명 (에디터용)
}

public class ScenarioManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static ScenarioManager _instance;
    public static ScenarioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ScenarioManager>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject("ScenarioManager");
                    _instance = obj.AddComponent<ScenarioManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    [Header("시나리오 리스트")]
    public List<ScenarioInfo> scenarios = new List<ScenarioInfo>();

    [Header("현재 진행 상태")]
    public int currentScenarioIndex = 0;  // 현재 진행 중인 시나리오 인덱스

    private Dictionary<string, ScenarioInfo> _scenarioDict = new Dictionary<string, ScenarioInfo>();
    private DialogueLoader _dialogueLoader;

    void Awake()
    {
        // 싱글톤 설정
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeScenarios();
    }

    // 시나리오 딕셔너리 초기화
    private void InitializeScenarios()
    {
        _scenarioDict.Clear();

        foreach (var scenario in scenarios)
        {
            if (!string.IsNullOrEmpty(scenario.scenarioId))
            {
                _scenarioDict[scenario.scenarioId] = scenario;
            }
        }

        Debug.Log($"[ScenarioManager] {scenarios.Count}개의 시나리오 로드됨");
    }

    // ID로 시나리오 정보 가져오기
    public ScenarioInfo GetScenarioById(string scenarioId)
    {
        if (_scenarioDict.TryGetValue(scenarioId, out ScenarioInfo info))
        {
            return info;
        }

        Debug.LogWarning($"[ScenarioManager] 시나리오를 찾을 수 없음: {scenarioId}");
        return null;
    }

    // 인덱스로 시나리오 정보 가져오기
    public ScenarioInfo GetScenarioByIndex(int index)
    {
        if (index >= 0 && index < scenarios.Count)
        {
            return scenarios[index];
        }

        Debug.LogWarning($"[ScenarioManager] 잘못된 인덱스: {index}");
        return null;
    }

    // DialogueLoader에 시나리오 로드 요청
    public DialogueData LoadScenario(string scenarioId, DialogueLoader loader)
    {
        var scenarioInfo = GetScenarioById(scenarioId);

        if (scenarioInfo == null)
        {
            Debug.LogError($"[ScenarioManager] 시나리오를 찾을 수 없음: {scenarioId}");
            return null;
        }

        if (scenarioInfo.jsonFile == null)
        {
            Debug.LogError($"[ScenarioManager] JSON 파일이 할당되지 않음: {scenarioId}");
            return null;
        }

        return loader.LoadFromTextAsset(scenarioInfo.jsonFile);
    }

    // 다음 시나리오로 진행
    public bool MoveToNextScenario()
    {
        if (currentScenarioIndex + 1 < scenarios.Count)
        {
            currentScenarioIndex++;
            Debug.Log($"[ScenarioManager] 다음 시나리오로 이동: {currentScenarioIndex}");
            return true;
        }

        Debug.Log("[ScenarioManager] 모든 시나리오 완료!");
        return false;
    }

    // 현재 시나리오 가져오기
    public ScenarioInfo GetCurrentScenario()
    {
        return GetScenarioByIndex(currentScenarioIndex);
    }

    // 특정 시나리오로 이동
    public void SetCurrentScenario(string scenarioId)
    {
        for (int i = 0; i < scenarios.Count; i++)
        {
            if (scenarios[i].scenarioId == scenarioId)
            {
                currentScenarioIndex = i;
                Debug.Log($"[ScenarioManager] 시나리오 설정: {scenarioId} (인덱스 {i})");
                return;
            }
        }

        Debug.LogWarning($"[ScenarioManager] 시나리오를 찾을 수 없음: {scenarioId}");
    }

    // 진행 상태 초기화
    public void ResetProgress()
    {
        currentScenarioIndex = 0;
        Debug.Log("[ScenarioManager] 진행 상태 초기화");
    }

    // 진행 상태 저장 (PlayerPrefs 사용)
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("ScenarioProgress", currentScenarioIndex);
        PlayerPrefs.Save();
        Debug.Log($"[ScenarioManager] 진행 상태 저장: {currentScenarioIndex}");
    }

    // 진행 상태 불러오기
    public void LoadProgress()
    {
        if (PlayerPrefs.HasKey("ScenarioProgress"))
        {
            currentScenarioIndex = PlayerPrefs.GetInt("ScenarioProgress");
            Debug.Log($"[ScenarioManager] 진행 상태 로드: {currentScenarioIndex}");
        }
        else
        {
            Debug.Log("[ScenarioManager] 저장된 진행 상태 없음");
        }
    }

    // 모든 시나리오 ID 목록 가져오기
    public List<string> GetAllScenarioIds()
    {
        List<string> ids = new List<string>();
        foreach (var scenario in scenarios)
        {
            ids.Add(scenario.scenarioId);
        }
        return ids;
    }

    // 시나리오 개수
    public int GetScenarioCount()
    {
        return scenarios.Count;
    }

    // 현재 진행률 (%)
    public float GetProgressPercentage()
    {
        if (scenarios.Count == 0) return 0f;
        return (currentScenarioIndex / (float)scenarios.Count) * 100f;
    }
}