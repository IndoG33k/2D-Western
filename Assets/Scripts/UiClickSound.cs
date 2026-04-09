using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UiClickSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => GameAudioManager.Instance?.PlayMenuClick());
    }
}
