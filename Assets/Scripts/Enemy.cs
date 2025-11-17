using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int health;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}