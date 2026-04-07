using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private PlayerWeaponController weapon;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private string format = "{0}/{1}";

    private void OnEnable()
    {
        if (weapon != null)
        {
            weapon.AmmoChanged += OnAmmoChanged;
        }
    }

    private void OnDisable()
    {
        if (weapon != null)
        {
            weapon.AmmoChanged -= OnAmmoChanged;
        }
    }

    private void Start()
    {
        if (weapon != null)
        {
            OnAmmoChanged(weapon.CurrentAmmo, weapon.MaxAmmoCapacity);
        }
    }

    private void OnAmmoChanged(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + string.Format(format, current, max);
        }
    }
}
