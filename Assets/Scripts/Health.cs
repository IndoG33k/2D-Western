using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool refillToMaxOnStart = true;

    [Header("Events")]
    public UnityEvent<int> OnDamaged;
    public UnityEvent<int> OnHealed;
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        if (refillToMaxOnStart)
            currentHealth = maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnDamaged?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int gained = currentHealth - before;
        if (gained <= 0)
            return;

        OnHealed?.Invoke(gained);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int newMax, bool refill = false)
    {
        maxHealth = Mathf.Max(1, newMax);
        if (refill)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
