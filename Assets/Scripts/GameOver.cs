using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool pauseTimeOnGameOver = true;
    [SerializeField] private bool showCursorOnGameOver = true;

    private bool _shown;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();

        if (hideOnStart)
            HideGameOver();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath.AddListener(ShowGameOver);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath.RemoveListener(ShowGameOver);
    }

    public void ShowGameOver()
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

        if (pauseTimeOnGameOver)
            Time.timeScale = 0f;

        if (showCursorOnGameOver)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void HideGameOver()
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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
        
    }
}
