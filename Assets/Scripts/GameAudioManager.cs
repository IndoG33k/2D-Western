using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Central audio routing: SFX / UI / music. Intended as DontDestroyOnLoad (place in first scene or on RunProgression).
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private AudioSet globalAudioSet;
    [SerializeField] private MusicLibrary musicLibrary;

    [Header("Mixer (expose one Float, e.g. Music group volume)")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private float musicVolumeNormalDb = 0f;
    [SerializeField] private float musicVolumeDuckedDb = -24f;
    [SerializeField] private float musicDuckFadeSeconds = 0.12f;

    [Header("Output groups (optional)")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource musicSource;

    private bool _level1IntroPlayedThisRun;
    private Coroutine _musicRoutine;
    private Coroutine _duckRoutine;
    private bool _musicDucked;
    private bool _forceNextBattleMusicRestart;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        if (sfxGroup != null)
            sfxSource.outputAudioMixerGroup = sfxGroup;
        if (uiGroup != null)
            uiSource.outputAudioMixerGroup = uiGroup;
        if (musicGroup != null)
            musicSource.outputAudioMixerGroup = musicGroup;

        ApplyMixerMusicVolume(musicVolumeNormalDb);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ResetRunAudioState()
    {
        _level1IntroPlayedThisRun = false;
        _forceNextBattleMusicRestart = false;
        StopBattleMusicInternal(resetDuck: true);
    }

    /// <summary>Stop battle music (e.g. main menu, quit). Full run reset also calls this via ResetRunAudioState.</summary>
    public void StopBattleMusic()
    {
        StopBattleMusicInternal(resetDuck: true);
    }

    private void StopBattleMusicInternal(bool resetDuck)
    {
        if (_musicRoutine != null)
        {
            StopCoroutine(_musicRoutine);
            _musicRoutine = null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        if (resetDuck)
        {
            _musicDucked = false;
            if (_duckRoutine != null)
            {
                StopCoroutine(_duckRoutine);
                _duckRoutine = null;
            }

            ApplyMixerMusicVolume(musicVolumeNormalDb);
            if (musicSource != null)
                musicSource.volume = 1f;
        }
    }

    public void PauseBattleMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Pause();
    }

    /// <summary>
    /// Stops the Level1 intro coroutine if running, then pauses the music source.
    /// Use for game over / win (realtime intro coroutine would otherwise advance while timeScale is 0).
    /// </summary>
    public void PauseBattleMusicForModal()
    {
        if (_musicRoutine != null)
        {
            StopCoroutine(_musicRoutine);
            _musicRoutine = null;
        }

        PauseBattleMusic();
    }

    public void ResumeBattleMusic()
    {
        if (musicSource != null && musicSource.clip != null)
            musicSource.UnPause();
    }

    public void RequestForceRestartBattleMusic()
    {
        _forceNextBattleMusicRestart = true;
    }

    public bool ConsumeForceRestartBattleMusic()
    {
        if (!_forceNextBattleMusicRestart)
            return false;
        _forceNextBattleMusicRestart = false;
        return true;
    }

    public void BindGlobalAudioSet(AudioSet set)
    {
        globalAudioSet = set;
    }

    public void ApplyMusicForTier(AITier tier, bool forceRestart = false)
    {
        if (musicLibrary == null || musicSource == null)
            return;

        if (_musicRoutine != null)
        {
            StopCoroutine(_musicRoutine);
            _musicRoutine = null;
        }

        if (forceRestart && musicSource.isPlaying)
            musicSource.Stop();

        switch (tier)
        {
            case AITier.Level1:
                if (!_level1IntroPlayedThisRun && musicLibrary.level1Intro != null)
                    _musicRoutine = StartCoroutine(Level1IntroThenLoopRoutine());
                else
                    PlayMusicLoop(musicLibrary.level1Loop, forceRestart);
                break;
            case AITier.Level2:
                PlayMusicLoop(musicLibrary.level2Loop, forceRestart);
                break;
            case AITier.Level3:
                PlayMusicLoop(musicLibrary.level3Loop, forceRestart);
                break;
            case AITier.Level4:
                PlayMusicLoop(musicLibrary.level4Loop, forceRestart);
                break;
        }

        if (musicSource != null && musicSource.clip != null)
            musicSource.UnPause();
    }

    private IEnumerator Level1IntroThenLoopRoutine()
    {
        _level1IntroPlayedThisRun = true;
        var intro = musicLibrary.level1Intro;
        musicSource.loop = false;
        musicSource.clip = intro;
        musicSource.Play();

        if (intro != null)
            yield return new WaitForSecondsRealtime(intro.length);

        PlayMusicLoop(musicLibrary.level1Loop, forceRestart: false);
        _musicRoutine = null;
    }

    private void PlayMusicLoop(AudioClip clip, bool forceRestart)
    {
        if (clip == null)
            return;

        if (!forceRestart && musicSource.clip == clip && musicSource.loop && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetMusicDucked(bool ducked)
    {
        if (_musicDucked == ducked)
            return;
        _musicDucked = ducked;

        if (_duckRoutine != null)
        {
            StopCoroutine(_duckRoutine);
            _duckRoutine = null;
        }

        if (mainMixer == null || string.IsNullOrEmpty(musicVolumeParameter))
        {
            if (musicSource != null)
                musicSource.volume = ducked ? 0.25f : 1f;
            return;
        }

        _duckRoutine = StartCoroutine(DuckMusicRoutine(ducked));
    }

    private IEnumerator DuckMusicRoutine(bool toDucked)
    {
        float from = toDucked ? musicVolumeNormalDb : musicVolumeDuckedDb;
        float to = toDucked ? musicVolumeDuckedDb : musicVolumeNormalDb;
        float t = 0f;
        float dur = Mathf.Max(0.01f, musicDuckFadeSeconds);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float db = Mathf.Lerp(from, to, k);
            ApplyMixerMusicVolume(db);
            yield return null;
        }

        ApplyMixerMusicVolume(to);
        _duckRoutine = null;
    }

    private void ApplyMixerMusicVolume(float db)
    {
        if (mainMixer != null && !string.IsNullOrEmpty(musicVolumeParameter))
            mainMixer.SetFloat(musicVolumeParameter, db);
    }

    public void PlayMenuClick()
    {
        PlayOneShot(uiSource, globalAudioSet != null ? globalAudioSet.menuClickClip : null);
    }

    public void PlayDeadeyeStart()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.deadeyeStartClip : null);
    }

    public void PlayDeadeyeEnd()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.deadeyeEndClip : null);
    }

    public void PlayBulletClink()
    {
        PlayRandomOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.bulletClinkClips : null);
    }

    public void PlayPlayerReloadStart()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadStartClip : null);
    }

    public void PlayPlayerReloadEmptyChamber()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadEmptyChamberClip : null);
    }

    public void PlayPlayerReloadLoadBullet()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadLoadBulletClip : null);
    }

    public void PlayPlayerReloadEnd()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadEndClip : null);
    }

    public void PlayAiReloadStart()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadStartClip : null);
    }

    public void PlayAiReloadEmptyChamber()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadEmptyChamberClip : null);
    }

    public void PlayAiReloadLoadBullet()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadLoadBulletClip : null);
    }

    public void PlayAiReloadEnd()
    {
        PlayOneShot(sfxSource, globalAudioSet != null ? globalAudioSet.reloadEndClip : null);
    }

    private static void PlayOneShot(AudioSource src, AudioClip clip)
    {
        if (src == null || clip == null)
            return;
        src.pitch = 1f;
        src.PlayOneShot(clip);
    }

    private static void PlayRandomOneShot(AudioSource src, AudioClip[] clips)
    {
        if (src == null || clips == null || clips.Length == 0)
            return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
            return;
        src.pitch = Random.Range(0.95f, 1.05f);
        src.PlayOneShot(clip);
    }
}
