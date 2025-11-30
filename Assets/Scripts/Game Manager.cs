using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler<Enemy> OnEnemyDeath;

    [System.Serializable]
    public class RunData // Data that is lost upon closing the game or dying
    {
        public int currentHealth;
        public int orbCount;
    }

    public RunData runData = new();
    [SerializeField] CanvasGroup blankCanvas, deathCanvas;
    [SerializeField] PlayerController playerController;
    [SerializeField] TextMeshProUGUI orbText, deathText;
    [SerializeField] GameObject gameOverFirst;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(FadeCanvas(blankCanvas, 0f, 0.5f)); // Fade from black
        StartCoroutine(AudioManager.Instance.FadeTo("MusicVolume", 0f, 2f));
    }

    public void OrbCollected()
    {
        runData.orbCount++;
        orbText.text = runData.orbCount.ToString();
    }

    public void EnterNextLevel() // Called when entering door at end of the level
    {
        StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        yield return FadeCanvas(blankCanvas, 1f, 1f);
        ObjectPooler.Instance.ClearAllPools();
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void RestartRun()
    {
        StartCoroutine(LoadScene(1));
    }

    public void ExitToMenu()
    {
        StartCoroutine(LoadScene(0));
    }

    private IEnumerator LoadScene(int sceneIndex)
    {
        yield return FadeCanvas(blankCanvas, 1f, 1f);
        SceneManager.LoadScene(sceneIndex);
    }

    public IEnumerator OnPlayerDeath()
    {
        deathText.gameObject.SetActive(true);
        PlayerController.Instance.playerInput.SwitchCurrentActionMap("UI");
        EventSystem.current.SetSelectedGameObject(gameOverFirst);
        StartCoroutine(AudioManager.Instance.FadeTo("MusicVolume", -80f, 2f));
        StartCoroutine(AudioManager.Instance.FadeTo("SfxVolume", -80f, 1f));
        yield return FadeCanvas(deathCanvas, 1f, 2f);
        ObjectPooler.Instance.ClearAllPools();
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas, float targetAlpha, float duration)
    {
        canvas.gameObject.SetActive(true);
        float startAlpha = canvas.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        canvas.alpha = targetAlpha;
    }

    internal void EnemyDied(Enemy enemy)
    {
        OnEnemyDeath?.Invoke(this, enemy);
    }
}