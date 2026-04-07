using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeadeyeController : MonoBehaviour
{
    [Header("Meter settings")]
    [SerializeField] private float maxMeter = 100f;
    [SerializeField] private float startupCost = 20f;
    [SerializeField] private float drainPerSecond = 15f;
    [SerializeField] private float refillPerSecond = 10f;

    [Header("Slow motion")]
    [SerializeField] [Range(0.05f, 1f)] private float timeScaleWhileActive = 0.35f;

    [Header("Input")]
    [SerializeField] private string deadeyePath = "<Keyboard>/q";

    [Header("Dependencies")]
    [SerializeField] private MonoBehaviour reloadBlocker;

    public bool IsActive { get; private set; }
    public float MeterNormalized => maxMeter <= 0f ? 0f : _meter / maxMeter;
    public float CurrentMeter => _meter;
    public float MaxMeter => maxMeter;

    public event Action DeadeyeStarted;
    public event Action DeadeyeEnded;

    private float _meter;
    private InputAction _deadeyeAction;
    private IReloadBlocker _reload;

    private void Awake()
    {
        _meter = maxMeter;
        _deadeyeAction = new InputAction(type: InputActionType.Button, binding: deadeyePath);
        _reload = reloadBlocker as IReloadBlocker;
    }

    private void OnEnable()
    {
        _deadeyeAction.Enable();
    }

    private void OnDisable()
    {
        _deadeyeAction.Disable();
    }

    private void OnDestroy()
    {
        _deadeyeAction.Dispose();
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (IsActive)
        {
            _meter -= drainPerSecond * dt;

            if (_meter <= 0f)
            {
                _meter = 0f;
                ForceExitDeadeye();
            }
        }
        else
        {
            _meter = Mathf.Min(maxMeter, _meter + refillPerSecond * dt);
        }

        if (_deadeyeAction.WasPressedThisFrame())
        {
            if (IsActive)
                ForceExitDeadeye();
            else
                TryEnterDeadeye();
        }
    }

    public bool TryEnterDeadeye()
    {
        if (IsActive)
        {
            return true;
        }

        if (_reload != null && _reload.IsReloading)
        {
            return false;
        }

        if (_meter < startupCost)
        {
            return false;
        }

        _meter -= startupCost;
        IsActive = true;
        Time.timeScale = timeScaleWhileActive;
        DeadeyeStarted?.Invoke();
        return true;
    }

    public void ForceExitDeadeye()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Time.timeScale = 1f;
        DeadeyeEnded?.Invoke();
    }
}

public interface IReloadBlocker
{
    bool IsReloading { get; }
}
