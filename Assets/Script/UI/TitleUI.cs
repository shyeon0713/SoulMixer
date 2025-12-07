using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class TitleUI : MonoBehaviour
{

    public Button playButton;
    public Button continueButton;  // 이어하기

    public GameObject titleUI;
    public GameObject MainUI;  //MainUI 창 숨기기
    public GameObject SelectMusic;  //selectmusic 창 숨기기
    public GameObject PlayMusic;  //Playmusic 창 숨기기
    public GameObject Result;  //Result 창 숨기기

    public GameObject dialoguePanel;
    public DialogueUI dialogueUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {  //TitleUI 제외 전부 비활성화
        MainUI.SetActive(false);
        SelectMusic.SetActive(false);
        PlayMusic.SetActive(false);
        Result.SetActive(false);

    }

    void Start()
    {
        playButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);

        // 저장된 진행도가 있으면 이어하기 버튼 활성화
        if (PlayerPrefs.HasKey("ScenarioProgress"))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    void OnNewGameClicked()
    {
        Debug.Log("[TitleUI] 새 게임 시작");

        // 진행 상태 초기화
        ScenarioManager.Instance.ResetProgress();

        // UI 전환
        titleUI.SetActive(false);
        dialoguePanel.SetActive(true);

        // 첫 번째 시나리오 로드
        var firstScenario = ScenarioManager.Instance.GetCurrentScenario();
        if (firstScenario != null)
        {
            dialogueUI.LoadScenarioById(firstScenario.scenarioId);
        }
    }

    void OnContinueClicked()
    {
        Debug.Log("[TitleUI] 이어하기");

        // 저장된 진행도 로드
        ScenarioManager.Instance.LoadProgress();

        // UI 전환
        titleUI.SetActive(false);
        dialoguePanel.SetActive(true);

        // 현재 시나리오 로드
        var currentScenario = ScenarioManager.Instance.GetCurrentScenario();
        if (currentScenario != null)
        {
            dialogueUI.LoadScenarioById(currentScenario.scenarioId);
        }
    }
}


