using System;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    internal int X;
    internal int Y;
    internal int Level;
    internal List<Spawner> spawnersToSpawnAfterCrash = new ();
    public ChunkType Type;

    public bool ExistTile(int tileX, int tileY)
    {
        return LevelGenerator.Instance.ExistsTile(X, Y, tileX, tileY);
    }

    public bool CanOpenBottomLeft(int x, int y)
    {
        return ExistTile(0, 1) && !ExistTile(1, 1);
    }

    public bool CanOpenBottomRight(int x, int y)
    {
        return ExistTile(LevelGenerator.Instance.ChunkWidth - 1, 1) && !ExistTile(LevelGenerator.Instance.ChunkWidth - 2, 1);
    }

    internal bool CanOpenTopLeftRoof()
    {
        return ExistTile(1, LevelGenerator.Instance.ChunkHeight - 1) && !ExistTile(1, LevelGenerator.Instance.ChunkHeight - 2);
    }

    internal bool CanOpenBottomLeftFloor()
    {
        return ExistTile(1, 0) && !ExistTile(1, 1) &&  (X != LevelGenerator.Instance.StartingChunk.x || Y != LevelGenerator.Instance.StartingChunk.y);
    }

    const int openingSize = 7;

    internal void OpenTopLeftRoof()
    {
        for (int i = 1; i < openingSize; i++)
        {
            LevelGenerator.Instance.SetTile(X, Y, i, LevelGenerator.Instance.ChunkHeight - 1, null);
        }
    }

    internal void OpenBottomLeftFloor()
    {
        for (int i = 1; i < openingSize; i++)
        {
            LevelGenerator.Instance.SetTile(X, Y, i, 0, null);
        }
    }
    public static Chunk GetChunkFromGameObject(GameObject gameObject)
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

    public void OnWaveCrash()
    {
        foreach (Spawner spawner in spawnersToSpawnAfterCrash)
        {
            spawner.TrySpawn();
        }
    }
}

public enum ChunkType
{
    Traversal,
    Fight,
}
