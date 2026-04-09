using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Countdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private int countdownTime = 3;
    [SerializeField] private float countdownTick = 1f;
    [SerializeField] private bool hideWhenDone = true;
    [SerializeField] private UnityEvent onFinished;


    public IEnumerator Run()
    {
        if (countdownText == null)
        {
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        for (int i = countdownTime; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(countdownTick);
        }

        if (hideWhenDone)
        {
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            countdownText.text = string.Empty;
        }
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownThenInvoke());
    }

    private IEnumerator CountdownThenInvoke()
    {
        yield return Run();
        onFinished.Invoke();
    }
}
