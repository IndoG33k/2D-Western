using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        GameAudioManager.Instance?.StopBattleMusic();
    }

    public void PlayGame()
    {
        RunProgression.Instance?.ResetRun();
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
