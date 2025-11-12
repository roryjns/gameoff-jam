using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance = null;
    private List<GameObject> loadedChunks;
    public RuleTile RuleTile;
    public int NumChunksWide = 5;
    public int NumChunksHigh = 3;
    public int ChunkWidth = 16;
    public int ChunkHeight = 16;
    public Chunk[][] InstantiatedChunks = new Chunk[][] { };
    internal Tilemap Tilemap = null;
    public LevelPattern pattern = LevelPattern.Random;

    void Awake()
    {
        Instance = this;
        loadedChunks = new List<GameObject>(Resources.LoadAll<GameObject>("Chunks"));
    }

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        InstantiatedChunks = new Chunk[NumChunksHigh][];
        if (Tilemap != null)
        {
            Tilemap.ClearAllTiles();
        }
        else
        {
            Tilemap = GetComponent<Tilemap>();
        }
        TileBase[] tiles = new TileBase[ChunkWidth * ChunkHeight];
        Vector3Int[] positions = new Vector3Int[ChunkWidth * ChunkHeight];

        for (int y = 0; y < NumChunksHigh; y++)
        {
            for (int x = 0; x < NumChunksWide; x++)
            {
                ChunkType? chunkType = null;
                switch (pattern)
                {
                    case LevelPattern.Random:
                        break;
                    case LevelPattern.EnemiesEveryOtherColumn:
                        chunkType = (x % 2 == 0) ? ChunkType.Traversal : ChunkType.Fight;
                        break;
                    case LevelPattern.EnemiesEveryOtherRow:
                        chunkType = (y % 2 == 0) ? ChunkType.Traversal : ChunkType.Fight;
                        break;
                    case LevelPattern.CheckeredEnemies:
                        chunkType = ((x + y) % 2 == 0) ? ChunkType.Traversal : ChunkType.Fight;
                        break;
                    default:
                        break;
                }
                var tilemap = SpawnChunk(new Vector2Int(x, y), new Vector2Int(16, 16), tiles, positions, chunkType);
            }
        }
        for (int yChunk = 0; yChunk < NumChunksHigh; yChunk++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    SetTile(0, yChunk, -1 - x, y, RuleTile);
                    SetTile(NumChunksWide, yChunk, x, y, RuleTile);
                }
            }
        }
        for (int xChunk = -1; xChunk <= NumChunksWide; xChunk++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    SetTile(xChunk, NumChunksHigh, x, y, RuleTile);
                }
            }
        }
        //for (int x = 0; x < NumChunksWide; x++)
        //{
        //    for (int i = 0; i < ChunkHeight; i++)
        //    {
        //        SetTile(x, NumChunksHigh, i, 0, RuleTile);
        //    }
        //}
        for (int y = 0; y < NumChunksHigh; y++)
        {
            for (int x = 0; x < NumChunksWide; x++)
            {
                if (CanOpenBottomRight(x, y) && CanOpenBottomLeft(x + 1, y))
                {
                    OpenBottomRight(x, y);
                    OpenBottomLeft(x + 1, y);
                }
            }
        }
    }

    public void SetTile(int chunkX, int chunkY, int tileX, int tileY, TileBase tile)
    {
        Tilemap.SetTile(new Vector3Int(chunkX * ChunkWidth + tileX, -chunkY * ChunkHeight + tileY), tile);
    }

    const int openingSize = 7;
    private void OpenBottomLeft(int x, int y)
    {
        for (int i = 1; i < openingSize; i++)
        {
            SetTile(x, y, 0, i, null);
        }
    }

    private void OpenBottomRight(int x, int y)
    {
        for (int i = 1; i < openingSize; i++)
        {
            SetTile(x, y, 15, i, null);
        }
    }

    internal bool ExistsTile(int chunkX, int chunkY, int tileX, int tileY)
    {
        return Tilemap.GetTile(new Vector3Int(chunkX * ChunkWidth + tileX, -chunkY * ChunkHeight + tileY)) != null;
    }

    private bool CanOpenBottomLeft(int x, int y)
    {
        return ExistsTile(x, y, 0, 1) && !ExistsTile(x, y, 1, 1);
    }

    private bool CanOpenBottomRight(int x, int y)
    {
        return ExistsTile(x, y, 15, 1) && !ExistsTile(x, y, 14, 1);
    }

    private GameObject SpawnChunk(Vector2Int gridPos, Vector2Int gridChunkSize, TileBase[] tiles, Vector3Int[] positions, ChunkType? chunkType)
    {
        if (loadedChunks.Count == 0) return null;
        int index = Random.Range(0, loadedChunks.Count);
        var toInstantiate = loadedChunks[index];
        if (toInstantiate == null) return null;

        Vector3 pos = new Vector3(gridPos.x * gridChunkSize.x, -gridPos.y * gridChunkSize.y, 0);
        GameObject obj = Instantiate(toInstantiate, pos, Quaternion.identity, transform);
        Chunk chunk = obj.GetComponent<Chunk>();

        if (chunk == null)
        {
            chunk = obj.AddComponent<Chunk>();
        } 
        else if (chunkType.HasValue && chunk.Type != chunkType.Value)
        {
            Destroy(obj);
            return SpawnChunk(gridPos, gridChunkSize, tiles, positions, chunkType);
        }
            chunk.X = gridPos.x;
        chunk.Y = gridPos.y;
        if (InstantiatedChunks[gridPos.y] == null)
        {
            InstantiatedChunks[gridPos.y] = new Chunk[NumChunksWide];
        }
        InstantiatedChunks[gridPos.y][gridPos.x] = chunk;
        obj.name = gridPos.ToString();

        var oldTilemap = obj.GetComponentInChildren<Tilemap>();
        int count = oldTilemap.GetTilesRangeNonAlloc(new Vector3Int(-424242, -424242), new Vector3Int(42424242, 4242424), positions, tiles);

        Vector3Int offset = new Vector3Int(gridPos.x * ChunkWidth, -gridPos.y * ChunkHeight, 0);

        for (int i = 0; i < count; i++)
        {
            Tilemap.SetTile(positions[i] + offset, tiles[i]);
        }

        Destroy(oldTilemap.gameObject);
        return obj;
    }

    internal bool CanChunkOpenTopLeft(Chunk chunk)
    {
        Chunk other = GetChunk(chunk.X, chunk.Y - 1);
        if (other == null) return false;
        return chunk.CanOpenTopLeftRoof() && other.CanOpenBottomLeftFloor();
    }

    public Chunk GetChunk(int x, int y)
    {
        if (x >= NumChunksWide || y >= NumChunksHigh || y < 0 || x < 0)
        {
            return null;
        }
        return InstantiatedChunks[y][x];
    }

    internal void OpenTopLeftRoof(Chunk chunk)
    {
        chunk.OpenTopLeftRoof();
        GetChunk(chunk.X, chunk.Y - 1).OpenBottomLeftFloor();
    }

    internal Chunk GetChunkFromPosition(Vector3 position)
    {
        Vector3 cellPos = transform.position - new Vector3(position.x, position.y);
        int x = (int)-Mathf.Floor(cellPos.x) / 16;
        int y = (int)Mathf.Floor(cellPos.y + 16) / 16;
        return GetChunk(x, y);
    }
}
public enum LevelPattern
{
    Random,
    EnemiesEveryOtherColumn,
    EnemiesEveryOtherRow,
    CheckeredEnemies,
}