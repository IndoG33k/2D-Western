using UnityEngine;
using UnityEngine.UI;

public class DeadeyeMeter : MonoBehaviour
{
    [SerializeField] private DeadeyeController deadeye;
    [SerializeField] private Slider meterSlider;

    private void Update()
    {
        if (deadeye == null || meterSlider == null)
            return;

        meterSlider.minValue = 0f;
        meterSlider.maxValue = deadeye.MaxMeter;
        meterSlider.value = deadeye.CurrentMeter;
    }
}
