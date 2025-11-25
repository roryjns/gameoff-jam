using UnityEngine;
using System.Collections;

public class Flash : MonoBehaviour
{
    [SerializeField] Material damageMat, healMat; // The shared material references
    Material damageFlashMat, healFlashMat; // Unique instances of the materials

    private void Awake()
    {
        // Create unique instances of each material for this enemy
        damageFlashMat = new Material(damageMat); 
        healFlashMat = new Material(healMat);
    }

    public void DamageFlash()
    {
        gameObject.GetComponent<Renderer>().material = damageFlashMat;
        StartCoroutine(Flasher(0.3f));
    }

    public void HealFlash()
    {
        gameObject.GetComponent<Renderer>().material = healFlashMat;
        StartCoroutine(Flasher(1.5f));
    }

    private IEnumerator Flasher(float flashTime)
    {
        float currentFlashAmount, elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = Mathf.Lerp(1f, 0f, elapsedTime / flashTime);
            gameObject.GetComponent<Renderer>().material.SetFloat("_FlashAmount", currentFlashAmount);
            yield return null;
        }
    }
}