using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpeakerSpriteSet
{
    public string speakerName;
    public List<Sprite> sprites;
    public List<string> spriteNames;
}

public class DialogueUI : MonoBehaviour
{
    public Button nextscript;
    public Button nexttutorial;
    private DialogueLoader dialogueLoader;
    public System.Action OnDialogueComplete;

    [Header("Text UI 부분")]
    public TMP_Text npcnametext;
    public TMP_Text dialogueText;

    [Header("튜토리얼 Text UI")]
    public TMP_Text tutorialDialogueText; // 튜토리얼용

    [Header("NPC 이미지")]
    public List<Image> npcImages;
    public List<string> npcNames;
    public List<SpeakerSpriteSet> spriteSets;

    [Header("이미지 위치")]
    public RectTransform leftSlot;
    public RectTransform rightSlot;
    public RectTransform centerSlot;

    [Header("UI 전환")]
    public GameObject dialoguePanel;
    public GameObject gamePlayPanel;
    public GameEntry gameEntry;
    public GameObject tutorialPanel;

    public Button tutorialButton1;
    public Button tutorialButton2;

    private Dictionary<string, Image> imageBank = new();
    private Dictionary<string, Dictionary<string, Sprite>> spriteBank = new();

    private float gray = 111f / 225f;
    private float typingDelay = 0.05f;

    private int currentLine = 0;
    private Coroutine typingCoroutine;


    void Awake()
    {
        // DialogueLoader 찾기 또는 추가
        dialogueLoader = GetComponent<DialogueLoader>();

        if (dialogueLoader == null)
        {
            Debug.LogWarning("[DialogueUI] DialogueLoader가 없어서 자동으로 추가합니다.");
            dialogueLoader = gameObject.AddComponent<DialogueLoader>();
        }
    }


    void Start()
    {
        if (nextscript == null)
        {
            Debug.LogError("[DialogueUI] Next Script Button이 할당되지 않았습니다!");
            return;
        }

        nexttutorial.onClick.AddListener(NextLine);
        nextscript.onClick.AddListener(NextLine);
        OnDialogueComplete += HandleDialogueEnd;


        if (tutorialButton1 != null)
            tutorialButton1.onClick.AddListener(() => StartDialogueByButton("Dialogues2")); 

        if (tutorialButton2 != null)
            tutorialButton2.onClick.AddListener(() => StartTutorialByButton("Tutorial2"));


        BuildImageBank();
        BuildSpriteBank();

        // ★ 초기 UI 상태 설정 (추가!)
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);   // 대화창 켜기

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);  // 튜토리얼 끄기

        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(false);  // 게임 끄기


        // ScenarioManager에서 첫 시나리오 로드
        var firstScenario = ScenarioManager.Instance.GetCurrentScenario();

        if (firstScenario != null)
        {
            Debug.Log($"[DialogueUI] 첫 시나리오 로드: {firstScenario.scenarioId}");
            dialogueLoader.LoadFromTextAsset(firstScenario.jsonFile);
            UpdateDialogueUI();  
        }
        else
        {
            Debug.LogError("[DialogueUI] 시작 시나리오를 찾을 수 없습니다!");
        }
    }

    private void BuildImageBank()
    {
        imageBank.Clear();
        int count = Mathf.Min(npcImages.Count, npcNames.Count);

        for (int i = 0; i < count; i++)
        {
            var img = npcImages[i];
            var name = npcNames[i];

            if (img == null || string.IsNullOrEmpty(name))
                continue;

            imageBank[name] = img;
        }
    }

    private void BuildSpriteBank()
    {
        spriteBank.Clear();

        foreach (var set in spriteSets)
        {
            var dict = new Dictionary<string, Sprite>();
            int count = Mathf.Min(set.spriteNames.Count, set.sprites.Count);

            for (int i = 0; i < count; i++)
            {
                string faceName = set.spriteNames[i];
                Sprite sp = set.sprites[i];

                if (string.IsNullOrEmpty(faceName) || sp == null)
                    continue;

                dict[faceName] = sp;
            }

            spriteBank[set.speakerName] = dict;
        }
    }

    void NextLine()
    {
        bool isTutorialMode = tutorialPanel != null && tutorialPanel.activeSelf;
        TMP_Text targetText = isTutorialMode && tutorialDialogueText != null
            ? tutorialDialogueText
            : dialogueText;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);

            if (targetText != null)
                targetText.text = dialogueLoader.dialoguedata.dialogues[currentLine].text;

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


    void OutputDialogue(int index)
    {
        var line = dialogueLoader.dialoguedata.dialogues[index];

        // 튜토리얼 모드인지 확인
        bool isTutorialMode = tutorialPanel != null && tutorialPanel.activeSelf;

        //튜토리얼이 아닐 때만 speaker 출력
        if (!isTutorialMode && npcnametext != null)
            npcnametext.text = line.speaker;

        bool isPlayerLine = (line.speaker == "Player");

        if (isPlayerLine)
        {
            SetAllImagesGray();
        }
        else
        {
            if (imageBank.TryGetValue(line.speaker, out var img))
            {
                if (!img.gameObject.activeSelf)
                    img.gameObject.SetActive(true);
            }

            ResetAllImagesToWhite();
            ApplyPosition(line.speaker, line.position);

            foreach (var kvp in imageBank)
            {
                if (kvp.Key != line.speaker)
                    kvp.Value.color = new Color(gray, gray, gray, 1f);
            }

            if (spriteBank.ContainsKey(line.speaker) &&
                spriteBank[line.speaker].ContainsKey(line.sprite))
            {
                imageBank[line.speaker].sprite = spriteBank[line.speaker][line.sprite];
            }
            else
            {
                Debug.LogWarning($"스프라이트 '{line.sprite}'를 speaker '{line.speaker}'에서 찾을 수 없습니다.");
            }
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text, isTutorialMode));
    }



    void ResetAllImagesToWhite()
    {
        foreach (var img in imageBank.Values)
            img.color = Color.white;
    }

    void SetAllImagesGray()
    {
        foreach (var img in imageBank.Values)
            img.color = new Color(gray, gray, gray, 1f);
    }

    IEnumerator TypeText(string text, bool isTutorialMode)
    {
    //    Debug.Log($"[DialogueUI] TypeText 시작: isTutorialMode={isTutorialMode}, text={text}");

        // 어느 텍스트 필드를 사용할지 선택
        TMP_Text targetText;

        if (isTutorialMode && tutorialDialogueText != null)
            targetText = tutorialDialogueText;
        else
            targetText = dialogueText;

        if (targetText == null)
        {
            Debug.LogWarning("[DialogueUI] 텍스트 필드가 null입니다!");
            yield break;
        }

        targetText.text = "";

        foreach (char c in text)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingDelay);
        }

        typingCoroutine = null;
    }


    void ApplyPosition(string speaker, string pos)
    {
        if (!imageBank.ContainsKey(speaker))
            return;

        Vector2 targetPos = pos switch
        {
            "right" => rightSlot.anchoredPosition,
            "left" => leftSlot.anchoredPosition,
            _ => centerSlot.anchoredPosition,
        };

        imageBank[speaker].rectTransform.anchoredPosition = targetPos;
    }

    // 대화 종료 시 분기 처리
    void HandleDialogueEnd()
    {
        var data = dialogueLoader.dialoguedata;

        if (string.IsNullOrEmpty(data.nextActionType))
        {
            Debug.Log("[DialogueUI] 다음 액션이 없어 종료됩니다.");
            return;
        }

        Debug.Log($"[DialogueUI] 대화 종료. 다음 액션: {data.nextActionType}");

        switch (data.nextActionType)
        {
            case "PlaySong":
                StartGameInSameScene(data.nextSong, data.nextDifficulty);
                break;

            case "NextDialogue":   //json기반으로 가져오기
                LoadScenarioById(data.nextScenarioId);  
                break;

            case "NextScenario":  // 튜토리얼 매니저 기반으로 가져오기
                LoadNextScenarioInSequence();
                break;

            case "End":
                EndDialogue();
                break;

            case "NextTutorial":  //json기반으로 가져오기
                LoadTutorialById(data.nextScenarioId);
                break;

            default:
                Debug.LogWarning($"[DialogueUI] 알 수 없는 액션 타입: {data.nextActionType}");
                break;
        }
    }

    // 게임 플레이 시작
    private void StartGameInSameScene(string songId, string difficulty)
    {
        Debug.Log($"[DialogueUI] 같은 씬에서 게임 시작: {songId} ({difficulty})");

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("[DialogueUI] 대화창 비활성화");
        }

        if (gamePlayPanel != null && !gamePlayPanel.activeSelf)
        {
            gamePlayPanel.SetActive(true);
            Debug.Log("[DialogueUI] 게임 플레이 UI 활성화");
        }

        if (gameEntry == null)
        {
            Debug.LogWarning("[DialogueUI] GameEntry가 Inspector에 할당되지 않음. FindFirstObjectByType으로 검색...");
            gameEntry = FindFirstObjectByType<GameEntry>();
        }

        if (gameEntry == null)
        {
            Debug.LogError("[DialogueUI] GameEntry를 찾을 수 없습니다!");
            return;
        }

        try
        {
            Debug.Log($"[DialogueUI] GameEntry.SelectSong 호출: {songId}, {difficulty}");
            gameEntry.SelectSong(songId, difficulty);

            // 4) GameEntry의 PlayUI도 확실히 켜준다 (NoteLayer가 이 자식이면 같이 켜짐)
            if (gameEntry.PlayUI != null && !gameEntry.PlayUI.activeSelf)
            {
                gameEntry.PlayUI.SetActive(true);
                Debug.Log("[DialogueUI] GameEntry.PlayUI 활성화");
            }

            Debug.Log("[DialogueUI] GameEntry.InitAndPlay 호출");
            gameEntry.InitAndPlay();

            Debug.Log("[DialogueUI] 게임 시작 성공!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DialogueUI] 게임 시작 실패: {ex.Message}\n{ex.StackTrace}");
        }
    }



    // TutorialManager 순서대로 다음 시나리오
    private void LoadNextScenarioInSequence()
    {
        Debug.Log("[DialogueUI] ScenarioManager에서 다음 시나리오 로드");

        if (ScenarioManager.Instance.MoveToNextScenario())
        {
            var nextScenario = ScenarioManager.Instance.GetCurrentScenario();

            // TextAsset으로 로드
            dialogueLoader.LoadFromTextAsset(nextScenario.jsonFile);
            currentLine = 0;
            nextscript.interactable = true;

            UpdateDialogueUI();
        }
        else
        {
            Debug.Log("[DialogueUI] 모든 시나리오 완료!");
            EndDialogue();
        }
    }

    // 대화 종료
    private void EndDialogue()
    {
        Debug.Log("[DialogueUI] 대화 시스템 종료");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // TODO: 메인 메뉴로 복귀하거나 엔딩 처리
    }

    // UI 업데이트 (NPC 이미지 및 첫 대사 출력)
    private void UpdateDialogueUI()
    {
        // dialoguedata가 null이면 리턴
        if (dialogueLoader.dialoguedata == null)
        {
            Debug.LogWarning("[DialogueUI] dialoguedata가 null입니다. 시나리오를 먼저 로드하세요.");
            return;
        }

        // NPC 이미지 초기화
        foreach (var img in imageBank.Values)
            img.gameObject.SetActive(false);

        var data = dialogueLoader.dialoguedata;

        // 새 NPC 표시
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

        // 첫 대사 출력
        if (data.dialogues != null && data.dialogues.Count > 0)
            OutputDialogue(currentLine);
    }


    #region - json 파일에서 지정해 놓은 대로 분기 설정

    // 외부에서 시나리오 ID로 직접 로드
    public void LoadScenarioById(string scenarioId)
    {
        Debug.Log($"[DialogueUI] 시나리오 ID로 로드: {scenarioId}");

        var data = ScenarioManager.Instance.LoadScenario(scenarioId, dialogueLoader);

        if (data == null)
        {
            Debug.LogError($"[DialogueUI] 시나리오 로드 실패: {scenarioId}");
            return;
        }

        currentLine = 0;
        nextscript.interactable = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(false);

        UpdateDialogueUI();
    }


    private void LoadTutorialById(string tutorialId)
    {
        Debug.Log($"[DialogueUI] 튜토리얼 시작: {tutorialId}");

        // 시나리오 UI OFF
        dialoguePanel?.SetActive(false);

        // 게임 UI OFF
        gamePlayPanel?.SetActive(false);

        // 튜토리얼 UI ON
        tutorialPanel?.SetActive(true);

        // 튜토리얼 JSON 로드 (LoadScenarioById 사용 금지)
        var data = ScenarioManager.Instance.LoadScenario(tutorialId, dialogueLoader);

        if (data == null)
        {
            Debug.LogError($"튜토리얼 로드 실패: {tutorialId}");
            return;
        }

        currentLine = 0;
        nextscript.interactable = true;

        // 이미지 리셋 후 출력
        UpdateDialogueUI();
    }

    #endregion

    // 게임 종료 후 대화 재개 (GameEntry에서 호출)
    public void ResumeAfterGame(string nextScenarioId)
    {
        Debug.Log($"[DialogueUI] 게임 후 대화 재개: {nextScenarioId}");

        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (!string.IsNullOrEmpty(nextScenarioId))
        {
            LoadScenarioById(nextScenarioId);  // ← 수정!
        }
        else
        {
            Debug.LogWarning("[DialogueUI] 다음 시나리오 ID가 지정되지 않음");
        }
    }


    // 특정 버튼으로 튜토리얼 실행
    public void StartTutorialByButton(string tutorialId)
    {
        // 1. 먼저 JSON 로드
        var data = ScenarioManager.Instance.LoadScenario(tutorialId, dialogueLoader);

        if (data == null)
        {
            Debug.LogError($"[DialogueUI] 튜토리얼 로드 실패: {tutorialId}");
            return;
        }

        // 2. UI 전환 (LoadScenarioById 호출 금지!)
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);   // 대화창 끄기

        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);    // 튜토리얼 패널만 켜기

        // 3. 대화 상태 초기화
        currentLine = 0;
        nextscript.interactable = true;

        // 4. UI 업데이트 (첫 대사 출력)
        UpdateDialogueUI();
    }


    // 특정 버튼으로 다이어로그 실행
    public void StartDialogueByButton(string scenarioId)
    {
        Debug.Log($"[DialogueUI] 버튼으로 대화 시작: {scenarioId}");

        // 기본 시나리오 로드 방식 사용
        LoadScenarioById(scenarioId);

        // UI 상태 정리 (선택)
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(false);
    }
}