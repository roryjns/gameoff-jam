using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<SoundEntry> sounds;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioMixer mixer;

    public static AudioManager Instance { get; private set; }
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
        LAND,
        LIGHTATTACK1,
        LIGHTATTACK2,
        LIGHTATTACK3,
        PLAYERHIT,
        WATERENTER,
        WATEREXIT,
        ORBPICKUP,
        ENEMYDEATH,
        ENEMYIDLE,
        ENEMYWINDUP,
        ENEMYATTACK,
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
    
    public SoundEntry GetSound(SoundType type)
    {
        if (soundMap.ContainsKey(type))
            return soundMap[type];
        return default;
    }

    public IEnumerator FadeOut(float duration)
    {
        // Collect all exposed volume parameters on the mixer
        var parameters = new List<string>
        {
            "MusicVolume",
            "SfxVolume"
        };

        // Cache initial values
        Dictionary<string, float> startValues = new();
        foreach (var p in parameters)
        {
            mixer.GetFloat(p, out float v);
            startValues[p] = v;
        }

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            foreach (var p in parameters)
            {
                float start = startValues[p];
                float end = -80f;
                mixer.SetFloat(p, Mathf.Lerp(start, end, t));
            }

            time += Time.unscaledDeltaTime; // Ignores pause
            yield return null;
        }

        // Force to silence at the very end
        foreach (var p in parameters)
            mixer.SetFloat(p, -80f);
    }
}