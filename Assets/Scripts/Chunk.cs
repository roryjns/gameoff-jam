using System;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    internal int X;
    internal int Y;
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
        return ExistTile(15, 1) && !ExistTile(14, 1);
    }

    internal bool CanOpenTopLeftRoof()
    {
        return ExistTile(1, 15) && !ExistTile(1, 14);
    }

    internal bool CanOpenBottomLeftFloor()
    {
        return ExistTile(1, 0) && !ExistTile(1, 1) && (X != 0 || Y != 0);
    }

    const int openingSize = 7;

    internal void OpenTopLeftRoof()
    {
        for (int i = 1; i < openingSize; i++)
        {
            LevelGenerator.Instance.SetTile(X, Y, i, 15, null);
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

    public void OnCrash()
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
