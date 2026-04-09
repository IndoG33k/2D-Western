using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour, IReloadBlocker
{
    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private bool startFullyLoaded = true;

    [Header("Firing")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float fireCooldownSeconds = 0.25f;

    [Header("Reload")]
    [SerializeField] private ChamberReloadController chamberReload;

    [Header("Input bindings")]
    [SerializeField] private string aimPath = "<Mouse>/rightButton";
    [SerializeField] private string firePath = "<Mouse>/leftButton";
    [SerializeField] private string reloadPath = "<Keyboard>/r";

    [Header("Dependencies")]
    [SerializeField] private DeadeyeController deadeye;

    public bool IsReloading { get; private set; }
    public bool IsAiming { get; private set; }
    public int CurrentAmmo { get; private set; }

    public int MaxAmmoCapacity => maxAmmo;

    public event Action<int, int> AmmoChanged;
    public event Action ShotFired;

    private float _nextFireTime;
    private InputAction _aimAction;
    private InputAction _fireAction;
    private InputAction _reloadAction;

    private void RaiseAmmoChanged() => AmmoChanged?.Invoke(CurrentAmmo, maxAmmo);

    private void Awake()
    {
        _aimAction = new InputAction(type: InputActionType.Button, binding: aimPath);
        _fireAction = new InputAction(type: InputActionType.Button, binding: firePath);
        _reloadAction = new InputAction(type: InputActionType.Button, binding: reloadPath);

        if(aimCamera == null)
        {
            aimCamera = Camera.main;
        }
    }

    private void Start()
    {
        CurrentAmmo = startFullyLoaded ? maxAmmo : 0;
        RaiseAmmoChanged();
    }

    private void OnEnable()
    {
        _aimAction.Enable();
        _fireAction.Enable();
        _reloadAction.Enable();
    }

    private void OnDisable()
    {
        _aimAction.Disable();
        _fireAction.Disable();
        _reloadAction.Disable();
        IsAiming = false;
    }

    private void OnDestroy()
    {
        _aimAction?.Dispose();
        _fireAction?.Dispose();
        _reloadAction?.Dispose();
    }

    private void Update()
    {
        if (IsReloading)
        {
            IsAiming = false;
            return;
        }


        IsAiming = _aimAction.IsPressed();

        if (!IsAiming)
        {
            return;
        }
            
        if (_fireAction.WasPressedThisFrame() && Time.time >= _nextFireTime)
        {
            TryFire();
        }     
    }

    private void TryFire()
    {
        if (CurrentAmmo <= 0)
        {
            return;
        }

        if (muzzle == null || bulletPrefab == null || aimCamera == null)
        {
            return;
        }

        Vector3 world = aimCamera.ScreenToWorldPoint(
          new Vector3(Mouse.current.position.ReadValue().x,
              Mouse.current.position.ReadValue().y,
              -aimCamera.transform.position.z));

        world.z = 0f;

        Vector2 dir = ((Vector2)world - (Vector2)muzzle.position).normalized;

        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = transform.right;
        }

        bool slowUntilDeadeyeEnds = deadeye != null && deadeye.IsActive;

        GameObject go = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        if (go.TryGetComponent(out PlayerBulletProjectile proj))
        {
            proj.Launch(dir, slowUntilDeadeyeEnds, deadeye);
        }
        ShotFired?.Invoke();

        CurrentAmmo--;
        RaiseAmmoChanged();
        _nextFireTime = Time.time + fireCooldownSeconds;
    }

    private void LateUpdate()
    {
        if (chamberReload != null && chamberReload.IsSessionActive)
            return;

        if (!_reloadAction.WasPressedThisFrame())
            return;

        if (chamberReload != null && chamberReload.ConsumeSuppressWeaponReloadPress())
            return;

        TryStartChamberReload();
    }

    private void TryStartChamberReload()
    {
        if (IsReloading || CurrentAmmo >= maxAmmo)
        {
            return;
        }
            
        if (chamberReload != null && chamberReload.TryBeginReload())
        {
            return;
        }

        StartCoroutine(FallbackTimedReloadRoutine());
    }

    private IEnumerator FallbackTimedReloadRoutine()
    {
        IsReloading = true;
        IsAiming = false;

        if (deadeye != null && deadeye.IsActive)
        {
            deadeye.ForceExitDeadeye();
        }

        GameAudioManager.Instance?.PlayPlayerReloadStart();
        yield return new WaitForSecondsRealtime(2f);

        CurrentAmmo = maxAmmo;
        IsReloading = false;
        RaiseAmmoChanged();
        GameAudioManager.Instance?.PlayPlayerReloadEnd();
    }

    public void EnterReloadMode()
    {
        IsReloading = true;
        IsAiming = false;

        if (deadeye != null && deadeye.IsActive)
        {
            deadeye.ForceExitDeadeye();
        }
    }

    public void ExitReloadModeWithAmmo(int newAmmoCount)
    {
        CurrentAmmo = Mathf.Clamp(newAmmoCount, 0, maxAmmo);
        IsReloading = false;

        RaiseAmmoChanged();
    }
}
