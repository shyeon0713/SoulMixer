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
    private Dictionary<string, GameObject> animDictionary = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;  // 싱글톤 설정
        else Destroy(gameObject);

        // 리스트를 딕셔너리로 변환 (초기화)
        foreach (var item in animationList)
        {
            animDictionary.Add(item.name, item.prefab);
        }

    }

    #region- 외부에서 호출하는 함수
    public void PlayAnimation(string name, Vector3 position)
    {
        if (animDictionary.ContainsKey(name))
        {
            Instantiate(animDictionary[name], position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"애니메이션 {name}을 찾을 수 없습니다.");
        }
    }
    #endregion
    //AnimationManager.Instance.PlayAnimation("LevelUpEffect", transform.position); 로 호출
}
