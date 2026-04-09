using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health target;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private string format = "{0} / {1}";

    private void OnEnable()
    {
        if (target != null)
            target.OnHealthChanged.AddListener(OnHealthChanged);
    }

    private void OnDisable()
    {
        if (target != null)
            target.OnHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void Start()
    {
        if (target != null)
            OnHealthChanged(target.CurrentHealth, target.MaxHealth);
    }

    private void OnHealthChanged(int current, int max)
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = max;
            slider.value = current;
        }

        if (textLabel != null)
            textLabel.text = string.Format(format, current, max);
    }
}
