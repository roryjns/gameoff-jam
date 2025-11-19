using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int health;
    DamageFlash damageFlash;

    private void Awake()
    {
        damageFlash = GetComponent<DamageFlash>();   
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        damageFlash.CallDamageFlash();
        if (health <= 0) Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}