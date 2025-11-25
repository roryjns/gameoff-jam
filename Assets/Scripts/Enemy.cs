using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int health, maxHealth;
    Flash flash;
    bool underwater;

    private void Awake()
    {
        flash = GetComponent<Flash>();   
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        flash.DamageFlash();
        if (health <= 0) Die();
    }

    public void Heal()
    {
        health = maxHealth;
        flash.HealFlash();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}