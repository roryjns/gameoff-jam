using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] Slider healthSlider, easeHealthSlider;
    
    public void Initialise(int currentHealth)
    {
        healthSlider.value = healthSlider.maxValue = easeHealthSlider.value = easeHealthSlider.maxValue = currentHealth;
    }

    public void UpdateSlider(int currentHealth)
    {
        healthSlider.value = currentHealth;
    }

    private void Update()
    {
        if (healthSlider.value != easeHealthSlider.value)
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, healthSlider.value, 3f * Time.deltaTime);
    }
}