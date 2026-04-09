using UnityEngine;

/// <summary>
/// Plays deadeye start/end SFX and ducks music via GameAudioManager while Deadeye is active.
/// Attach next to DeadeyeController (e.g. on the player).
/// </summary>
public class DeadeyeAudioDuck : MonoBehaviour
{
    [SerializeField] private DeadeyeController deadeye;

    private void Awake()
    {
        if (deadeye == null)
            deadeye = GetComponent<DeadeyeController>();
    }

    private void OnEnable()
    {
        if (deadeye != null)
        {
            deadeye.DeadeyeStarted += OnDeadeyeStarted;
            deadeye.DeadeyeEnded += OnDeadeyeEnded;
        }
    }

    private void OnDisable()
    {
        if (deadeye != null)
        {
            deadeye.DeadeyeStarted -= OnDeadeyeStarted;
            deadeye.DeadeyeEnded -= OnDeadeyeEnded;
        }

        GameAudioManager.Instance?.SetMusicDucked(false);
    }

    private void OnDeadeyeStarted()
    {
        GameAudioManager.Instance?.PlayDeadeyeStart();
        GameAudioManager.Instance?.SetMusicDucked(true);
    }

    private void OnDeadeyeEnded()
    {
        GameAudioManager.Instance?.PlayDeadeyeEnd();
        GameAudioManager.Instance?.SetMusicDucked(false);
    }
}
