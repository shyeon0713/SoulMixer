using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class SelectMusic : MonoBehaviour
{

    [SerializeField] 
    [Header("musicselcet â���� ó���� ���ܾ��ϴ� ��ư ����Ʈ")]
    private List<GameObject> ButtonObjects = new List<GameObject>();  //musicselcet â���� ó���� ���ܾ��ϴ� ��ư ����Ʈ

    [SerializeField] private GameObject MainUI;  // ���� UI
    [SerializeField] private GameObject Selectmusic; //�ǰ�� UI 
    [SerializeField] private GameObject Playmusic; //�ǰ�� UI 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainUI.SetActive(false);
        Playmusic.SetActive(false);
    }

}
