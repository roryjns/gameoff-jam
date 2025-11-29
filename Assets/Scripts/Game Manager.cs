using UnityEngine;
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
    [SerializeField] CanvasGroup fadeCanvas;
    [SerializeField] PlayerController playerController;
    [SerializeField] TextMeshProUGUI orbText;

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
        // Generate level
        StartCoroutine(FadeCanvas(0f, 0.5f)); // Fade from black
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
        yield return FadeCanvas(1f, 1f);
        ObjectPooler.Instance.ClearAllPools();
        fadeCanvas.gameObject.SetActive(false);
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void ExitToMenu()
    {
        AudioManager.PlaySound(AudioManager.SoundType.UIBACK);
        StartCoroutine(LoadScene(0));
    }

    private IEnumerator LoadScene(int sceneIndex)
    {
        yield return FadeCanvas(1f, 0.5f); // Fade to black
        SceneManager.LoadScene(sceneIndex);
    }

    public IEnumerator OnPlayerDeath()
    {
        yield return FadeCanvas(1f, 0.5f); // Fade to black
        runData = null;
    }

    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        fadeCanvas.gameObject.SetActive(true);
        float startAlpha = fadeCanvas.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
    }

    internal void EnemyDied(Enemy enemy)
    {
        OnEnemyDeath?.Invoke(this, enemy);
    }
}