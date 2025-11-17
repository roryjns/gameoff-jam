using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Material material; // The shared material reference
    [SerializeField] private float flashTime;
    private Material instanceMaterial; // Unique instance of the material

    private void Awake()
    {
        instanceMaterial = new Material(material); // Create a unique instance of the material for this GameObject
        GetComponent<Renderer>().material = instanceMaterial; // Assign the new instance to the renderer
    }

    public void CallDamageFlash()
    {
        if (gameObject.activeInHierarchy) // Only start the coroutine if the GameObject is active
        {
            StartCoroutine(DamageFlasher());
        }
    }

    private IEnumerator DamageFlasher()
    {
        instanceMaterial.SetColor("_FlashColour", Color.white);
        float currentFlashAmount, elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = Mathf.Lerp(1f, 0f, elapsedTime / flashTime);
            instanceMaterial.SetFloat("_FlashAmount", currentFlashAmount);
            yield return null;
        }
    }
}