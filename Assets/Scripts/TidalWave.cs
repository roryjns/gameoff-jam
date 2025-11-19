using UnityEngine;

public class TidalWave : MonoBehaviour
{
    [SerializeField] float waveDuration; // Wave rise and fall time
    [SerializeField] float dormantDuration; // How long between waves
    [SerializeField] float heightIncrease; // How much the wave rises by, reduce this each level to make it look like you are ascending the wave (20, 13, 7, 3?)
    [SerializeField] float flattenAmount; // 0-1, how much to flatten the top edge by upon reaching the peak
    [SerializeField] AnimationCurve verticalSpeed; // Decrease as the wave rises, increase as it falls
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform foregroundWave;

    WaveLayer waveLayer;
    float timer, dormantTimer, startY, offsetY, lastOffsetY;
    bool isDormant = true;

    void Start()
    {
        waveLayer = GetComponent<WaveLayer>();
        startY = transform.position.y;
    }

    void Update()
    {
        if (isDormant)
        {
            dormantTimer += Time.deltaTime;

            // Gradually restore original amplitude
            waveLayer.amplitudeMultiplier = Mathf.Lerp(waveLayer.amplitudeMultiplier, 1f, Time.deltaTime * 0.3f);

            if (dormantTimer >= dormantDuration)
            {
                dormantTimer = 0f;
                isDormant = false;
                lastOffsetY = startY = transform.position.y - cameraTransform.position.y;
            }
            return;
        }

        timer += Time.deltaTime;
        float normalizedTime = timer / waveDuration;
        float curvedT = verticalSpeed.Evaluate(normalizedTime);

        // Move wave vertically
        float baseY = cameraTransform.position.y;
        offsetY = Mathf.Lerp(startY, heightIncrease, curvedT);
        transform.position = new Vector3(transform.position.x, baseY + offsetY, transform.position.z);

        // Gradually flatten the top at the start
        if (normalizedTime < 0.3f) waveLayer.amplitudeMultiplier = Mathf.Lerp(1f, 1f - flattenAmount, normalizedTime / 0.3f);
        else waveLayer.amplitudeMultiplier = 1f - flattenAmount;

        // Move foreground wave down the screen when background wave starts falling
        if (offsetY < lastOffsetY)
        {
            if (!foregroundWave.gameObject.activeSelf) foregroundWave.gameObject.SetActive(true);
            else 
            {
                float deltaY = lastOffsetY - offsetY;
                foregroundWave.position += Vector3.down * deltaY;
            }
        }

        lastOffsetY = offsetY;
        
        // Wave has finishing falling and is now dormant
        if (timer >= waveDuration)
        {
            timer = 0f;
            isDormant = true;
            dormantTimer = 0f;
            if (foregroundWave.gameObject.activeSelf)
            {
                foregroundWave.gameObject.SetActive(false);
                foregroundWave.position = new Vector3(foregroundWave.position.x, 25f, foregroundWave.position.z);
            }
        }
    }
}