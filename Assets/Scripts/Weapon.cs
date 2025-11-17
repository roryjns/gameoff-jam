using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] int damage = 20;
    BoxCollider2D hitbox;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        gameObject.SetActive(false);
    }

    public void SetHitboxSettings(PlayerController.HitboxSettings hitboxSettings)
    {
        hitbox.offset = hitboxSettings.offset;
        hitbox.size = hitboxSettings.size;
        damage = hitboxSettings.damage;
    }

    public void DetectHits()
    {
        ContactFilter2D filter = new()
        {
            useTriggers = true
        };

        Collider2D[] results = new Collider2D[10];
        int count = hitbox.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D other = results[i];
            if (!other) continue;
            if (!other.CompareTag("Enemy")) continue;
            if (other.TryGetComponent<Enemy>(out var enemy)) enemy.TakeDamage(damage);
        }
    }
}