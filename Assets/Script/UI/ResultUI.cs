using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private GameObject MainUI;  // ∏ﬁ¿Œ UI
    [SerializeField] private GameObject Selectmusic; //æ«∞Óº±≈√ UI 
    [SerializeField] private GameObject Playmusic; //æ«∞Óø¨¡÷ UI 


    void Start()
    {
        MainUI.SetActive(false);
        Playmusic.SetActive(false);
        Selectmusic.SetActive(false);  // Result √¢ ¡¶ø‹«œ∞Ì ∫Ò»∞º∫»≠
    }
}
