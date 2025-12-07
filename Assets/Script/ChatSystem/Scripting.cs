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
public class NpcEntry  //화면에 출력될 NPC
{
    public string name;         
    public bool visible;   
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueLine> dialogues;   //List로 대화 가져오기 
    public List<NpcEntry> npcs;

    // 다음 액션 타입: "PlaySong", "NextDialogue", "NextScenario", "End"
    public string nextActionType;

    // PlaySong일 때
    public string nextSong;
    public string nextDifficulty;

    // NextDialogue일 때 (파일명 직접 지정)
    public string nextScenarioId;

    // NextScenario는 ScenarioManager의 순서대로 자동 진행
}

public class DialogueLoader : MonoBehaviour
{
    public Dictionary<string, DialogueData> scenarioBank = new();
    public DialogueData dialoguedata;
    //시나리오들을 딕셔너리로 만들어서 관리 + 필요할 때 호출

    // 신규: TextAsset으로 직접 로드
    public DialogueData LoadFromTextAsset(TextAsset jsonFile)
    {
        if (jsonFile == null)
        {
            Debug.LogError("[DialogueLoader] TextAsset이 null입니다!");
            return null;
        }

        string key = jsonFile.name;

        // 이미 로드한 적 있으면 캐시에서 반환
        if (scenarioBank.ContainsKey(key))
        {
            dialoguedata = scenarioBank[key];
            return dialoguedata;
        }

        Debug.Log($"[DialogueLoader] JSON 파싱: {jsonFile.name}");
        Debug.Log($"[DialogueLoader] JSON 내용:\n{jsonFile.text}");

        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        if (data == null || data.dialogues == null)
        {
            Debug.LogError($"[DialogueLoader] JSON 파싱 실패: {jsonFile.name}");
            return null;
        }

        Debug.Log($"[DialogueLoader] 파싱 성공! 대화 개수: {data.dialogues.Count}");

        scenarioBank[key] = data;
        dialoguedata = data;
        return dialoguedata;
    }

}