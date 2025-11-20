using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance = null;
    private List<GameObject> randomChunks;
    private List<GameObject> fixedChunks;
    public Vector2Int StartingChunk = new Vector2Int(0, 1);
    public RuleTile RuleTile;
    public int NumChunksWide = 5;
    public int NumChunksHigh = 3;
    public int ChunkWidth = 16;
    public int ChunkHeight = 8;
    public Dictionary<Vector2Int, Chunk> InstantiatedChunks = new();
    internal Tilemap Tilemap = null;
    private GameObject ExitChunk;
    private GameObject BunkerChunk;
    private GameObject BossArenaChunk;
    public LevelPattern pattern = LevelPattern.Random;

    void Awake()
    {
        Instance = this;
        randomChunks = new List<GameObject>(Resources.LoadAll<GameObject>("RandomChunks"));
        fixedChunks = new List<GameObject>(Resources.LoadAll<GameObject>("FixedChunks"));
        BossArenaChunk = fixedChunks.Find(x => x.name == "BossArenaChunk");
        BunkerChunk = fixedChunks.Find(x => x.name == "BunkerChunk");
        ExitChunk = fixedChunks.Find(x => x.name == "ExitChunk");
    }

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        InstantiatedChunks = new Dictionary<Vector2Int, Chunk>();
        int maxLevels = 3;
        if (Tilemap != null)
        {
            Tilemap.ClearAllTiles();
        }
        else
        {
            Tilemap = GetComponent<Tilemap>();
        }
        for (int levelCount = 1; levelCount <= maxLevels; levelCount++)
        {
            GenerateLevel(levelCount);
        }
        Vector2Int gridPos = new Vector2Int(0, 1);
        int finalLevel = maxLevels + 1;
        Vector2Int worldGridPos = GetChunkPos(gridPos.x, gridPos.y, finalLevel);
        Vector2Int gridChunkSize = new Vector2Int(ChunkWidth, ChunkHeight);
        Vector3 pos = new Vector3(worldGridPos.x * gridChunkSize.x, -worldGridPos.y * gridChunkSize.y, 0);
        GameObject obj = Instantiate(BossArenaChunk, pos, Quaternion.identity, transform);
        Chunk chunk = obj.GetComponent<Chunk>();
        TileBase[] tiles = new TileBase[ChunkWidth * ChunkHeight];
        Vector3Int[] positions = new Vector3Int[ChunkWidth * ChunkHeight];

        CopyChunkToTilemap(gridPos, finalLevel, tiles, positions, obj, chunk);
    }
    void GenerateLevel(int level)
    {
        TileBase[] tiles = new TileBase[ChunkWidth * ChunkHeight];
        Vector3Int[] positions = new Vector3Int[ChunkWidth * ChunkHeight];

        Vector2Int gridChunkSize = new Vector2Int(ChunkWidth, ChunkHeight);
        for (int y = 0; y < NumChunksHigh; y++)
        {
            for (int x = 0; x < NumChunksWide; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                if (ShouldSpawnSpecialChunk(gridPos, gridChunkSize, level, tiles, positions))
                {
                    continue;
                }
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
                GameObject tilemap = SpawnChunk(gridPos, gridChunkSize, level, tiles, positions, chunkType);
            }
        }
        Vector2Int offset = GetChunkPos(0, 0, level);
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

    public Vector2Int GetChunkPos(int x, int y, int level)
    {
        return new Vector2Int((NumChunksWide - 1) * (level - 1) + x, (NumChunksHigh + 1) * -(level - 1)+ y);
    }

    private bool ShouldSpawnSpecialChunk(Vector2Int gridPos, Vector2Int gridChunkSize, int level, TileBase[] tiles, Vector3Int[] positions)
    {
        if (gridPos.x == NumChunksWide - 1 && gridPos.y == 0)
        {
            Vector2Int worldGridPos = GetChunkPos(gridPos.x, gridPos.y, level);
            Vector3 pos = new Vector3(worldGridPos.x * gridChunkSize.x, -worldGridPos.y * gridChunkSize.y, 0);
            GameObject obj = Instantiate(ExitChunk, pos, Quaternion.identity, transform);
            Chunk chunk = obj.GetComponent<Chunk>();
            CopyChunkToTilemap(gridPos, level, tiles, positions, obj, chunk);
            return true;
        }
        return false;
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
            SetTile(x, y, ChunkWidth - 1, i, null);
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
        return ExistsTile(x, y, ChunkWidth - 1, 1) && !ExistsTile(x, y, ChunkWidth - 2, 1);
    }

    private GameObject SpawnChunk(Vector2Int gridPos, Vector2Int gridChunkSize, int level, TileBase[] tiles, Vector3Int[] positions, ChunkType? chunkType)
    {
        if (randomChunks.Count == 0) return null;
        int index = Random.Range(0, randomChunks.Count);
        var toInstantiate = randomChunks[index];
        if (toInstantiate == null) return null;

        Vector2Int worldGridPos = GetChunkPos(gridPos.x, gridPos.y, level);
        Vector3 pos = new Vector3(worldGridPos.x * gridChunkSize.x, -worldGridPos.y * gridChunkSize.y, 0);
        GameObject obj = Instantiate(toInstantiate, pos, Quaternion.identity, transform);
        Chunk chunk = obj.GetComponent<Chunk>();

        if (chunk == null)
        {
            chunk = obj.AddComponent<Chunk>();
        }
        else if (chunkType.HasValue && chunk.Type != chunkType.Value)
        {
            Destroy(obj);
            return SpawnChunk(gridPos, gridChunkSize, level, tiles, positions, chunkType);
        }
        return CopyChunkToTilemap(gridPos, level, tiles, positions, obj, chunk);
    }

    private GameObject CopyChunkToTilemap(Vector2Int gridPos, int level, TileBase[] tiles, Vector3Int[] positions, GameObject obj, Chunk chunk)
    {
        chunk.X = gridPos.x;
        chunk.Y = gridPos.y;
        chunk.Level = level;
        Vector2Int worldPos = GetChunkPos(gridPos.x, gridPos.y, level);
        InstantiatedChunks.Add(worldPos, chunk);
        obj.name = worldPos.ToString();

        var oldTilemap = obj.GetComponentInChildren<Tilemap>();
        int count = oldTilemap.GetTilesRangeNonAlloc(new Vector3Int(-424242, -424242), new Vector3Int(42424242, 4242424), positions, tiles);

        Vector3Int offset = new Vector3Int(worldPos.x * ChunkWidth, -worldPos.y * ChunkHeight, 0);

        for (int i = 0; i < count; i++)
        {
            Tilemap.SetTile(positions[i] + offset, tiles[i]);
        }

        Destroy(oldTilemap.gameObject);
        return obj;
    }

    internal bool CanChunkOpenTopLeft(Chunk chunk)
    {
        Chunk other = GetChunk(chunk.X, chunk.Y - 1, chunk.Level);
        if (other == null) return false;
        return chunk.CanOpenTopLeftRoof() && other.CanOpenBottomLeftFloor();
    }

    public Chunk GetChunk(int x, int y, int level)
    {
        if (InstantiatedChunks.TryGetValue(GetChunkPos(x,y,level), out var chunk))
        {
            return chunk;
        }
        return null;
    }

    internal void OpenTopLeftRoof(Chunk chunk)
    {
        chunk.OpenTopLeftRoof();
        GetChunk(chunk.X, chunk.Y - 1, chunk.Level).OpenBottomLeftFloor();
    }

    internal Chunk GetChunkFromPosition(Vector3 position)
    {
        Vector3 cellPos = transform.position - new Vector3(position.x, position.y);
        int x = (int)-Mathf.Floor(cellPos.x) / ChunkWidth;
        int y = (int)Mathf.Floor(cellPos.y + ChunkHeight) / ChunkHeight;
        return GetChunk(x, y, 0);
    }
}
public enum LevelPattern
{
    Random,
    EnemiesEveryOtherColumn,
    EnemiesEveryOtherRow,
    CheckeredEnemies,
}