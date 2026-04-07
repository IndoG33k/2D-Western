using System.Collections;
using UnityEngine;

public class FightCountdownBootstrapper : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private Countdown countdown;

    [Header("Combat to gate")]
    [SerializeField] private PlayerWeaponController playerWeapon;
    [SerializeField] private DeadeyeController deadeye;
    [SerializeField] private AICombatController[] enemies;

    [Header("Behavior")]
    [SerializeField] private bool autoFindIfMissing = true;
    [SerializeField] private bool forceUnpauseOnStart = true;

    private IEnumerator Start()
    {
        if (forceUnpauseOnStart)
            Time.timeScale = 1f;

        if (countdown == null)
            countdown = GetComponentInChildren<Countdown>(true);

        if (autoFindIfMissing)
        {
            if (playerWeapon == null)
                playerWeapon = FindFirstObjectByType<PlayerWeaponController>();

            if (deadeye == null)
                deadeye = FindFirstObjectByType<DeadeyeController>();

            if (enemies == null || enemies.Length == 0)
                enemies = FindObjectsByType<AICombatController>(FindObjectsSortMode.None);
        }

        SetCombatEnabled(false);

        if (countdown != null)
            yield return countdown.Run();

        SetCombatEnabled(true);
    }

    private void SetCombatEnabled(bool enabled)
    {
        if (playerWeapon != null)
            playerWeapon.enabled = enabled;

        if (!enabled && deadeye != null)
            deadeye.ForceExitDeadeye();

        if (deadeye != null)
            deadeye.enabled = enabled;

        if (enemies == null)
            return;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
                enemies[i].enabled = enabled;
        }
    }
}

