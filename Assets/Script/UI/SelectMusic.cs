using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class SelectMusic : MonoBehaviour
{

    [SerializeField] 
    [Header("musicselcet 창에서 처음에 숨겨야하는 버튼 리스트")]
    private List<GameObject> ButtonObjects = new List<GameObject>();  //musicselcet 창에서 처음에 숨겨야하는 버튼 리스트

    [SerializeField] private GameObject MainUI;  // 메인 UI
    [SerializeField] private GameObject Selectmusic; //악곡선택 UI 
    [SerializeField] private GameObject Playmusic; //악곡연주 UI 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainUI.SetActive(false);
        Playmusic.SetActive(false);
    }

}
