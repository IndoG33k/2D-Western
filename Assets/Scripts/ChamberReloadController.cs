using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChamberReloadController : MonoBehaviour
{
    public enum SessionPhase
    {
        DumpCasings,
        Loading,
    }

    [SerializeField] private PlayerWeaponController weapon;
    [SerializeField] private DeadeyeController deadeye;

    [Header("Presentation")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Camera chamberCamera;
    [SerializeField] private GameObject chamberViewRoot;

    [Header("Chamber UI")]
    [SerializeField] private Button[] chamberLoadButtons;
    [SerializeField] private GameObject dumpPromptVisual;
    [SerializeField] private GameObject loadPromptVisual;

    [Header("Input")]
    [SerializeField] private string dumpCasingsPath = "<Keyboard>/space";
    [SerializeField] private string closeReloadPath = "<Keyboard>/r";

    public bool IsSessionActive { get; private set; }

    public event Action<SessionPhase> PhaseChanged;

    private SessionPhase _sessionPhase;
    private readonly bool[] _slotLoaded = new bool[32];
    private int _slotCount;
    private InputAction _dumpAction;
    private InputAction _closeReloadAction;
    private bool _suppressWeaponReloadFromCloseThisFrame;

    public bool ConsumeSuppressWeaponReloadPress()
    {
        if (!_suppressWeaponReloadFromCloseThisFrame)
            return false;
        _suppressWeaponReloadFromCloseThisFrame = false;
        return true;
    }

    private void Awake()
    {
        _dumpAction = new InputAction(type: InputActionType.Button, binding: dumpCasingsPath);
        _closeReloadAction = new InputAction(type: InputActionType.Button, binding: closeReloadPath);

        for (int i = 0; i < chamberLoadButtons.Length && i < _slotLoaded.Length; i++)
        {
            int index = i;
            if (chamberLoadButtons[i] != null)
            {
                chamberLoadButtons[i].onClick.AddListener(() => OnChamberButtonClicked(index));
            }
        }
    }

    private void OnEnable()
    {
        _dumpAction.Enable();
        _closeReloadAction.Enable();
    }

    private void OnDisable()
    {
        _dumpAction.Disable();
        _closeReloadAction.Disable();
    }

    private void OnDestroy()
    {
        _dumpAction?.Dispose();
        _closeReloadAction?.Dispose();
    }

    private void Update()
    {
        if (!IsSessionActive || weapon == null)
        {
            return;
        }

        if (_sessionPhase == SessionPhase.DumpCasings && _dumpAction.WasPressedThisFrame())
        {
            EnterLoadingPhase();
            return;
        }

        if (_sessionPhase == SessionPhase.Loading && _closeReloadAction.WasPressedThisFrame())
        {
            FinishAndApplyAmmo();
        }
    }

    public bool TryBeginReload()
    {
        if (weapon == null || IsSessionActive)
        {
            return false;
        }
        if (weapon.CurrentAmmo >= weapon.MaxAmmoCapacity)
        {
            return false;
        }

        _slotCount = Mathf.Min(weapon.MaxAmmoCapacity, chamberLoadButtons.Length, _slotLoaded.Length);
        if (_slotCount <= 0)
        {
            Debug.LogWarning($"{nameof(ChamberReloadController)}: need at least one chamber button (size {weapon.MaxAmmoCapacity}).");
            return false;
        }

        weapon.EnterReloadMode();
        if (deadeye != null && deadeye.IsActive)
        {
            deadeye.ForceExitDeadeye();
        }

        SetChamberPresentation(true);

        int live = Mathf.Clamp(weapon.CurrentAmmo, 0, _slotCount);
        for (int i = 0; i < _slotCount; i++)
        {
            _slotLoaded[i] = i < live;
            RefreshSlotVisual(i, _slotLoaded[i]);
        }

        IsSessionActive = true;
        _sessionPhase = SessionPhase.DumpCasings;
        RefreshPrompts();
        RefreshSlotInteractable();
        PhaseChanged?.Invoke(_sessionPhase);
        return true;
    }

    public void InterruptPreserveLoadedAmmo()
    {
        if (!IsSessionActive)
        {
            return;
        }

        int count = CountLoadedSlots();
        weapon.ExitReloadModeWithAmmo(count);
        EndSessionPresentation();
    }

    private void EnterLoadingPhase()
    {
        _sessionPhase = SessionPhase.Loading;
        RefreshPrompts();
        RefreshSlotInteractable();
        PhaseChanged?.Invoke(_sessionPhase);
    }

    private void OnChamberButtonClicked(int index)
    {
        if (!IsSessionActive || _sessionPhase != SessionPhase.Loading || index < 0 || index >= _slotCount)
        {
            return;
        }
        if (_slotLoaded[index])
        {
            return;
        }

        _slotLoaded[index] = true;
        RefreshSlotVisual(index, true);
    }

    private void FinishAndApplyAmmo()
    {
        _suppressWeaponReloadFromCloseThisFrame = true;

        int count = CountLoadedSlots();
        weapon.ExitReloadModeWithAmmo(count);
        EndSessionPresentation();
    }

    private int CountLoadedSlots()
    {
        int n = 0;
        for (int i = 0; i < _slotCount; i++)
            if (_slotLoaded[i])
            {
                n++;
            }
        return n;
    }

    private void RefreshSlotInteractable()
    {
        for (int i = 0; i < chamberLoadButtons.Length; i++)
        {
            if (chamberLoadButtons[i] == null)
            {
                continue;
            }
            bool canClick = IsSessionActive && _sessionPhase == SessionPhase.Loading && i < _slotCount && !_slotLoaded[i];
            chamberLoadButtons[i].interactable = canClick;
        }
    }

    private void RefreshSlotVisual(int index, bool filled)
    {
        if (index < 0 || index >= chamberLoadButtons.Length || chamberLoadButtons[index] == null)
        {
            return;
        }

        var colors = chamberLoadButtons[index].colors;
        colors.normalColor = filled ? new Color(0.85f, 0.65f, 0.2f) : new Color(0.35f, 0.35f, 0.38f);
        colors.highlightedColor = colors.normalColor;
        colors.selectedColor = colors.normalColor;
        chamberLoadButtons[index].colors = colors;
    }

    private void RefreshPrompts()
    {
        if (dumpPromptVisual != null)
        {
            dumpPromptVisual.SetActive(IsSessionActive && _sessionPhase == SessionPhase.DumpCasings);
        }
        if (loadPromptVisual != null)
        {
            loadPromptVisual.SetActive(IsSessionActive && _sessionPhase == SessionPhase.Loading);
        }
    }

    private void SetChamberPresentation(bool chamberView)
    {
        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = true;
        }

        if (chamberViewRoot != null)
        {
            chamberViewRoot.SetActive(chamberView);
        }

        if (chamberCamera != null)
        {
            chamberCamera.gameObject.SetActive(chamberView);
            chamberCamera.enabled = chamberView;
        }
    }

    private void EndSessionPresentation()
    {
        IsSessionActive = false;
        SetChamberPresentation(false);
        RefreshPrompts();
        RefreshSlotInteractable();
    }
}
