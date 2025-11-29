using UnityEngine;

public class LevelTeleport : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.position += Vector3.up * LevelGenerator.Instance.ChunkHeight * 3; 
        }
    }
}
