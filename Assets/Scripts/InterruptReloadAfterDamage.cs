using UnityEngine;

public class InterruptReloadAfterDamage : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private ChamberReloadController chamberReload;

    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged.AddListener(OnDamaged);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged.RemoveListener(OnDamaged);
    }

    private void OnDamaged(int _)
    {
        chamberReload?.InterruptPreserveLoadedAmmo();
    }
}
