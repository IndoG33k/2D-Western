using UnityEngine;
using System.Collections;

public class GameBootSequence : MonoBehaviour
{
    [SerializeField] private Countdown fightCountdown;
    [SerializeField] private BulletWaveSpawner bulletWaveSpawner;
    [SerializeField] private ReticleBehaviour reticleBehaviour;
    [SerializeField] private int initialWaveCount = 3;

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

        bulletWaveSpawner?.SpawnWave(initialWaveCount);
        reticleBehaviour?.ActivateReticle();
    }
}
