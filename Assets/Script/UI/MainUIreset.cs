using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class MainUIreset : MonoBehaviour
{

    [SerializeField] private List<GameObject> ButtonObjects = new List<GameObject>(); //Main â���� ó���� ���ܾ��ϴ� ��ư ����Ʈ
    public GameObject SelectMusic;  //selectmusic â �����
    public GameObject PlayMusic;  //Playmusic â �����

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SelectMusic.SetActive(false);
        PlayMusic.SetActive(false);

        ButtonObjects[0].SetActive(false);  // ó�������� ��, right ��ư �Ⱥ��̵��� ����
    }

}
