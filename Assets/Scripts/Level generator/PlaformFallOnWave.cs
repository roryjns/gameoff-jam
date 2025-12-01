using System.Collections;
using UnityEngine;

public class PlaformFallOnWave : MonoBehaviour
{
    Rigidbody2D rb;
    Vector3 fallPosition;
    bool hasFallen = false;
    bool isRising = false;
    float riseSpeed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (TidalWave.Instance != null)
        {
            TidalWave.Instance.OnWaveCrash += TidalWave_OnWaveCrash;
            TidalWave.Instance.OnWaveStart += TidalWave_OnWaveStart;
        }
    }

    void FixedUpdate()
    {
        if (isRising && hasFallen)
        {
            float step = riseSpeed * Time.fixedDeltaTime;
            Vector3 newPos = Vector3.MoveTowards(transform.position, fallPosition, step);
            rb.MovePosition(newPos);

            if (Vector3.Distance(transform.position, fallPosition) < 0.01f)
            {
                transform.position = fallPosition;
                isRising = false;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    private void TidalWave_OnWaveCrash(object sender, System.EventArgs e)
    {
        if (!hasFallen)
        {
            fallPosition = transform.position;
            hasFallen = true;
        }
        
        isRising = false;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        StartCoroutine(StartRisingAfterDelay());
    }

    private IEnumerator StartRisingAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
        isRising = true;
    }

    private void TidalWave_OnWaveStart(object sender, System.EventArgs e)
    {
    }

    private void OnDestroy()
    {
        if (TidalWave.Instance != null)
        {
            TidalWave.Instance.OnWaveCrash -= TidalWave_OnWaveCrash;
            TidalWave.Instance.OnWaveStart -= TidalWave_OnWaveStart;
        }
    }
}
