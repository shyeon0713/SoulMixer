using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class SelectMusic : MonoBehaviour
{

    [SerializeField] private List<GameObject> ButtonObjects = new List<GameObject>();  //musicselcet â���� ó���� ���ܾ��ϴ� ��ư ����Ʈ
    [SerializeField] private GameObject MainUI;  // ���� UI
    [SerializeField] private GameObject Selectmusic; //�ǰ�� UI 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainUI.SetActive(false);
        Selectmusic.SetActive(true);
    }

}
