using UnityEngine;

public class RunProgression : MonoBehaviour
{
    public static RunProgression Instance { get; private set; }

    [SerializeField] private AITier currentTier = AITier.Level1;

    public AITier CurrentTier => currentTier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetRun()
    {
        currentTier = AITier.Level1;
    }

    public bool TryAdvanceTier()
    {
        if (currentTier >= AITier.Level4)
            return false;

        currentTier = (AITier)((int)currentTier + 1);
        return true;
    }
}

