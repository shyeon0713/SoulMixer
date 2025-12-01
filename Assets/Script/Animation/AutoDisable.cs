using UnityEngine;
using System.Collections;

public class AutoDisable : MonoBehaviour
{
    public float lifeTime = 1.0f;

    // 오브젝트가 켜질 때마다 실행됨 (재활용될 때마다 실행)
    void OnEnable()
    {
        StartCoroutine(DisableAfterTime());
    }

    IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false); // 파괴(Destroy) 대신 꺼두기(Off)
    }
}

