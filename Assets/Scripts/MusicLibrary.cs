using UnityEngine;

[CreateAssetMenu(menuName = "2D-Western/Music Library", fileName = "MusicLibrary")]
public class MusicLibrary : ScriptableObject
{
    [Tooltip("Played once per run when first L1 encounter starts, then Level 1 Loop takes over.")]
    public AudioClip level1Intro;

    [Tooltip("Looped while fighting Level 1 tier (after intro).")]
    public AudioClip level1Loop;

    public AudioClip level2Loop;
    public AudioClip level3Loop;
    public AudioClip level4Loop;
}
