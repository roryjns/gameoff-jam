using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int health, maxHealth, baseOrbsDropped;
    [HideInInspector] public bool underwater;
    Flash flash;

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
        var orbObject = ObjectPooler.Instance.GetFromPool("Orbs", transform.position + Vector3.up, Quaternion.identity);
        Orbs orbs = orbObject.GetComponent<Orbs>();

        if (underwater) orbs.SetOrbCount(baseOrbsDropped * 2);
        else orbs.SetOrbCount(baseOrbsDropped);
        
        gameObject.SetActive(false);
    }
}