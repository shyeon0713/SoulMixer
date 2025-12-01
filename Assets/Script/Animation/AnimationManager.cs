using UnityEngine;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour
{
   public static AnimationManager instance;  // 싱글톤 생성

    [System.Serializable] public struct AnimData
    {
        public string name;
        public GameObject prefab;
    }

    public List<AnimData> animationList; // 애니메이션 리스트

    // 빠른 검색을 위한 딕셔너리
    private Dictionary<string, GameObject> animDict = new Dictionary<string, GameObject>();

    //애니메이션 재활용을위한 리스트
    private Dictionary<string, List<GameObject>> poolDict = new Dictionary<string, List<GameObject>>();
   
    private void Awake()
    {
        if (instance == null) instance = this;  // 싱글톤 설정

        foreach (var item in animationList)
        {
            // 1. 프리팹 등록
            if (!animDict.ContainsKey(item.name))
                animDict.Add(item.name, item.prefab);

            // 2. 창고 공간 마련
            if (!poolDict.ContainsKey(item.name))
                poolDict.Add(item.name, new List<GameObject>());
        }

    }

    #region- 외부에서 호출하는 함수
    public void PlayAnimation(string name, Vector3 position)
    {
        if (!animDict.ContainsKey(name)) return;

        GameObject selectObj = null;
        List<GameObject> currentPool = poolDict[name];

        foreach (var obj in currentPool)
        {
            if (!obj.activeSelf) 
            {
                selectObj = obj;
                break;
            }
        }

        if (selectObj == null)
        {
            selectObj = Instantiate(animDict[name]);
            // 부모를 매니저로 설정해서 Hierarchy창 정리 (선택사항)
            selectObj.transform.SetParent(transform);
            // 창고 명단에 등록
            currentPool.Add(selectObj);
        }

        selectObj.transform.position = position;
        selectObj.SetActive(true); 
    }
    #endregion
    //AnimationManager.Instance.PlayAnimation("LevelUpEffect", transform.position); 로 호출
}
