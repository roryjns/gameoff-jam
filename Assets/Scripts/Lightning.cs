using UnityEngine;
using System.Collections;

public class LightningFlash : MonoBehaviour
{
    [SerializeField] float minInterval, maxInterval, maxAlpha, flashDuration;
    private SpriteRenderer spriteRenderer;
    private Color color;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color;
    }

    private void Start()
    {
        StartCoroutine(LightningRoutine());
    }

    private IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            int flashes = Random.Range(1, 3);

            for (int i = 0; i < flashes; i++)
            {
                yield return Flash();
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }

            int distance = Random.Range(1, 3); // 1, 2 or 3
            yield return new WaitForSeconds(distance);
            if (distance == 1) SoundManager.PlaySound(SoundManager.SoundType.THUNDERCLOSE);
            else SoundManager.PlaySound(SoundManager.SoundType.THUNDERFAR);
        }
    }

    private IEnumerator Flash()
    {
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, maxAlpha, t / flashDuration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }

        t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(maxAlpha, 0f, t / flashDuration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }
    }
}