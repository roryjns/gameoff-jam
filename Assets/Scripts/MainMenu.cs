using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] CanvasGroup blankCanvas;
    [SerializeField] Camera cam;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource menuMusic;
    [SerializeField] Slider controllerDeadzoneSlider, musicSlider, sfxSlider;
    [SerializeField] float cameraSpeed;

    private void Start()
    {
        StartCoroutine(FadeCanvas(0, 1f));
        Cursor.visible = true;
        controllerDeadzoneSlider.value = PlayerPrefs.GetFloat("ControllerDeadzone", 0.1f) / 0.05f;
        sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", 1) / 0.1f;
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1) / 0.1f;
        audioMixer.SetFloat("MusicVolume", -40f);
        StartCoroutine(AudioManager.Instance.FadeTo("MusicVolume", 0f, 2f));
        StartCoroutine(DelayedMusicStart());
        ApplyOptions();
    }

    private IEnumerator DelayedMusicStart()
    {
        yield return new WaitForSeconds(0.02f);
        menuMusic.Play();
    }

    private void Update()
    {
        cam.transform.position += cameraSpeed * Time.deltaTime * Vector3.right;
    }

    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        blankCanvas.gameObject.SetActive(true);
        float startAlpha = blankCanvas.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            blankCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        blankCanvas.alpha = targetAlpha;
    }

    public void Play()
    {
        StartCoroutine(FadeAndLoad(1f));
    }

    private IEnumerator FadeAndLoad(float duration)
    {
        StartCoroutine(FadeCanvas(1f, duration));
        float startVolume = menuMusic.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            menuMusic.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        menuMusic.Stop();
        menuMusic.volume = startVolume;
        SceneManager.LoadScene(1);
    }

    public void UpdateOptions()
    {
        PlayerPrefs.SetFloat("ControllerDeadzone", controllerDeadzoneSlider.value * 0.05f);
        PlayerPrefs.SetFloat("SfxVolume", sfxSlider.value * 0.1f);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value * 0.1f);
        ApplyOptions();
    }

    private void ApplyOptions()
    {
        audioMixer.SetFloat("SfxVolume", Mathf.Log10(Mathf.Clamp(sfxSlider.value * 0.1f, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(musicSlider.value * 0.1f, 0.0001f, 1f)) * 20);
    }

    public void Quit()
    {
        Application.Quit();
    }
}