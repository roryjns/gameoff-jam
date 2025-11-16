using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public GameObject Controls;
    public GameObject ContinueButton;
    GameObject currentActive = null;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        TogglePaused(IsMainMenu());
    }

    void Update()
    {
        if (InputSystem.actions["Pause Menu"].WasPressedThisFrame())
        {
            if (IsMainMenu())
            {
                // No hiding pause on Main Menu
                return;
            }
            TogglePaused();
        }
    }

    public bool IsMainMenu()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu";
    }

    public void TogglePaused(bool? forceTo = null)
    {
        IsPaused = forceTo.GetValueOrDefault(!IsPaused);
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(IsPaused);
        }
        float oneIfNotPaused = IsPaused ? 0.0f : 1.0f;
        Time.timeScale = oneIfNotPaused;
        if (currentActive != null)
        {
            currentActive.SetActive(false);
        }
        ContinueButton.SetActive(IsMainMenu());
    }

    public void ResumeGame()
    {
        TogglePaused(false);
    }
    public void PauseGame()
    {
        TogglePaused(true);
    }
    public void ShowControls()
    {
        if (currentActive != null)
        {
            currentActive.SetActive(false);
        }
        Controls.SetActive(true);
        currentActive = Controls;
    }
    public void NewGame()
    {
        SceneManager.LoadScene("Prototype");
        TogglePaused(false);
    }
}
