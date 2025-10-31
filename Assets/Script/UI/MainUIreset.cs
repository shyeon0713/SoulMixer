using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class MainUIreset : MonoBehaviour
{

    [SerializeField] private List<GameObject> ButtonObjects = new List<GameObject>(); //Main 창에서 처음에 숨겨야하는 버튼 리스트
    public GameObject SelectMusic;  //selectmusic 창 숨기기
    public GameObject PlayMusic;  //Playmusic 창 숨기기

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SelectMusic.SetActive(false);
        PlayMusic.SetActive(false);

        ButtonObjects[0].SetActive(false);  // 처음시작할 떄, right 버튼 안보이도록 설정
    }

}
