using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
    public string sprite;
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueLine> dialogues;   //List로 대화 가져오기 -> 추후에 상황보고 Dictionary로 수정
}

public class DialogueLoader : MonoBehaviour
{
    public DialogueData dialoguedata;

    private void Start()
    {
        LoadDialogue("JSON/conversation"); //json파일 가져오기
        foreach (var line in dialoguedata.dialogues)
        {
            // Debug.Log($"{line.speaker}: {line.text}");
        }
    }

    public void LoadDialogue(string file)
    {
        TextAsset dia_json = Resources.Load<TextAsset>("JSON/conversation");
        if (dia_json != null)
        {
            dialoguedata = JsonUtility.FromJson<DialogueData>(dia_json.text);
        }

    }

}