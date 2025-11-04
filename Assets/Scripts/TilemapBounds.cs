#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
[RequireComponent(typeof(Tilemap))]
public class TilemapBounds : MonoBehaviour
{
    public Vector2Int minBounds = Vector2Int.zero;
    public Vector2Int maxBounds = new Vector2Int(15, 15);

    private Tilemap tilemap;

    void OnEnable() => tilemap = GetComponent<Tilemap>();

    void Update()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (pos.x < minBounds.x || pos.y < minBounds.y || pos.x > maxBounds.x || pos.y > maxBounds.y)
                tilemap.SetTile(pos, null);
        }
    }
}
#endif
