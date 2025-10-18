using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UI;

public class Cameramove : MonoBehaviour
{
    [SerializeField]
    public int listnumber;  //ī�޶� ��ġ ����Ʈ��ȣ

    private List<Vector3> CameraPosition = new List<Vector3>();// camera postion ����
    private List<Vector3> CameraRotation = new List<Vector3>();// camera rotaion ����
    private List<GameObject> CameraObjects = new List<GameObject>(); // ī�޶� ���� ��ġ�� ������Ʈ�� �ξ� ��ǥ��������
    public List<Button> Buttons = new List<Button>(); // ��ư�鵵 ����Ʈ�� ����


    public void SetPosition()  //ī�޶� ��ġ ����
    {
        CameraObjects[0] = GameObject.Find("Pos1");
        CameraObjects[1] = GameObject.Find("Pos2");
        CameraObjects[2] = GameObject.Find("Pos3");
        CameraObjects[3] = GameObject.Find("Pos4");

        Vector3 pos1 = CameraObjects[0].transform.position; // main1,2�� ��ġ�� ����
        Vector3 pos2 = CameraObjects[1].transform.position;  
        Vector3 pos3 = CameraObjects[2].transform.position;    // �ǰ���ϴ� ��ġ
        Vector3 pos4 = CameraObjects[3].transform.position;   // �ǰ� �÷����ϴ� ��ġ

        CameraPosition.Add(pos1);
        CameraPosition.Add(pos2);
        CameraPosition.Add(pos3);
        CameraPosition.Add(pos4);

    }

    public void SetRotation() //ī�޶� ���� ����
    {
        Vector3 rot1 = CameraObjects[0].transform.eulerAngles; // main1,2�� ��ġ�� ����
        Vector3 rot2 = CameraObjects[1].transform.eulerAngles;  
        Vector3 rot3 = CameraObjects[2].transform.eulerAngles;  // �ǰ���ϴ� ��ġ
        Vector3 rot4 = CameraObjects[3].transform.eulerAngles;   // �ǰ� �÷����ϴ� ��ġ

        CameraRotation.Add(rot1);
        CameraRotation.Add(rot2);
        CameraRotation.Add(rot3);
        CameraRotation.Add(rot4);

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //�ʱ� ī�޶� ���� -> main1 ī�޶� ��ġ��
        this.transform.position = CameraPosition[0];
        this.transform.eulerAngles = CameraRotation[0];

        Debug.LogError(this.transform.position);
        Debug.LogError(this.transform.eulerAngles);

        //button listener �߰�
    //    Buttons[0].onClick.AddListener(MovePos2);  //pos2�� �̵�
    //    Buttons[1].onClick.AddListener(MovePos3);  //pos3�� �̵�
    //    Buttons[2].onClick.AddListener(MovePos4);  //pos4�� �̵�

    }

    #region - ��ư Ŭ�� ��, �ش� ��ġ�� ī�޶� �̵�

    public void MovePos2()
    {
        this.transform.position = CameraPosition[1];
        this.transform.eulerAngles = CameraRotation[1];
    }
    public void MovePos3()
    {
        this.transform.position = CameraPosition[2];
        this.transform.eulerAngles = CameraRotation[2];

    }
    public void MovePos4()
    {
        this.transform.position = CameraPosition[3];
        this.transform.eulerAngles = CameraRotation[3];
    }

    #endregion


}
