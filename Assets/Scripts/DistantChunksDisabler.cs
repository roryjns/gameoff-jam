using UnityEngine;

public class DistantChunksDisabler : MonoBehaviour
{
    Chunk currentChunk = null;
    void Update()
    {

        LevelGenerator lg = LevelGenerator.Instance;
        if (currentChunk == null)
        {
            foreach (var row in lg.InstantiatedChunks)
            {
                foreach (var chunk in row)
                {
                    chunk.gameObject.SetActive(false);
                }
            }
        }
        Chunk middleChunk = LevelGenerator.Instance.GetChunkFromPosition(transform.position);
        if (middleChunk == null || middleChunk == currentChunk)
        {
            return;
        }
        currentChunk = middleChunk;
        void ToggleChunk(int x, int y, bool active)
        {
            Chunk chunk = lg.GetChunk(currentChunk.X + x, currentChunk.Y + y);
            if (chunk != null)
            {
                chunk.gameObject.SetActive(active);
            }
        }
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                ToggleChunk(x, y, true);
            }
        }
        for (int x = -2; x <= 2; x++)
        {
            ToggleChunk(x, 2, false);
            ToggleChunk(x, -2, false);
            ToggleChunk(2, x, false);
            ToggleChunk(-2, x, false);
        }
    }
}
