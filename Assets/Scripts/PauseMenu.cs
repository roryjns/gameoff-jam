using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        TogglePaused(false);
    }

    void Update()
    {
        if (InputSystem.actions["Pause Menu"].WasPressedThisFrame())
        { 
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
            {
                // No hiding pause on Main Menu
                return; 
            }
            TogglePaused();
        }
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
    }

    public void ResumeGame()
    {
        TogglePaused(false);
    }
    public void PauseGame()
    {
        TogglePaused(true);
    }
}
