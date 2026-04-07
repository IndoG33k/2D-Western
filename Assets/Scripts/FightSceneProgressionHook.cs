using UnityEngine;
using UnityEngine.SceneManagement;

public class FightSceneProgressionHook : MonoBehaviour
{
    [Header("Enemy (the duel target)")]
    [SerializeField] private AICombatController enemyAI;
    [SerializeField] private Health enemyHealth;

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
        if (enemyAI != null && RunProgression.Instance != null)
            enemyAI.SetTier(RunProgression.Instance.CurrentTier, resetAmmo: true);
    }

    private void OnEnemyDefeated()
    {
        if (RunProgression.Instance == null)
            return;

        bool advanced = RunProgression.Instance.TryAdvanceTier();
        if (advanced)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(fightSceneName);
            return;
        }

        winPanel?.ShowWin();
    }
}

