
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Grid))]
public class GridBoundaryGizmo : MonoBehaviour
{
    [Header("Grid Boundary Settings")]
    Vector2Int gridSize = new Vector2Int(16,  8);
    public Color boundaryColor = Color.yellow;           
    public bool drawCenterCross = true;                  

    private Grid grid;
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (grid == null)
            grid = GetComponent<Grid>();

        Gizmos.color = boundaryColor;

        Vector3 cellSize = grid.cellSize;
        Vector3 origin = transform.position;

        float width = gridSize.x * cellSize.x;
        float height = gridSize.y * cellSize.y;

        Vector3 bottomLeft = origin;
        Vector3 bottomRight = origin + new Vector3(width, 0, 0);
        Vector3 topLeft = origin + new Vector3(0, height, 0);
        Vector3 topRight = origin + new Vector3(width, height, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        if (drawCenterCross)
        {
            Vector3 center = origin + new Vector3(width / 2f, height / 2f, 0);
            float crossSize = Mathf.Min(cellSize.x, cellSize.y) * 0.5f;

            Gizmos.DrawLine(center + Vector3.left * crossSize, center + Vector3.right * crossSize);
            Gizmos.DrawLine(center + Vector3.up * crossSize, center + Vector3.down * crossSize);
        }
    }
#endif
}