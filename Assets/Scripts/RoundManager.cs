using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private BulletWaveSpawner bulletWaveSpawner;
    [SerializeField] private ReticleBehaviour reticleBehaviour;
    [SerializeField] private Slider matchTimerSlider;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI livesText;

    [Header("Round rules")]
    [SerializeField] private float roundDurationSeconds = 10f;
    [SerializeField] private int initialWaveBulletCount = 3;
    [SerializeField] private int extraBulletsPerRoundCleared = 1;
    [SerializeField] private float delayBeforeNextRoundSeconds = 1f;

    [Header("Input / physics")]
    [SerializeField] private LayerMask bulletLayers;

    [Header("Lives")]
    [SerializeField] private int startingLives = 3;

    private int roundIndex;
    private int bulletsRemaining;
    private float timeRemaining;
    private bool roundActive;
    private bool gameOver;
    private int livesRemaining;

    public void StartMatchAfterIntro()
    {
        gameOver = false;
        roundIndex = 0;
        livesRemaining = startingLives;
        RefreshLivesUI();

        reticleBehaviour?.ActivateReticle();
        BeginRoundSpawnAndTimer();
    }

    private int BulletCountForCurrentRound()
    {
        return initialWaveBulletCount + roundIndex * extraBulletsPerRoundCleared;
    }
    
    private void BeginRoundSpawnAndTimer()
    {
        if (gameOver)
        {
            return;
        }

        DestroyRemainingBullets();

        int requested = BulletCountForCurrentRound();
        bulletsRemaining = bulletWaveSpawner.SpawnWave(requested);
        timeRemaining = roundDurationSeconds;
        roundActive = bulletsRemaining > 0;
        RefreshSliderVisual();
    }

    private void Update()
    {
        if (gameOver || !roundActive)
        {
            return;
        }
          
        timeRemaining -= Time.deltaTime;
        RefreshSliderVisual();

        if (timeRemaining <= 0f)
        {
            if (bulletsRemaining > 0)
                LoseRoundRestart();
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            EliminateBulletUnderCursor();
        }
            
    }

    private void EliminateBulletUnderCursor()
    {
        if (gameplayCamera == null || Mouse.current == null)
        {
            return;
        }
            
        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 world = gameplayCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -gameplayCamera.transform.position.z));
        world.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(world, bulletLayers);
        if (hit == null)
        {
            return;
        }

        var target = hit.GetComponent<BulletTarget>() ?? hit.GetComponentInParent<BulletTarget>();
        if (target == null)
        {
            return;
        }
            
        Destroy(target.gameObject);
        bulletsRemaining = Mathf.Max(0, bulletsRemaining - 1);

        if (bulletsRemaining <= 0)
        {
            StartCoroutine(RoundClearedRoutine());
        }
    }

    private IEnumerator RoundClearedRoutine()
    {
        roundActive = false;
        yield return new WaitForSeconds(delayBeforeNextRoundSeconds);

        roundIndex++;
        BeginRoundSpawnAndTimer();
    }

    private void LoseRoundRestart()
    {
        roundActive = false;

        livesRemaining--;
        RefreshLivesUI();

        if (livesRemaining <= 0)
        {
            TriggerGameOver();
            return;
        }

        BeginRoundSpawnAndTimer();
    }

    private static void DestroyRemainingBullets()
    {
        var targets = FindObjectsByType<BulletTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < targets.Length; i++)
        {
            Destroy(targets[i].gameObject);
        }
    }

    private void RefreshSliderVisual()
    {
        if (matchTimerSlider == null)
            return;
        matchTimerSlider.minValue = 0f;
        matchTimerSlider.maxValue = roundDurationSeconds;
        matchTimerSlider.value = Mathf.Clamp(timeRemaining, 0f, roundDurationSeconds);
    }

    private void RefreshLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {livesRemaining}";
        }
            
    }

    private void TriggerGameOver()
    {
        gameOver = true;
        roundActive = false;
        Time.timeScale = 0f;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
    }
}
