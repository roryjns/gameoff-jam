using System;
using UnityEngine;

public class TidalWave : MonoBehaviour
{
    [SerializeField] float waveDuration; // Wave rise and fall time
    [SerializeField] float dormantDuration; // How long between waves
    [SerializeField] float heightIncrease; // How much the wave rises by, reduce this each level to make it look like you are ascending the wave (20, 13, 7, 3?)
    [SerializeField] float flattenAmount; // 0-1, how much to flatten the top edge by upon reaching the peak
    [SerializeField] AnimationCurve verticalSpeed; // Decrease as the wave rises, increase as it falls
    [SerializeField] Transform cameraTransform, foregroundWave;
    [SerializeField] PlayerController player;

    Wave wave;
    float timer, dormantTimer, startY, offsetY, lastOffsetY;
    bool isDormant = true;

    [SerializeField] Transform interactableWater;  // Your water object that can rise
    [SerializeField] float interactableRiseHeight;
    float interactableStartHeight, riseDuration, holdDuration, fallDuration;

    internal event EventHandler OnWaveCrash;
    internal event EventHandler OnWaveStart;
    internal static TidalWave Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        wave = GetComponent<Wave>();
        startY = transform.position.y;
        if (interactableWater != null) interactableStartHeight = interactableWater.position.y;
        riseDuration = dormantDuration * 0.1f;
        holdDuration = dormantDuration * 0.6f;
        fallDuration = dormantDuration * 0.3f;
    }

    private void FixedUpdate()
    {
        if (isDormant)
        {
            dormantTimer += Time.deltaTime;
            wave.amplitudeMultiplier = Mathf.Lerp(wave.amplitudeMultiplier, 1f, Time.deltaTime * 0.3f);

            if (interactableWater != null)
            {
                if (dormantTimer <= riseDuration)
                {
                    float t = dormantTimer / riseDuration;

                    interactableWater.position = new Vector3(
                        interactableWater.position.x,
                        Mathf.Lerp(interactableStartHeight, interactableRiseHeight, t),
                        interactableWater.position.z
                    );
                }

                else if (dormantTimer <= riseDuration + holdDuration)
                {
                    interactableWater.position = new Vector3(
                        interactableWater.position.x,
                        interactableRiseHeight,
                        interactableWater.position.z
                    );
                }

                else
                {
                    float fallTime = dormantTimer - (riseDuration + holdDuration);
                    float t = fallTime / fallDuration;

                    interactableWater.position = new Vector3(
                        interactableWater.position.x,
                        Mathf.Lerp(interactableRiseHeight, interactableStartHeight, t),
                        interactableWater.position.z
                    );
                }
            }

            // Start next wave
            if (dormantTimer >= dormantDuration)
            {
                dormantTimer = 0f;
                isDormant = false;
                lastOffsetY = startY = transform.position.y - cameraTransform.position.y;
                if (interactableWater != null)
                {
                    interactableWater.gameObject.SetActive(false);
                    player.underwater = false;
                }
                OnWaveStart?.Invoke(this, null);
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
        if (normalizedTime < 0.3f) wave.amplitudeMultiplier = Mathf.Lerp(1f, 1f - flattenAmount, normalizedTime / 0.3f);
        else wave.amplitudeMultiplier = 1f - flattenAmount;

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
            if (interactableWater != null) interactableWater.gameObject.SetActive(true);
            dormantTimer = 0f;
            OnWaveCrash?.Invoke(this, null);

            if (foregroundWave.gameObject.activeSelf)
            {
                foregroundWave.gameObject.SetActive(false);
                foregroundWave.position = new Vector3(foregroundWave.position.x, 25f, foregroundWave.position.z);
            }
        }
    }
}