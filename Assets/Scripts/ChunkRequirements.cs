using UnityEngine;

[CreateAssetMenu(menuName = "ChunkRequirement")]
public class ChunkRequirements : ScriptableObject, IRequirement
{
    public int Type = 0;

    public bool IsMet(GameObject gameObject)
    {
        switch (Type)
        {
            case 0:
                Debug.LogWarning($"Requirement at {gameObject.GetFullPath()}/{name} is set to 0 (Empty, does nothing)");
                return true;
            case 1: // Chunk can open top left ceiling;
                Chunk chunk = GetChunk(gameObject);
                return LevelGenerator.Instance.CanChunkOpenTopLeft(chunk);
        }
        Debug.LogWarning($"Requirement at {gameObject.GetFullPath()}/{name} is configured with a wrong number");
        return false;
    }

    public void OnSpawned(GameObject gameObject)
    {
        switch (Type)
        {
            case 1: // Open top left ceiling;
                Chunk chunk = GetChunk(gameObject);
                LevelGenerator.Instance.OpenTopLeftRoof(chunk);
                break;

        }
    }

    public Chunk GetChunk(GameObject gameObject)
    {
        Transform tr = gameObject.transform;
        while (true)
        {
            Chunk chunk = tr.GetComponentInParent<Chunk>();
            if (chunk != null)
            {
                return chunk;
            }
            tr = tr.parent;
        }
    }
}

public interface IRequirement
{
    bool IsMet(GameObject gameObject);
    void OnSpawned(GameObject gameObject);
}