using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public System.Action OnDialogueComplete; // 대사 출력이 전부 끝났는지 판별

    [Header("Text UI 부분")]
    public TMP_Text npcnametext;
    public TMP_Text dialogueText;

    [Header("NPC 이미지 -> 하나의 Dictionary로 관리")]
    public List<Image> npcImages;
    public List<string> npcNames;

    public List<SpeakerSpriteSet> spriteSets; // 각 NPC별 스프라이트 묶음 리스트

    [Header("이미지 위치")]
    public RectTransform leftSlot;
    public RectTransform rightSlot;
    public RectTransform centerSlot;

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

        OnDialogueComplete += HandleDialogueEnd;

        BuildImageBank();
        BuildSpriteBank();

        // 모든 NPC 이미지 비활성화
        foreach (var img in imageBank.Values)
            img.gameObject.SetActive(false);

        var data = dialogueLoader.dialoguedata;  // 등장할 npc 목록 표시 


        if (data.npcs != null)
        {
            foreach (var npcEntry in data.npcs)
            {
                if (imageBank.TryGetValue(npcEntry.name, out var img))
                {
                    img.gameObject.SetActive(npcEntry.visible);
                }
            }
        }

        // 대사 출력
        if (data.dialogues != null && data.dialogues.Count > 0)
            OutputDialogue(currentLine);


    }

    private void BuildImageBank()
    {
        imageBank.Clear(); // 초기화 

        int count = Mathf.Min(npcImages.Count, npcNames.Count);

        for (int i = 0; i < count; i++)
        {
            var img = npcImages[i];
            var name = npcNames[i];   // line.speaker와 정확히 같은 이름이어야 함

            if (img == null || string.IsNullOrEmpty(name))
                continue;

            // "speaker 이름" → "해당 Image" 매핑
            imageBank[name] = img;
        }
    }

    private void BuildSpriteBank() { 

        spriteBank.Clear(); // 초기화

        foreach (var set in spriteSets)
        {
            var dict = new Dictionary<string, Sprite>();

            int count = Mathf.Min(set.spriteNames.Count, set.sprites.Count);
            for (int i = 0; i < count; i++)
            {
                string faceName = set.spriteNames[i]; // "happy", "sad" 등
                Sprite sp = set.sprites[i];

                if (string.IsNullOrEmpty(faceName) || sp == null)
                    continue;

                dict[faceName] = sp;
            }

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

            OnDialogueComplete?.Invoke();
            return;
        }

        OutputDialogue(currentLine);
    }

    // 대화 화면 구성하는 UI출력
    void OutputDialogue(int index)
    {
        var line = dialogueLoader.dialoguedata.dialogues[index];

        npcnametext.text = line.speaker;

        bool isPlayerLine = (line.speaker == "Player");  // 플레이어 대사 기준

        if (isPlayerLine)  // 플레이어가 대사를 할때 NPC전부 회색처리
        {
            SetAllImagesGray();
        }
        else
        {
            // 화자 NPC가 비활성 상태라면 이때 켜기
            if (imageBank.TryGetValue(line.speaker, out var img))
            {
                if (!img.gameObject.activeSelf)
                    img.gameObject.SetActive(true);
            }

            ResetAllImagesToWhite();  // 생성된 이미지 전부 흰색으로 설정

            ApplyPosition(line.speaker, line.position);  // 위치 배치

            foreach (var kvp in imageBank)
            {
                if (kvp.Key != line.speaker)
                    kvp.Value.color = new Color(gray, gray, gray, 1f);
            }

            if (spriteBank.ContainsKey(line.speaker) &&
                spriteBank[line.speaker].ContainsKey(line.sprite))
            {
                imageBank[line.speaker].sprite =
                    spriteBank[line.speaker][line.sprite];
            }
            else
            {
                Debug.LogWarning(
                    $"스프라이트 '{line.sprite}'를 speaker '{line.speaker}'에서 찾을 수 없습니다.");
            }
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text));
    }
    #region - 스프라이트 초기화
    // 모든 스프라이트를 하얀색으로 초기화
    void ResetAllImagesToWhite()  
    {
        foreach (var img in imageBank.Values)
            img.color = Color.white;
    }
    // 모든 스프라이트를 회색으로 초기화
    void SetAllImagesGray()
    {
        foreach (var img in imageBank.Values)
            img.color = new Color(gray, gray, gray, 1f);
    }

    #endregion 
    // 대사가 타이핑 형식으로 출력하는 코루틴 메서드
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


    #region - 스프라이트 위치결정 -> 앵커 이동
    void ApplyPosition(string speaker, string pos)
    {
        if (!imageBank.ContainsKey(speaker))
            return;

        Vector2 targetPos = pos switch   // switch 표현식 - C# 8 이상
        {
            "right" => rightSlot.anchoredPosition,
            "left" => leftSlot.anchoredPosition,
            _ => centerSlot.anchoredPosition,
        };

            imageBank[speaker].rectTransform.anchoredPosition = targetPos;
    }
    #endregion


    void HandleDialogueEnd()
    {
        var data = dialogueLoader.dialoguedata;

        if (!string.IsNullOrEmpty(data.nextSong))
        {
            GameEntry gameEntry = FindObjectOfType<GameEntry>();
            gameEntry.SelectSong(data.nextSong, data.nextDifficulty);

            SceneManager.LoadScene("RhythmScene");
        }
        else
        {
            Debug.Log("다음 곡 정보가 없어 스킵됩니다.");
        }
    }
}