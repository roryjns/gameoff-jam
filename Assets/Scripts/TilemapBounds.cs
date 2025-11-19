using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
[RequireComponent(typeof(Tilemap))]
public class TilemapBounds : MonoBehaviour
{
    Vector2Int minBounds = Vector2Int.zero;
    Vector2Int maxBounds = new Vector2Int(15, 7);
    private Tilemap tilemap;
#if UNITY_EDITOR

    void OnEnable() => tilemap = GetComponent<Tilemap>();

    void Update()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (pos.x < minBounds.x || pos.y < minBounds.y || pos.x > maxBounds.x || pos.y > maxBounds.y)
                tilemap.SetTile(pos, null);
        }
    }
#endif
}
