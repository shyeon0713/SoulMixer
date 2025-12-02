using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
    public string sprite;
    public string position;  // 스프라이트 위치
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueLine> dialogues;   //List로 대화 가져오기 -> 추후에 상황보고 Dictionary로 수정
}

public class DialogueLoader : MonoBehaviour
{
    public Dictionary<string, DialogueData> scenarioBank = new();
    //시나리오들을 딕셔너리로 만들어서 관리 + 필요할 때 호출

    public DialogueData dialoguedata;
    // 현재 사용중인 대사 데이터

    //시나리오 이름(파일명)으로 JSON을 로드하고 scenarioBank에 캐싱
    public DialogueData LoadScenario(string scenarioName)
    {

        // 이미 로딩한 시나리오가 있다면 딕셔너리에서 즉시 반환
        if (scenarioBank.ContainsKey(scenarioName))
        {
            dialoguedata = scenarioBank[scenarioName];
            return dialoguedata;
        }

        // Resources/JSON/scenarioName.json 파일 로드
        TextAsset jsonFile = Resources.Load<TextAsset>("JSON/" + scenarioName);

        if (jsonFile == null)
        {
            Debug.LogError($"시나리오 파일을 찾을 수 없습니다: JSON/{scenarioName}.json");
            return null;
        }

        // JSON → DialogueData 변환
        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        if (data == null || data.dialogues == null)
        {
            Debug.LogError($"JSON 파싱 실패: {scenarioName}");
            return null;
        }

        scenarioBank[scenarioName] = data;

        // 현재 데이터 설정
        dialoguedata = data;
        return dialoguedata;
    }

}