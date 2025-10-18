using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class MainUIreset : MonoBehaviour
{

    [SerializeField] private List<GameObject> ButtonObjects = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ButtonObjects.Add(GameObject.Find("falsebutton"));
        ButtonObjects[0].SetActive(false);  // 처음시작할 떄, right 버튼 안보이도록 설정
    }

}
