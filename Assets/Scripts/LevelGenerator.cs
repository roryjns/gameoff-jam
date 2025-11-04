using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public class LevelGenerator : MonoBehaviour
{
    private List<GameObject> loadedChunks;
    public int NumChunksWide = 5;
    public int NumChunksHeight = 5;
    private GameObject[][] instantiatedChunks = new GameObject[][] { };

    void Awake()
    {
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
        instantiatedChunks = new GameObject[NumChunksHeight][];
        for (int y = 0; y < NumChunksHeight; y++)
        {
            for (int x = 0; x < NumChunksHeight; x++)
            {
                SpawnChunk(new Vector2Int(x, y), new Vector2Int(16, 16));
            }
        }
        for (int y = 0; y < NumChunksHeight; y++)
        {
            for (int x = 0; x < NumChunksHeight; x++)
            {
                if (CanOpenBottomRight(x, y) && CanOpenBottomLeft(x + 1, y))
                {
                    OpenBottomRight(x, y);
                    OpenBottomLeft(x + 1, y);
                }
            }
        }
    }

    private Tilemap GetTilemap(int x, int y)
    {
        return instantiatedChunks[y][x].GetComponentInChildren<Tilemap>();
    }

    private void SetTile(int chunkX, int chunkY, int tileX, int tileY, TileBase tile)
    {
        GetTilemap(chunkX, chunkY).SetTile(new Vector3Int(tileX, tileY), tile);
    }

    private void OpenBottomLeft(int x, int y)
    {
        SetTile(x, y, 0, 1, null);
        SetTile(x, y, 0, 2, null);
    }

    private void OpenBottomRight(int x, int y)
    {
        SetTile(x, y, 15, 1, null);
        SetTile(x, y, 15, 2, null);
    }

    private bool ExistTile(int chunkX, int chunkY, int tileX, int tileY)
    {
        if (chunkX >= NumChunksWide || chunkY >= NumChunksHeight)
        {
            return false;
        }
        return GetTilemap(chunkX, chunkY).GetTile(new Vector3Int(tileX, tileY));
    }

    private bool CanOpenBottomLeft(int x, int y)
    {
        return ExistTile(x, y, 0, 1) && !ExistTile(x, y, 1, 1);
    }

    private bool CanOpenBottomRight(int x, int y)
    {
        return ExistTile(x, y, 15, 1) && !ExistTile(x, y, 14, 1);
    }

    public GameObject GetRandomChunk()
    {
        if (loadedChunks.Count == 0) return null;
        int index = Random.Range(0, loadedChunks.Count);
        return loadedChunks[index];
    }

    public GameObject SpawnChunk(Vector2Int gridPos, Vector2Int gridChunkSize)
    {
        var chunk = GetRandomChunk();
        if (chunk == null) return null;

        Vector3 pos = new Vector3(gridPos.x * gridChunkSize.x, -gridPos.y * gridChunkSize.y, 0);
        GameObject obj = Instantiate(chunk, pos, Quaternion.identity, transform);
        if (instantiatedChunks[gridPos.y] == null)
        {
            instantiatedChunks[gridPos.y] = new GameObject[NumChunksWide];
        }
        instantiatedChunks[gridPos.y][gridPos.x] = obj;
        obj.name = gridPos.ToString();
        return obj;
    }
}
