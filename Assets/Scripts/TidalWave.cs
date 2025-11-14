using UnityEngine;

public class TidalWave : MonoBehaviour
{
    [SerializeField] float waveDuration; // Wave rise and fall time
    [SerializeField] float dormantDuration; // How long between waves
    [SerializeField] float heightIncrease; // How much the wave rises by, reduce this each level to make it look like you are ascending the wave (20, 13, 7, 3?)
    [SerializeField] float flattenAmount; // 0-1, how much to flatten the top edge by upon reaching the peak
    [SerializeField] AnimationCurve verticalSpeed; // Decrease as the wave rises, increase as it falls
    [SerializeField] Transform cameraTransform;

    WaveLayer waveLayer;
    float timer, dormantTimer, startY;
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
                startY = transform.position.y - cameraTransform.position.y;
            }
            return;
        }

        timer += Time.deltaTime;
        float normalizedTime = timer / waveDuration;

        if (timer >= waveDuration)
        {
            normalizedTime = 1f;
            timer = 0f;
            isDormant = true;
            dormantTimer = 0f;
        }

        float curvedT = verticalSpeed.Evaluate(normalizedTime);

        // Move wave vertically
        float baseY = cameraTransform.position.y;
        float offsetY = Mathf.Lerp(startY, heightIncrease, curvedT);

        transform.position = new Vector3(
            transform.position.x,
            baseY + offsetY,
            transform.position.z
        );

        // Gradually flatten the top at the start
        if (normalizedTime < 0.3f)
        {
            float riseT = normalizedTime / 0.3f;
            waveLayer.amplitudeMultiplier = Mathf.Lerp(1f, 1f - flattenAmount, riseT);
        }
        else
        {
            waveLayer.amplitudeMultiplier = 1f - flattenAmount;
        }
    }
}