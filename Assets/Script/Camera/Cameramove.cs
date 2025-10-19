using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UI;

public class Cameramove : MonoBehaviour
{
   // [SerializeField] private int listnumber;  //카메라 위치 리스트번호

    private List<Vector3> CameraPosition = new List<Vector3>();// camera postion 지정
    private List<Vector3> CameraRotation = new List<Vector3>();// camera rotaion 지정
    [SerializeField] private List<GameObject> CameraObjects = new List<GameObject>(); // 카메라가 있을 위치에 오브젝트를 두어 좌표가져오기
    [SerializeField] private List<Button> Buttons = new List<Button>(); // 버튼들도 리스트로 묶음

    #region - 카메라 위치 및 회전 설정 -> 오브젝트를 배치하여 해당 오브젝트 좌표,회전값을 얻어 사용 
    public void SetPosition()  //카메라 위치 설정
    {
        CameraObjects.Add(GameObject.Find("Pos1"));
        CameraObjects.Add(GameObject.Find("Pos2"));
        CameraObjects.Add(GameObject.Find("Pos3"));
        CameraObjects.Add(GameObject.Find("Pos4"));

        Vector3 pos1 = CameraObjects[0].transform.position; // main1,2는 위치는 동일
        Vector3 pos2 = CameraObjects[1].transform.position;  
        Vector3 pos3 = CameraObjects[2].transform.position;    // 악곡선택하는 위치
        Vector3 pos4 = CameraObjects[3].transform.position;   // 악곡 플레이하는 위치

        CameraPosition.Add(pos1);
        CameraPosition.Add(pos2);
        CameraPosition.Add(pos3);
        CameraPosition.Add(pos4);

    }

    public void SetRotation() //카메라 각도 설정
    {
        Vector3 rot1 = CameraObjects[0].transform.eulerAngles; // main1,2는 위치는 동일
        Vector3 rot2 = CameraObjects[1].transform.eulerAngles;  
        Vector3 rot3 = CameraObjects[2].transform.eulerAngles;  // 악곡선택하는 위치
        Vector3 rot4 = CameraObjects[3].transform.eulerAngles;   // 악곡 플레이하는 위치

        CameraRotation.Add(rot1);
        CameraRotation.Add(rot2);
        CameraRotation.Add(rot3);
        CameraRotation.Add(rot4);

    }

    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //초기 카메라 설정 -> main1 카메라 위치로
        SetPosition();
        SetRotation();
       
        this.transform.position = CameraPosition[0];
        this.transform.eulerAngles = CameraRotation[0];


        //button listener 추가
        Buttons[0].onClick.AddListener(MovePos1);  //pos1로 이동
        Buttons[1].onClick.AddListener(MovePos2);  //pos2로 이동
        Buttons[2].onClick.AddListener(MovePos3);  //pos3로 이동
        Buttons[3].onClick.AddListener(MovePos4);  //pos4로 이동
        Buttons[4].onClick.AddListener(MovePos1);  //pos4로 이동

    }

    #region - 버튼 클릭 시, 해당 위치로 카메라 이동

    public void MovePos1()
    {
        this.transform.position = CameraPosition[0];
        this.transform.eulerAngles = CameraRotation[0];
    }
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
