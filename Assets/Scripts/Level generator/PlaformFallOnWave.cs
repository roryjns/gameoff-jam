using UnityEngine;

public class PlaformFallOnWave : MonoBehaviour
{
    void Start()
    {
        if (TidalWave.Instance != null)
        {
            TidalWave.Instance.OnWaveCrash += TidalWave_OnWaveCrash;
        }
    }

    private void TidalWave_OnWaveCrash(object sender, System.EventArgs e)
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
    }

    private void OnDestroy()
    {
        if (TidalWave.Instance != null)
        {
            TidalWave.Instance.OnWaveCrash -= TidalWave_OnWaveCrash;
        }
    }
}
