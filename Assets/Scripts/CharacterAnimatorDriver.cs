using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimatorDriver : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;
    [SerializeField] private PlayerWeaponController playerWeapon;
    [SerializeField] private AICombatController aiCombat;

    [Header("Animator parameters")]
    [SerializeField] private string shootTrigger = "Shoot";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string dieTrigger = "Die";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (health == null)
            health = GetComponent<Health>();
        if (playerWeapon == null)
            playerWeapon = GetComponent<PlayerWeaponController>();
        if (aiCombat == null)
            aiCombat = GetComponent<AICombatController>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged.AddListener(OnDamaged);
            health.OnDeath.AddListener(OnDeath);
        }

        if (playerWeapon != null)
            playerWeapon.ShotFired += OnShotFired;
        if (aiCombat != null)
            aiCombat.ShotFired += OnShotFired;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged.RemoveListener(OnDamaged);
            health.OnDeath.RemoveListener(OnDeath);
        }

        if (playerWeapon != null)
            playerWeapon.ShotFired -= OnShotFired;
        if (aiCombat != null)
            aiCombat.ShotFired -= OnShotFired;
    }

    public void SetRuntimeController(RuntimeAnimatorController controller)
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;
    }

    private void OnShotFired()
    {
        if (animator != null && !string.IsNullOrEmpty(shootTrigger))
            animator.SetTrigger(shootTrigger);
    }

    private void OnDamaged(int _)
    {
        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);
    }

    private void OnDeath()
    {
        if (animator != null && !string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);
    }
}

