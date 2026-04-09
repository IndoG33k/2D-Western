using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Scene names")]
    [SerializeField] private string fightSceneName = "SampleScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool pauseTimeOnWin = true;
    [SerializeField] private bool showCursorOnWin = true;

    private bool _shown;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();

        if (hideOnStart)
            HideWin();
    }

    public void ShowWin()
    {
        if (_shown)
            return;
        _shown = true;

        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (pauseTimeOnWin)
            Time.timeScale = 0f;

        GameAudioManager.Instance?.PauseBattleMusicForModal();

        if (showCursorOnWin)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void HideWin()
    {
        _shown = false;

        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        RunProgression.Instance?.ResetRun();
        SceneManager.LoadScene(fightSceneName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        RunProgression.Instance?.ResetRun();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

