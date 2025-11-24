using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class TitleUI : MonoBehaviour
{

    //[SerializeField] private List<GameObject> ButtonObjects = new List<GameObject>(); //Main 창에서 처음에 숨겨야하는 버튼 리스트
    
    public GameObject MainUI;  //MainUI 창 숨기기
    public GameObject SelectMusic;  //selectmusic 창 숨기기
    public GameObject PlayMusic;  //Playmusic 창 숨기기
    public GameObject Result;  //Result 창 숨기기

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {  //TitleUI 제외 전부 비활성화
        MainUI.SetActive(false);
        SelectMusic.SetActive(false);
        PlayMusic.SetActive(false);
        Result.SetActive(false);

     //   ButtonObjects[0].SetActive(false);  // 처음시작할 떄, right 버튼 안보이도록 설정
    }

}
