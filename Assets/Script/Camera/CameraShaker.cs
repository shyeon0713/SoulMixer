using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker instance; // ½Ì±ÛÅæ »ý¼º


    void Awake()
    {
        instance = this;
    }

    public void Shake(float duration = 0.1f, float magnitude = 0.1f)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float time = 0f;

        while (time < duration)
        {
            float x = Random.Range(-6f, 6f) * magnitude; 
            float y = Random.Range(-3f, 3f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0); // Ä«¸Þ¶ó ½¦ÀÌÅ©
            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos; // ¿ø·¡ À§Ä¡·Î º¹±Í
    }
}
