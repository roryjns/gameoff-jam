using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<SoundEntry> sounds;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioMixer mixer;
    [SerializeField] PlayerController player;

    private static AudioManager Instance;
    Dictionary<SoundType, SoundEntry> soundMap;
    private readonly float normalCutoff = 22000f, underwaterCutoff = 1200f;

    private void Update()
    {
        float target = player.underwater ? underwaterCutoff : normalCutoff;

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
        [Range(0f, 2f)] public float defaultVolume;
    }

    public enum SoundType
    {
        UICONFIRM,
        UIBACK,
        WALK,
        JUMP,
        DASH,
        THUNDERCLOSE,
        THUNDERFAR,
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