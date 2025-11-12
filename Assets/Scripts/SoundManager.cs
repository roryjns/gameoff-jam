using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private static SoundManager Instance;
    Dictionary<SoundType, SoundEntry> soundMap;
    [SerializeField] List<SoundEntry> sounds;
    [SerializeField] AudioSource sfxSource;

    [System.Serializable]
    public struct SoundEntry
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 2f)] public float defaultVolume;
    }

    public enum SoundType
    {
        UICONFIRM,
        UIBACK,
        WALK,
        JUMP,
        DASH,
    }

    void Awake()
    {
        Instance = this;
        soundMap = new Dictionary<SoundType, SoundEntry>();

        foreach (var entry in sounds)
        {
            soundMap[entry.type] = entry;
        }
    }

    public static void PlaySound(SoundType type)
    {
        if (!Instance.soundMap.ContainsKey(type)) return;

        var sound = Instance.soundMap[type];

        Instance.sfxSource.ignoreListenerPause =
            type == SoundType.UICONFIRM || type == SoundType.UIBACK;

        float finalVolume = sound.defaultVolume;
        Instance.sfxSource.PlayOneShot(sound.clip, finalVolume);
    }

    public static void PlaySound(SoundType type, AudioSource source)
    {
        if (!Instance.soundMap.ContainsKey(type)) return;

        var sound = Instance.soundMap[type];
        float finalVolume = sound.defaultVolume;
        source.PlayOneShot(sound.clip, finalVolume);
    }
}