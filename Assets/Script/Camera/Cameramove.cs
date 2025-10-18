using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UI;

public class Cameramove : MonoBehaviour
{
    [SerializeField]
    public int listnumber;  //카메라 위치 리스트번호

    private List<Vector3> CameraPosition = new List<Vector3>();// camera postion 지정
    private List<Vector3> CameraRotation = new List<Vector3>();// camera rotaion 지정
    private List<GameObject> CameraObjects = new List<GameObject>(); // 카메라가 있을 위치에 오브젝트를 두어 좌표가져오기
    public List<Button> Buttons = new List<Button>(); // 버튼들도 리스트로 묶음


    public void SetPosition()  //카메라 위치 설정
    {
        CameraObjects[0] = GameObject.Find("Pos1");
        CameraObjects[1] = GameObject.Find("Pos2");
        CameraObjects[2] = GameObject.Find("Pos3");
        CameraObjects[3] = GameObject.Find("Pos4");

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //초기 카메라 설정 -> main1 카메라 위치로
        this.transform.position = CameraPosition[0];
        this.transform.eulerAngles = CameraRotation[0];

        Debug.LogError(this.transform.position);
        Debug.LogError(this.transform.eulerAngles);

        //button listener 추가
    //    Buttons[0].onClick.AddListener(MovePos2);  //pos2로 이동
    //    Buttons[1].onClick.AddListener(MovePos3);  //pos3로 이동
    //    Buttons[2].onClick.AddListener(MovePos4);  //pos4로 이동

    }

    #region - 버튼 클릭 시, 해당 위치로 카메라 이동

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
