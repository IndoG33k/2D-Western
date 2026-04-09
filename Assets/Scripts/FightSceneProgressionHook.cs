using UnityEngine;
using UnityEngine.SceneManagement;

public class FightSceneProgressionHook : MonoBehaviour
{
    [Header("Enemy (the duel target)")]
    [SerializeField] private AICombatController enemyAI;
    [SerializeField] private Health enemyHealth;
    [SerializeField] private SpriteRenderer enemySpriteRenderer;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private CharacterAnimatorDriver enemyAnimatorDriver;
    [SerializeField] private CharacterAudioDriver enemyAudioDriver;

    [Header("Win UI (shown after L4)")]
    [SerializeField] private WinPanel winPanel;

    [Header("Scene names")]
    [SerializeField] private string fightSceneName = "SampleScene";

    private void Awake()
    {
        if (RunProgression.Instance == null)
            new GameObject(nameof(RunProgression)).AddComponent<RunProgression>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath.AddListener(OnEnemyDefeated);
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath.RemoveListener(OnEnemyDefeated);
    }

    private void Start()
    {
        if (RunProgression.Instance == null)
            return;

        var encounter = RunProgression.Instance.CurrentEncounter;

        if (enemyAI != null)
            enemyAI.SetTier(encounter.tier, resetAmmo: true);

        if (enemyAnimator != null && encounter.animatorControllerOverride != null)
            enemyAnimator.runtimeAnimatorController = encounter.animatorControllerOverride;
        if (enemyAnimatorDriver != null && encounter.animatorControllerOverride != null)
            enemyAnimatorDriver.SetRuntimeController(encounter.animatorControllerOverride);

        if (enemySpriteRenderer != null && encounter.spriteOverride != null)
            enemySpriteRenderer.sprite = encounter.spriteOverride;

        if (enemyAudioDriver != null && encounter.audioSetOverride != null)
            enemyAudioDriver.SetAudioSet(encounter.audioSetOverride);
    }

    private void OnEnemyDefeated()
    {
        if (RunProgression.Instance == null)
            return;

        bool advanced = RunProgression.Instance.TryAdvanceEncounter();
        if (advanced)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(fightSceneName);
            return;
        }

        winPanel?.ShowWin();
    }
}

