using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayUIreset : MonoBehaviour
{

 
    //[Header("musicselcet 창에서 처음에 숨겨야하는 버튼 리스트")]
    //[SerializeField]private List<Button> ButtonObjects = new List<Button>();  //musicselcet 창에서 처음에 숨겨야하는 버튼 리스트

    [SerializeField] private GameObject MainUI;  // 메인 UI
    [SerializeField] private GameObject Selectmusic; //악곡선택 UI 
    [SerializeField] private GameObject Playmusic; //악곡연주 UI 
    [SerializeField] private GameObject Result;  //Result 창 숨기기

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
    }



}
