using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Input")]
    [SerializeField] private string togglePausePath = "<Keyboard>/escape";

    [Header("Optional: disable while paused")]
    [SerializeField] private PlayerWeaponController playerWeapon;
    [SerializeField] private ChamberReloadController chamberReload;
    [SerializeField] private DeadeyeController deadeye;
    [SerializeField] private MonoBehaviour[] extraBehavioursToDisable;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool showCursorWhenPaused = true;

    public bool IsPaused { get; private set; }

    private InputAction _togglePauseAction;

    private void Awake()
    {
        _togglePauseAction = new InputAction(type: InputActionType.Button, binding: togglePausePath);

        if (panelRoot == null)
            panelRoot = gameObject;

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();

        if (hideOnStart)
            HidePanel();
    }

    private void OnEnable()
    {
        _togglePauseAction?.Enable();
    }

    private void OnDisable()
    {
        _togglePauseAction?.Disable();
    }

    private void OnDestroy()
    {
        _togglePauseAction?.Dispose();
    }

    private void Update()
    {
        if (_togglePauseAction != null && _togglePauseAction.WasPressedThisFrame())
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused)
            Unpause();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;

        if (deadeye != null)
        {
            deadeye.ForceExitDeadeye();
            deadeye.enabled = false;
        }

        if (playerWeapon != null)
            playerWeapon.enabled = false;

        if (chamberReload != null)
            chamberReload.enabled = false;

        if (extraBehavioursToDisable != null)
        {
            for (int i = 0; i < extraBehavioursToDisable.Length; i++)
            {
                var b = extraBehavioursToDisable[i];
                if (b != null)
                    b.enabled = false;
            }
        }

        ShowPanel();
        Time.timeScale = 0f;
        GameAudioManager.Instance?.PauseBattleMusic();

        if (showCursorWhenPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Unpause()
    {
        if (!IsPaused)
            return;

        IsPaused = false;

        Time.timeScale = 1f;
        GameAudioManager.Instance?.ResumeBattleMusic();
        HidePanel();

        if (playerWeapon != null)
            playerWeapon.enabled = true;

        if (chamberReload != null)
            chamberReload.enabled = true;

        if (deadeye != null)
            deadeye.enabled = true;

        if (extraBehavioursToDisable != null)
        {
            for (int i = 0; i < extraBehavioursToDisable.Length; i++)
            {
                var b = extraBehavioursToDisable[i];
                if (b != null)
                    b.enabled = true;
            }
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        RunProgression.Instance?.ResetRun();
        GameAudioManager.Instance?.StopBattleMusic();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ShowPanel()
    {
        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    private void HidePanel()
    {
        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}
