using UnityEngine;
using System.Collections;

public class GameBootSequence : MonoBehaviour
{
    [SerializeField] private Countdown fightCountdown;
    [SerializeField] private ReticleBehaviour reticleBehaviour;
    [SerializeField] private RoundManager roundManager;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        StartCoroutine(RunOpeningSequence());
    }
    
    private IEnumerator RunOpeningSequence()
    {
        reticleBehaviour?.PrepareForRound();

        if (fightCountdown != null)
        {
            yield return fightCountdown.Run();
        }

        roundManager?.StartMatchAfterIntro();
    }
}
