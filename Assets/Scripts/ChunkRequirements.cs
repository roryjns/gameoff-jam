using UnityEngine;

[CreateAssetMenu(menuName = "ChunkRequirement")]
public class ChunkRequirements : ScriptableObject, IRequirement
{
    public ChunkRequirementType Type = 0;

    public bool IsMet(GameObject gameObject)
    {
        switch (Type)
        {
            case ChunkRequirementType.EMPTY_SELECTONEBELOW_:
                Debug.LogWarning($"Requirement at {gameObject.GetFullPath()}/{name} is set to 0 (Empty, does nothing)");
                return true;
            case ChunkRequirementType.ChunkCanOpenTopLeftCeiling:
                {
                    Chunk chunk = GetChunk(gameObject);
                    bool can = LevelGenerator.Instance.CanChunkOpenTopLeft(chunk);
                    return can;
                }
            case ChunkRequirementType.IsNotStartOfLevel:
                {
                    Chunk chunk = GetChunk(gameObject);
                    return chunk.X != 0 || chunk.Y != 0;
                }
        }
        Debug.LogWarning($"Requirement at {gameObject.GetFullPath()}/{name} is configured with a wrong number");
        return false;
    }

    public void OnSpawned(GameObject gameObject)
    {
        switch (Type)
        {
            case ChunkRequirementType.ChunkCanOpenTopLeftCeiling: // Open top left ceiling;
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

public enum ChunkRequirementType
{
    EMPTY_SELECTONEBELOW_,
    ChunkCanOpenTopLeftCeiling,
    IsNotStartOfLevel,
}

public interface IRequirement
{
    bool IsMet(GameObject gameObject);
    void OnSpawned(GameObject gameObject);
}