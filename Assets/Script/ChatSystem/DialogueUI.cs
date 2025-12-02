using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SpeakerSpriteSet
{
    public string speakerName;
    public List<Sprite> sprites;
    public List<string> spriteNames;   // sprites 와 동일한 순서
}

public class DialogueUI : MonoBehaviour
{

    // 튜토리얼은 NPC1명 , normal , hard 설명은 NPC2명으로
    public Button nextscript;

    public DialogueLoader dialogueLoader;   // 대사 가져오기

    [Header("Text UI 부분")]
    public TMP_Text npcnametext;
    public TMP_Text dialogueText;

    [Header("NPC 이미지 -> 하나의 Dictionary로 관리")]
    public List<Image> npcImages;
    public List<string> npcNames;

    public List<SpeakerSpriteSet> spriteSets; // 각 NPC별 스프라이트 묶음 리스트


    public RectTransform leftSlot;
    public RectTransform rightSlot;

    private Dictionary<string, Image> imageBank = new();
    private Dictionary<string, Dictionary<string, Sprite>> spriteBank = new();

    private float gray = 111f / 225f;
    private float typingDelay = 0.05f;

    private int currentLine = 0;
    private Coroutine typingCoroutine; // 타이핑 효과를 담당하는 코루틴

    void Start()
    {

        dialogueLoader.LoadScenario("conversation");
        nextscript.onClick.AddListener(NextLine); // 버튼 리스너 연결

        BuildImageBank();
        BuildSpriteBank();

        if (dialogueLoader.dialoguedata.dialogues.Count > 0)
            OutputDialogue(currentLine);


    }

    private void BuildImageBank()
    {
        for (int i = 0; i < npcImages.Count; i++)
            imageBank[npcNames[i]] = npcImages[i];
    }

    private void BuildSpriteBank()
    {
        foreach (var set in spriteSets)
        {
            var dict = new Dictionary<string, Sprite>();

            for (int i = 0; i < set.spriteNames.Count; i++)
                dict[set.spriteNames[i]] = set.sprites[i];

            spriteBank[set.speakerName] = dict;
        }
    }

    void NextLine()  // 버튼 리스너
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogueLoader.dialoguedata.dialogues[currentLine].text;
            typingCoroutine = null;
            return;
        }

        currentLine++;

        if (currentLine >= dialogueLoader.dialoguedata.dialogues.Count)
        {
            nextscript.interactable = false;
            return;
        }

        OutputDialogue(currentLine);
    }

    void OutputDialogue(int index)
    {
        var line = dialogueLoader.dialoguedata.dialogues[index];

        npcnametext.text = line.speaker;
        ResetAllImagesToWhite();

        ApplyPosition(line.speaker, line.position);

        foreach (var name in imageBank.Keys)
        {
            if (name != line.speaker)
                imageBank[name].color = new Color(gray, gray, gray, 1f);
        }

        if (spriteBank.ContainsKey(line.speaker) &&
            spriteBank[line.speaker].ContainsKey(line.sprite))
        {
            imageBank[line.speaker].sprite =
                spriteBank[line.speaker][line.sprite];
        }
        else
        {
            Debug.LogWarning($"스프라이트 '{line.sprite}'를 speaker '{line.speaker}'에서 찾을 수 없습니다.");
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    void ResetAllImagesToWhite()
    {
        foreach (var img in imageBank.Values)
            img.color = Color.white;
    }

    IEnumerator TypeText(string text)
    {
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
           // SoundSetting.Instance.PlaySfx(6);
            yield return new WaitForSeconds(typingDelay);
        }

        typingCoroutine = null;
    }


    #region - 스프라이트 위치결정 -> 앵커 이동?
    void ApplyPosition(string speaker, string pos)
    {
        if (!imageBank.ContainsKey(speaker))
            return;

        Vector2 targetPos = pos == "right"
            ? rightSlot.anchoredPosition
            : leftSlot.anchoredPosition;

        imageBank[speaker].rectTransform.anchoredPosition = targetPos;
    }
    #endregion
}