using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    internal int X;
    internal int Y;

    Tilemap tilemap = null;
    public Tilemap GetTilemap()
    {
        if (tilemap == null)
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }
        return tilemap;
    }
    public bool ExistTile(int tileX, int tileY)
    {
        return GetTilemap().GetTile(new Vector3Int(tileX, tileY)) != null;
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
        return ExistTile(1, 0) && !ExistTile(1, 1);
    }

    internal void SetTile(Vector3Int pos, TileBase tile)
    {
        GetTilemap().SetTile(pos, tile);
    }

    const int openingSize = 7;

    internal void OpenTopLeftRoof()
    {
        for (int i = 1; i < openingSize; i++)
        {
            SetTile(new Vector3Int(i, 15), null);
        }
    }

    internal void OpenBottomLeftFloor()
    {
        for (int i = 1; i < openingSize; i++)
        {
            SetTile(new Vector3Int(i, 0), null);
        }
    }
}
