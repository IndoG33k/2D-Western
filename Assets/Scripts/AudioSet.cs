using UnityEngine;

[CreateAssetMenu(menuName = "2D-Western/Audio Set", fileName = "AudioSet")]
public class AudioSet : ScriptableObject
{
    [Header("Character actions")]
    public AudioClip[] shootClips;
    public AudioClip[] hitClips;
    public AudioClip[] reloadClips;
    public AudioClip[] deathClips;

    [Header("World impacts")]
    public AudioClip[] bulletClinkClips;
}

