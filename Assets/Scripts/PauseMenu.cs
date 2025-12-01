using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu, optionsMenu, pauseMenuFirst, optionsMenuFirst;
    [SerializeField] Slider controllerDeadzoneSlider, musicSlider, sfxSlider;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Volume volume;
    DepthOfField dof;
    bool isPaused = false;

    private void Start()
    {
        controllerDeadzoneSlider.value = PlayerPrefs.GetFloat("ControllerDeadzone", 0.1f) / 0.05f;
        sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", 1) / 0.1f;
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1) / 0.1f;
        volume.profile.TryGet(out dof);
        ApplyOptions();
    }

    public void OnTogglePause(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void OnMenuClose(InputAction.CallbackContext context)
    {
        MenuClose();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;

        if (isPaused && pauseMenuFirst != null)
        {
            dof.active = true;
            dof.focusDistance.value = 1f;
            PlayerController.Instance.playerInput.SwitchCurrentActionMap("UI");
            EventSystem.current.SetSelectedGameObject(pauseMenuFirst);
        }
        else
        {
            dof.active = false;
            dof.focusDistance.value = 10f;
            PlayerController.Instance.playerInput.SwitchCurrentActionMap("Player");
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void MenuClose()
    {
        if (pauseMenu.activeSelf) TogglePause();
        else if (optionsMenu.activeSelf)
        {
            optionsMenu.SetActive(false);
            pauseMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(pauseMenuFirst);
        }
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(optionsMenuFirst);
    }

    public void UpdateOptions()
    {
        PlayerPrefs.SetFloat("ControllerDeadzone", controllerDeadzoneSlider.value * 0.05f); // Maps 0-10 to 0 to 0.5
        PlayerPrefs.SetFloat("SfxVolume", sfxSlider.value * 0.1f);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value * 0.1f);
        ApplyOptions();
    }

    private void ApplyOptions()
    {
        PlayerController.Instance.controllerDeadzone = PlayerPrefs.GetFloat("ControllerDeadzone", 0.1f);
        audioMixer.SetFloat("SfxVolume", Mathf.Log10(Mathf.Clamp(sfxSlider.value * 0.1f, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(musicSlider.value * 0.1f, 0.0001f, 1f)) * 20);
    }
}