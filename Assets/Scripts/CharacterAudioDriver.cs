using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CharacterAudioDriver : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AudioSet audioSet;

    [Header("Dependencies")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Health health;
    [SerializeField] private PlayerWeaponController playerWeapon;
    [SerializeField] private AICombatController aiCombat;

    [Header("Variation")]
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (health == null)
            health = GetComponent<Health>();
        if (playerWeapon == null)
            playerWeapon = GetComponent<PlayerWeaponController>();
        if (aiCombat == null)
            aiCombat = GetComponent<AICombatController>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged.AddListener(OnDamaged);
            health.OnDeath.AddListener(OnDeath);
        }

        if (playerWeapon != null)
            playerWeapon.ShotFired += OnShotFired;
        if (aiCombat != null)
            aiCombat.ShotFired += OnShotFired;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged.RemoveListener(OnDamaged);
            health.OnDeath.RemoveListener(OnDeath);
        }

        if (playerWeapon != null)
            playerWeapon.ShotFired -= OnShotFired;
        if (aiCombat != null)
            aiCombat.ShotFired -= OnShotFired;
    }

    public void SetAudioSet(AudioSet set)
    {
        audioSet = set;
    }

    private void OnShotFired()
    {
        PlayRandom(audioSet != null ? audioSet.shootClips : null);
    }

    private void OnDamaged(int _)
    {
        PlayRandom(audioSet != null ? audioSet.hitClips : null);
    }

    private void OnDeath()
    {
        PlayRandom(audioSet != null ? audioSet.deathClips : null);
    }

    public void PlayBulletClink()
    {
        GameAudioManager.Instance?.PlayBulletClink();
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return;

        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
            return;

        if (randomizePitch)
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
        else
            audioSource.pitch = 1f;

        audioSource.PlayOneShot(clip);
    }
}
