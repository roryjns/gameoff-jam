using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<SoundEntry> sounds;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioMixer mixer;

    private static AudioManager Instance;
    Dictionary<SoundType, SoundEntry> soundMap;
    Dictionary<SoundType, int> lastClipIndex;
    private readonly float normalCutoff = 22000f, underwaterCutoff = 1200f;

    private void Update()
    {
        float target = PlayerController.Instance.underwater ? underwaterCutoff : normalCutoff;

        mixer.SetFloat("MusicLPF", Mathf.Lerp(
            GetCurrentLPF("MusicLPF"), target, Time.deltaTime * 5f));

        mixer.SetFloat("SfxLPF", Mathf.Lerp(
            GetCurrentLPF("SfxLPF"), target, Time.deltaTime * 5f));
    }

    private float GetCurrentLPF(string param)
    {
        mixer.GetFloat(param, out float value);
        return value;
    }

    [System.Serializable]
    public struct SoundEntry
    {
        public SoundType type;
        public AudioClip clip;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float defaultVolume;
    }

    public enum SoundType
    {
        UICONFIRM,
        UIBACK,
        WALK,
        JUMP,
        DASH,
        LIGHTATTACK1,
        LIGHTATTACK2,
        LIGHTATTACK3,
        WATERENTER,
        WATEREXIT,
        ORBPICKUP,
        THUNDERCLOSE,
        THUNDERFAR,
    }

    void Awake()
    {
        Instance = this;
        soundMap = new Dictionary<SoundType, SoundEntry>();
        lastClipIndex = new Dictionary<SoundType, int>();
        foreach (var entry in sounds)
        {
            soundMap[entry.type] = entry;
            lastClipIndex[entry.type] = -1;
        }
    }

    public static void PlaySound(SoundType type)
    {
        if (!Instance.soundMap.ContainsKey(type)) return;

        var sound = Instance.soundMap[type];

        Instance.sfxSource.ignoreListenerPause =
            type == SoundType.UICONFIRM || type == SoundType.UIBACK;

        float finalVolume = sound.defaultVolume;
        
        // Use clips array if available, otherwise fall back to single clip
        AudioClip clipToPlay = sound.clip;
        if (sound.clips != null && sound.clips.Length > 0)
        {
            int clipIndex;
            if (sound.clips.Length > 1)
            {
                // Pick a different clip than last time
                do
                {
                    clipIndex = Random.Range(0, sound.clips.Length);
                } while (clipIndex == Instance.lastClipIndex[type]);
                Instance.lastClipIndex[type] = clipIndex;
            }
            else
            {
                clipIndex = 0;
            }
            clipToPlay = sound.clips[clipIndex];
        }
        
        if (clipToPlay == null) return;
        Instance.sfxSource.PlayOneShot(clipToPlay, finalVolume);
    }

    public static void PlaySound(SoundType type, AudioSource source)
    {
        if (!Instance.soundMap.ContainsKey(type)) return;

        var sound = Instance.soundMap[type];
        float finalVolume = sound.defaultVolume;
        
        // Use clips array if available, otherwise fall back to single clip
        AudioClip clipToPlay = sound.clip;
        if (sound.clips != null && sound.clips.Length > 0)
        {
            int clipIndex;
            if (sound.clips.Length > 1)
            {
                // Pick a different clip than last time
                do
                {
                    clipIndex = Random.Range(0, sound.clips.Length);
                } while (clipIndex == Instance.lastClipIndex[type]);
                Instance.lastClipIndex[type] = clipIndex;
            }
            else
            {
                clipIndex = 0;
            }
            clipToPlay = sound.clips[clipIndex];
        }
        
        if (clipToPlay == null) return;
        source.PlayOneShot(clipToPlay, finalVolume);
    }
}