using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
public class MainUIreset : MonoBehaviour
{
    [Header("숨겨야하는 버튼 리스트")]
    [SerializeField] private List<GameObject> ButtonObjects = new List<GameObject>(); //Main 창에서 처음에 숨겨야하는 버튼 리스트
    //추후에 활성화,비활성화 버튼 리스트 분리 필요

    [Header("UI 오브젝트")]
    [SerializeField] private GameObject SelectMusic;  //selectmusic 창 숨기기
     [SerializeField] private GameObject PlayMusic;  //Playmusic 창 숨기기
    [SerializeField] private GameObject ResultUI; // 결과창 숨기기


   
   void Awake()
    {
        SelectMusic.SetActive(false);
        PlayMusic.SetActive(false);
        ResultUI.SetActive(false);

        ButtonObjects[0].SetActive(false);  // 처음시작할 떄, right 버튼 안보이도록 설정
      

    }

}
