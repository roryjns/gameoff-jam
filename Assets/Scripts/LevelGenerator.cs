using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance = null;
    private List<GameObject> loadedChunks;
    public int NumChunksWide = 5;
    public int NumChunksHigh = 3;
    public Chunk[][] InstantiatedChunks = new Chunk[][] { };

    void Awake()
    {
        Instance = this;
        loadedChunks = new List<GameObject>(Resources.LoadAll<GameObject>("Chunks"));
    }

    void Start()
    {
        Generate();
    }

    void Update()
    {

    }

    void Generate()
    {
        InstantiatedChunks = new Chunk[NumChunksHigh][];
        for (int y = 0; y < NumChunksHigh; y++)
        {
            for (int x = 0; x < NumChunksWide; x++)
            {
                SpawnChunk(new Vector2Int(x, y), new Vector2Int(16, 16));
            }
        }
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

    private void SetTile(int chunkX, int chunkY, int tileX, int tileY, TileBase tile)
    {
        GetChunk(chunkX, chunkY).SetTile(new Vector3Int(tileX, tileY), tile);
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

    private bool ExistTile(int chunkX, int chunkY, int tileX, int tileY)
    {
        Chunk chunk = GetChunk(chunkX, chunkY);
        return chunk != null && chunk.ExistTile(tileX, tileY);
    }

    private bool CanOpenBottomLeft(int x, int y)
    {
        return ExistTile(x, y, 0, 1) && !ExistTile(x, y, 1, 1);
    }

    private bool CanOpenBottomRight(int x, int y)
    {
        return ExistTile(x, y, 15, 1) && !ExistTile(x, y, 14, 1);
    }

    private GameObject GetRandomChunk()
    {
        if (loadedChunks.Count == 0) return null;
        int index = Random.Range(0, loadedChunks.Count);
        return loadedChunks[index];
    }

    private GameObject SpawnChunk(Vector2Int gridPos, Vector2Int gridChunkSize)
    {
        var toInstantiate = GetRandomChunk();
        if (toInstantiate == null) return null;

        Vector3 pos = new Vector3(gridPos.x * gridChunkSize.x, -gridPos.y * gridChunkSize.y, 0);
        GameObject obj = Instantiate(toInstantiate, pos, Quaternion.identity, transform);
        Chunk chunk = obj.AddComponent<Chunk>();
        chunk.X = gridPos.x;
        chunk.Y = gridPos.y;
        if (InstantiatedChunks[gridPos.y] == null)
        {
            InstantiatedChunks[gridPos.y] = new Chunk[NumChunksWide];
        }
        InstantiatedChunks[gridPos.y][gridPos.x] = chunk;
        obj.name = gridPos.ToString();
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
        Tilemap tilemap = GetChunk(0, 0).GetTilemap();
        var cellPos = transform.position - new Vector3(position.x * tilemap.cellSize.x, position.y * tilemap.cellSize.y);
        int x = (int)-Mathf.Floor(cellPos.x) / 16;
        int y = (int)Mathf.Floor(cellPos.y + 16) / 16;
        return GetChunk(x,y);
    }
}
