using UnityEngine;

[CreateAssetMenu(menuName = "2D-Western/Audio Set", fileName = "AudioSet")]
public class AudioSet : ScriptableObject
{
    [Header("Character actions")]
    public AudioClip[] shootClips;
    public AudioClip[] hitClips;
    public AudioClip[] deathClips;

    [Header("Reload (player chamber order + AI sequence)")]
    public AudioClip reloadStartClip;
    public AudioClip reloadEmptyChamberClip;
    public AudioClip reloadLoadBulletClip;
    public AudioClip reloadEndClip;

    [Header("Deadeye")]
    public AudioClip deadeyeStartClip;
    public AudioClip deadeyeEndClip;

    [Header("UI")]
    public AudioClip menuClickClip;

    [Header("World impacts")]
    public AudioClip[] bulletClinkClips;
}
