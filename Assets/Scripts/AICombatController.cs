using System.Collections;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AICombatController : MonoBehaviour
{
    [SerializeField] private AITier tier = AITier.Level1;
    [SerializeField] private Health health;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform target;
    [SerializeField] private PlayerWeaponController playerWeapon;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private int magazineSize = 6;
    [SerializeField] private float minShotIntervalSeconds = 1.2f;
    [SerializeField] private float maxShotIntervalSeconds = 2.5f;
    [SerializeField] private float reloadSecondsPerBullet = 1f;
    [SerializeField] private int reloadWhenAmmoAtOrBelow = 0;
    [SerializeField] private bool reloadToFullAlways = true;
    [SerializeField] private int minBurstShots = 1;
    [SerializeField] private int maxBurstShots = 1;
    [SerializeField] private bool keepOneBulletInChamber;
    [SerializeField] [Range(0f, 1f)] private float startReloadWhenPlayerReloadsChance;
    [SerializeField] [Range(0f, 1f)] private float punishPlayerReloadWithShotChance;
    [SerializeField] [Range(0f, 1f)] private float surpriseShotDuringReloadChancePerSecond;
    [SerializeField] private float punishShotCooldownSeconds = 0.75f;
    [SerializeField] [Range(0f, 1f)] private float aimAtTargetChance = 0.25f;
    [SerializeField] private float aimSpreadDegrees = 25f;
    [SerializeField] [Range(0f, 1f)] private float waitForPlayerShotProbability;
    [SerializeField] private float pressureIntervalSeconds = 2f;
    [SerializeField] private int pressureMinShots = 1;
    [SerializeField] private int pressureMaxShots = 1;
    [SerializeField] private float waitTickSeconds = 0.1f;
    [SerializeField] private float deflectCheckIntervalSeconds = 0.1f;
    [SerializeField] private float deflectCooldownSeconds = 0.35f;
    [SerializeField] private float deflectAwarenessRadius = 12f;
    [SerializeField] private float directThreatRadius = 1.1f;
    [SerializeField] private float maxThreatLookaheadSeconds = 1.25f;
    [SerializeField] private bool debugLogs;

    private int _ammo;
    private Coroutine _loop;
    private bool _isReloading;
    private bool _lastPlayerReloading;
    private float _nextPunishShotTime;
    private float _nextDeflectCheckTime;
    private float _nextDeflectTime;
    private float _deflectChanceDirect;
    private float _deflectChanceOffTarget;
    private float _lastPlayerShotTime;
    private float _lastPressureTime;
    private int _bulletDamage;
    private float _lastMeaningfulActionTime;

    public AITier Tier => tier;
    public event Action ShotFired;

    public void SetTier(AITier newTier, bool resetAmmo)
    {
        tier = newTier;
        ApplyTierStats();

        if (resetAmmo)
            _ammo = Mathf.Max(0, magazineSize);

        _isReloading = false;
        _lastPressureTime = Time.time;
        _lastPlayerShotTime = -999f;
        _lastMeaningfulActionTime = Time.time;
    }

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        _ammo = Mathf.Max(0, magazineSize);
        _lastPressureTime = Time.time;
        _lastPlayerShotTime = -999f;
        _lastMeaningfulActionTime = Time.time;
    }

    private void OnDisable()
    {
        if (playerWeapon != null)
            playerWeapon.ShotFired -= OnPlayerShotFired;
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
    }

    private void Start()
    {
        ApplyTierStats();
        _ammo = Mathf.Max(0, magazineSize);
        if (playerWeapon != null)
            playerWeapon.ShotFired += OnPlayerShotFired;
        if (debugLogs)
            Debug.Log($"[{nameof(AICombatController)}] Started. Tier={tier} HP={(health != null ? health.CurrentHealth : -1)}/{(health != null ? health.MaxHealth : -1)} Ammo={_ammo} timeScale={Time.timeScale}", this);

        if (_loop == null)
            _loop = StartCoroutine(CombatLoop());
    }

    private void ApplyTierStats()
    {
        if (health == null)
            return;

        int maxHp;
        switch (tier)
        {
            case AITier.Level1:
                maxHp = 10;
                minBurstShots = 1;
                maxBurstShots = 1;
                reloadSecondsPerBullet = 1f;
                reloadWhenAmmoAtOrBelow = 0;
                reloadToFullAlways = true;
                keepOneBulletInChamber = false;
                startReloadWhenPlayerReloadsChance = 0f;
                punishPlayerReloadWithShotChance = 0f;
                surpriseShotDuringReloadChancePerSecond = 0f;
                _deflectChanceDirect = 0.25f;
                _deflectChanceOffTarget = 0.75f;
                waitForPlayerShotProbability = 0f;
                pressureIntervalSeconds = 1.75f;
                pressureMinShots = 1;
                pressureMaxShots = 1;
                _bulletDamage = 1;
                aimSpreadDegrees = 35f;
                break;
            case AITier.Level2:
                maxHp = 12;
                minBurstShots = 1;
                maxBurstShots = 2;
                reloadSecondsPerBullet = 0.8f;
                reloadWhenAmmoAtOrBelow = 3;
                reloadToFullAlways = true;
                keepOneBulletInChamber = false;
                startReloadWhenPlayerReloadsChance = 0f;
                punishPlayerReloadWithShotChance = 0.5f;
                surpriseShotDuringReloadChancePerSecond = 0f;
                _deflectChanceDirect = 0.5f;
                _deflectChanceOffTarget = 0.5f;
                waitForPlayerShotProbability = 0f;
                pressureIntervalSeconds = 1.5f;
                pressureMinShots = 1;
                pressureMaxShots = 2;
                _bulletDamage = 1;
                aimSpreadDegrees = 25f;
                break;
            case AITier.Level3:
                maxHp = 15;
                minBurstShots = 1;
                maxBurstShots = 1;
                reloadSecondsPerBullet = 0.5f;
                reloadWhenAmmoAtOrBelow = 0;
                reloadToFullAlways = false;
                keepOneBulletInChamber = true;
                startReloadWhenPlayerReloadsChance = 0.5f;
                punishPlayerReloadWithShotChance = 0.5f;
                surpriseShotDuringReloadChancePerSecond = 0.35f;
                _deflectChanceDirect = 0.75f;
                _deflectChanceOffTarget = 0.25f;
                waitForPlayerShotProbability = 0.75f;
                pressureIntervalSeconds = 2.25f;
                pressureMinShots = 1;
                pressureMaxShots = 2;
                _bulletDamage = 1;
                aimSpreadDegrees = 15f;
                break;
            case AITier.Level4:
                maxHp = 20;
                minBurstShots = 1;
                maxBurstShots = 1;
                reloadSecondsPerBullet = 0.2f;
                reloadWhenAmmoAtOrBelow = 0;
                reloadToFullAlways = false;
                keepOneBulletInChamber = true;
                startReloadWhenPlayerReloadsChance = 0.5f;
                punishPlayerReloadWithShotChance = 0.5f;
                surpriseShotDuringReloadChancePerSecond = 0.6f;
                _deflectChanceDirect = 0.95f;
                _deflectChanceOffTarget = 0.05f;
                waitForPlayerShotProbability = 0.95f;
                pressureIntervalSeconds = 2.75f;
                pressureMinShots = 1;
                pressureMaxShots = 3;
                _bulletDamage = 1;
                aimSpreadDegrees = 6f;
                break;
            default:
                maxHp = 3;
                _deflectChanceDirect = 0.25f;
                _deflectChanceOffTarget = 0.75f;
                waitForPlayerShotProbability = 0f;
                _bulletDamage = 1;
                aimSpreadDegrees = 35f;
                break;
        }

        health.SetMaxHealth(maxHp, true);
    }

    private void OnPlayerShotFired()
    {
        _lastPlayerShotTime = Time.time;
    }

    private void Update()
    {
        if (health != null && !health.IsAlive)
            return;
        if (firePoint == null || enemyBulletPrefab == null)
            return;
        if (_isReloading)
            return;
        if (Time.time < _nextDeflectCheckTime || Time.time < _nextDeflectTime)
            return;

        _nextDeflectCheckTime = Time.time + deflectCheckIntervalSeconds;

        if (!TryDeflectIncomingPlayerBullet())
            return;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, deflectAwarenessRadius);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, directThreatRadius);
    }

    private bool TryDeflectIncomingPlayerBullet()
    {
        int minAmmoToShoot = keepOneBulletInChamber ? 2 : 1;
        if (_ammo < minAmmoToShoot)
            return false;

        Vector2 aiPos = transform.position;
        PlayerBulletProjectile best = null;
        float bestT = float.MaxValue;
        bool bestDirect = false;

        foreach (var b in PlayerBulletProjectile.Active)
        {
            if (b == null)
                continue;
            var rb = b.Rigidbody;
            if (rb == null)
                continue;

            Vector2 v = rb.linearVelocity;
            float speedSq = v.sqrMagnitude;
            if (speedSq < 0.1f)
                continue;

            Vector2 p = rb.position;
            Vector2 r = aiPos - p;
            float t = -Vector2.Dot(r, v) / speedSq;
            if (t < 0f)
                continue;
            if (t > maxThreatLookaheadSeconds)
                continue;

            Vector2 closest = p + v * t;
            float dist = Vector2.Distance(closest, aiPos);
            if (dist > deflectAwarenessRadius)
                continue;

            bool direct = dist <= directThreatRadius;
            if (t < bestT)
            {
                bestT = t;
                best = b;
                bestDirect = direct;
            }
        }

        if (best == null)
            return false;

        float chance = bestDirect ? _deflectChanceDirect : _deflectChanceOffTarget;
        if (Random.value > chance)
            return false;

        Vector2 bulletPos = best.Rigidbody != null ? best.Rigidbody.position : (Vector2)best.transform.position;
        Vector2 dir = (bulletPos - (Vector2)firePoint.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        FireOneShotDirected(dir);
        _ammo--;
        _nextDeflectTime = Time.time + deflectCooldownSeconds;

        if (debugLogs)
            Debug.Log($"[{nameof(AICombatController)}] Deflect shot. Direct={bestDirect} chance={chance:0.00} t={bestT:0.00} ammo={_ammo}", this);

        return true;
    }

    private IEnumerator CombatLoop()
    {
        if (debugLogs)
            Debug.Log($"[{nameof(AICombatController)}] CombatLoop running.", this);

        while (enabled)
        {
            if (health != null && !health.IsAlive)
            {
                if (debugLogs)
                    Debug.Log($"[{nameof(AICombatController)}] Exiting loop: dead.", this);
                yield break;
            }

            bool playerReloading = playerWeapon != null && playerWeapon.IsReloading;
            if (playerReloading && !_lastPlayerReloading)
                OnPlayerReloadStarted();
            _lastPlayerReloading = playerReloading;

            if (_isReloading)
            {
                if (debugLogs)
                    Debug.Log($"[{nameof(AICombatController)}] Reloading... Ammo={_ammo}", this);

                if (playerReloading)
                    TrySurpriseShotDuringReload();
                yield return null;
                continue;
            }

            if (keepOneBulletInChamber && _ammo <= 1)
            {
                if (Time.time - _lastMeaningfulActionTime >= pressureIntervalSeconds)
                {
                    if (debugLogs)
                        Debug.Log($"[{nameof(AICombatController)}] Low ammo stall fallback: reloading from {_ammo}.", this);
                    yield return StartCoroutine(ReloadRoutine());
                    _lastMeaningfulActionTime = Time.time;
                    continue;
                }
            }

            if (tier == AITier.Level3 || tier == AITier.Level4)
            {
                if (Time.time - _lastPressureTime >= pressureIntervalSeconds)
                {
                    int shots = Random.Range(pressureMinShots, pressureMaxShots + 1);
                    if (debugLogs)
                        Debug.Log($"[{nameof(AICombatController)}] Pressure fire. Shots={shots}", this);
                    FireBurst(shots, allowRandomIfNotAimed: false);
                    _lastMeaningfulActionTime = Time.time;
                    _lastPressureTime = Time.time;
                    yield return null;
                    continue;
                }

                if (Random.value < waitForPlayerShotProbability && Time.time - _lastPlayerShotTime > waitTickSeconds)
                {
                    yield return new WaitForSeconds(waitTickSeconds);
                    continue;
                }
            }

            if (ShouldStartReload())
            {
                yield return StartCoroutine(ReloadRoutine());
                _lastMeaningfulActionTime = Time.time;
                continue;
            }

            float wait = Random.Range(minShotIntervalSeconds, maxShotIntervalSeconds);
            if (debugLogs)
                Debug.Log($"[{nameof(AICombatController)}] Waiting {wait:0.00}s before next shot. Ammo={_ammo}", this);
            yield return new WaitForSeconds(wait);

            if (health != null && !health.IsAlive)
            {
                if (debugLogs)
                    Debug.Log($"[{nameof(AICombatController)}] Exiting loop: dead (post-wait).", this);
                yield break;
            }

            if (_ammo <= 0)
                continue;

            int burst = Random.Range(minBurstShots, maxBurstShots + 1);
            FireBurst(burst, allowRandomIfNotAimed: true);
            _lastMeaningfulActionTime = Time.time;
        }
    }

    private void OnPlayerReloadStarted()
    {
        if (target == null || playerWeapon == null)
            return;

        if (Time.time < _nextPunishShotTime)
            return;

        if (_ammo <= 1)
        {
            if (debugLogs)
                Debug.Log($"[{nameof(AICombatController)}] Saw player reload with low ammo ({_ammo}): prioritizing reload.", this);
            StartReloadNow();
            return;
        }

        if (tier == AITier.Level3 || tier == AITier.Level4)
        {
            if (Random.value < startReloadWhenPlayerReloadsChance)
            {
                if (debugLogs)
                    Debug.Log($"[{nameof(AICombatController)}] Saw player reload: choosing to reload (contest).", this);
                StartReloadNow();
            }
            else if (Random.value < punishPlayerReloadWithShotChance)
            {
                if (debugLogs)
                    Debug.Log($"[{nameof(AICombatController)}] Saw player reload: choosing to punish with a shot.", this);
                FireBurst(1, allowRandomIfNotAimed: false);
                _nextPunishShotTime = Time.time + punishShotCooldownSeconds;
            }
            return;
        }

        if (tier == AITier.Level2)
        {
            if (Random.value < punishPlayerReloadWithShotChance)
            {
                if (debugLogs)
                    Debug.Log($"[{nameof(AICombatController)}] Saw player reload (L2): punish shot.", this);
                FireBurst(1, allowRandomIfNotAimed: false);
                _nextPunishShotTime = Time.time + punishShotCooldownSeconds;
            }
        }
    }

    private void StartReloadNow()
    {
        if (_isReloading)
            return;
        if (_ammo >= magazineSize)
            return;
        _isReloading = true;
        StartCoroutine(ReloadRoutine());
    }

    private void TrySurpriseShotDuringReload()
    {
        if (Time.time < _nextPunishShotTime)
            return;
        if (_ammo <= 0)
            return;

        float chanceThisFrame = surpriseShotDuringReloadChancePerSecond * Time.deltaTime;
        if (chanceThisFrame <= 0f)
            return;

        if (Random.value < chanceThisFrame)
        {
            if (debugLogs)
                Debug.Log($"[{nameof(AICombatController)}] Surprise shot during reload.", this);
            FireBurst(1, allowRandomIfNotAimed: false);
            _nextPunishShotTime = Time.time + punishShotCooldownSeconds;
        }
    }

    private bool ShouldStartReload()
    {
        if (_ammo <= 0)
            return true;

        if (reloadWhenAmmoAtOrBelow > 0 && _ammo <= reloadWhenAmmoAtOrBelow)
            return true;

        return false;
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;

        int targetAmmo = reloadToFullAlways ? magazineSize : magazineSize;
        if (!reloadToFullAlways && keepOneBulletInChamber && _ammo > 1)
            targetAmmo = magazineSize;

        GameAudioManager.Instance?.PlayAiReloadStart();
        GameAudioManager.Instance?.PlayAiReloadEmptyChamber();

        bool diedMidReload = false;
        while (_ammo < targetAmmo)
        {
            if (health != null && !health.IsAlive)
            {
                diedMidReload = true;
                break;
            }

            yield return new WaitForSecondsRealtime(reloadSecondsPerBullet);
            _ammo = Mathf.Min(magazineSize, _ammo + 1);
            GameAudioManager.Instance?.PlayAiReloadLoadBullet();

            if (!reloadToFullAlways && keepOneBulletInChamber && _ammo >= 1 && Random.value < 0.25f)
                break;
        }

        if (!diedMidReload)
            GameAudioManager.Instance?.PlayAiReloadEnd();
        _isReloading = false;
    }

    private void FireBurst(int shots, bool allowRandomIfNotAimed)
    {
        if (shots <= 0)
            return;

        int allowed = shots;
        if (keepOneBulletInChamber)
            allowed = Mathf.Min(allowed, Mathf.Max(0, _ammo - 1));
        else
            allowed = Mathf.Min(allowed, _ammo);

        for (int i = 0; i < allowed; i++)
        {
            if (!FireOneShot(allowRandomIfNotAimed))
                return;
            _ammo--;
            if (_ammo <= 0)
                return;
        }
    }

    private bool FireOneShot(bool allowRandomIfNotAimed)
    {
        if (enemyBulletPrefab == null || firePoint == null)
        {
            if (debugLogs)
                Debug.LogWarning($"[{nameof(AICombatController)}] Cannot fire: enemyBulletPrefab or firePoint missing.", this);
            return false;
        }

        Vector2 dir;
        bool aimed = target != null && Random.value < aimAtTargetChance;
        if (aimed)
        {
            Vector2 baseDir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
            if (baseDir.sqrMagnitude < 0.0001f)
                baseDir = Vector2.right;

            float angle = Random.Range(-aimSpreadDegrees, aimSpreadDegrees);
            dir = (Quaternion.Euler(0f, 0f, angle) * baseDir).normalized;
        }
        else
        {
            if (!allowRandomIfNotAimed)
                return false;

            dir = Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude < 0.01f)
                dir = Vector2.right;
        }

        var go = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        if (debugLogs)
            Debug.Log($"[{nameof(AICombatController)}] Fired. Aimed={aimed} Spawned={(go != null ? go.name : "null")} Dir={dir}", this);
        if (go.TryGetComponent(out EnemyProjectile proj))
        {
            proj.SetDamage(_bulletDamage);
            proj.Launch(dir);
        }
        ShotFired?.Invoke();
        return true;
    }

    private void FireOneShotDirected(Vector2 direction)
    {
        if (enemyBulletPrefab == null || firePoint == null)
            return;

        var go = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        if (go.TryGetComponent(out EnemyProjectile proj))
        {
            proj.SetDamage(_bulletDamage);
            proj.Launch(direction);
        }
        ShotFired?.Invoke();
    }
}
