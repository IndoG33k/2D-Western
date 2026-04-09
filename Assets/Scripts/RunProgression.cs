using UnityEngine;

public class RunProgression : MonoBehaviour
{
    public static RunProgression Instance { get; private set; }

    [System.Serializable]
    public struct Encounter
    {
        public AITier tier;
        public Sprite spriteOverride;
        public RuntimeAnimatorController animatorControllerOverride;
        public AudioSet audioSetOverride;
    }

    [Header("Encounters (8 total: L1x2, L2x2, L3x2, L4x2)")]
    [SerializeField] private Encounter[] encounters;

    [SerializeField] private int encounterIndex;

    public int EncounterIndex => encounterIndex;
    public int EncounterCount => encounters?.Length ?? 0;

    public Encounter CurrentEncounter
    {
        get
        {
            EnsureDefaultEncountersIfMissing();
            int max = Mathf.Max(0, EncounterCount - 1);
            int idx = Mathf.Clamp(encounterIndex, 0, max);
            return encounters[idx];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureDefaultEncountersIfMissing();
    }

    public void ResetRun()
    {
        encounterIndex = 0;
        GameAudioManager.Instance?.ResetRunAudioState();
    }

    public bool TryAdvanceEncounter()
    {
        EnsureDefaultEncountersIfMissing();

        if (EncounterCount <= 0)
            return false;

        if (encounterIndex >= EncounterCount - 1)
            return false;

        encounterIndex++;
        return true;
    }

    private void EnsureDefaultEncountersIfMissing()
    {
        if (encounters != null && encounters.Length > 0)
            return;

        encounters = new[]
        {
            new Encounter { tier = AITier.Level1, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level1, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level2, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level2, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level3, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level3, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level4, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
            new Encounter { tier = AITier.Level4, spriteOverride = null, animatorControllerOverride = null, audioSetOverride = null },
        };
    }
}

