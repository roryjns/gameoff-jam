using UnityEngine;
using System.Collections.Generic;

public class InteractableWater : MonoBehaviour
{
    [SerializeField] int wavePropagationIterations;
    [SerializeField] float springConstant, damping, spread, forceMultiplier, maxForce;
    [SerializeField] LayerMask waterMask;

    Mesh mesh;
    Wave wave;
    EdgeCollider2D edgeCollider;
    int[] topVerticesIndex;
    Vector3[] vertices;
    float[] splashOffset;  // length = topVerticesIndex.Length

    public static WaterSplash Instance;

    private class WaterPoint
    {
        public float velocity, pos, targetHeight;
    }

    private readonly List<WaterPoint> waterPoints = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        edgeCollider = GetComponent<EdgeCollider2D>();
        wave = GetComponent<Wave>();
        vertices = mesh.vertices;
        topVerticesIndex = wave.topVerticesIndex;
        splashOffset = new float[topVerticesIndex.Length];
        CreateWaterPoints();
    }

    private void CreateWaterPoints()
    {
        waterPoints.Clear();

        for (int i = 0; i < topVerticesIndex.Length; i++)
        {
            waterPoints.Add(new WaterPoint
            {
                pos = mesh.vertices[topVerticesIndex[i]].y,
                targetHeight = mesh.vertices[topVerticesIndex[i]].y,
            });
        }
    }

    private void FixedUpdate()
    {
        // Update spring positions
        for (int i = 1; i < waterPoints.Count - 1; i++)
        {
            WaterPoint point = waterPoints[i];
            float x = point.pos - point.targetHeight;
            float acceleration = -springConstant * x - damping * point.velocity;
            point.pos += point.velocity * Time.fixedDeltaTime;
            splashOffset[i] = point.pos;
            point.velocity += acceleration * Time.fixedDeltaTime;
        }

        // Wave propagation
        for (int j = 0; j < wavePropagationIterations; j++)
        {
            for (int i = 1; i < waterPoints.Count - 1; i++)
            {
                float leftDelta = spread * (waterPoints[i].pos - waterPoints[i - 1].pos) * Time.fixedDeltaTime;
                waterPoints[i - 1].velocity += leftDelta;
                float rightDelta = spread * (waterPoints[i].pos - waterPoints[i + 1].pos) * Time.fixedDeltaTime;
                waterPoints[i + 1].velocity += rightDelta;
            }
        }

        for (int i = 0; i < topVerticesIndex.Length; i++)
        {
            int v = topVerticesIndex[i];
            vertices[v].y = wave.baseVertices[v].y + splashOffset[i];
        }

        mesh.vertices = vertices;
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        Vector2[] colliderPoints = new Vector2[topVerticesIndex.Length];
        for (int i = 0; i < topVerticesIndex.Length; i++) colliderPoints[i] = (Vector2)vertices[topVerticesIndex[i]];
        edgeCollider.points = colliderPoints;
    }

    private void Splash(Collider2D collision, float force)
    {
        float radius = collision.bounds.extents.x * 4f;
        Vector2 center = collision.transform.position;

        for (int i = 0; i < waterPoints.Count; i++)
        {
            Vector2 vertexWorldPos = transform.TransformPoint(vertices[topVerticesIndex[i]]);
            float distanceSquared = (vertexWorldPos - center).sqrMagnitude;
            if (distanceSquared <= radius * radius)
            {
                waterPoints[i].velocity = force;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((waterMask.value & (1 << collision.gameObject.layer)) == 0) return;
        if (!collision.TryGetComponent<Rigidbody2D>(out var rb)) return;

        Vector3 spawnPos;

        if (collision.bounds.center.y >= edgeCollider.points[1].y + edgeCollider.offset.y + gameObject.transform.localPosition.y) 
        {
            // Hit from above, now underwater
            spawnPos = new Vector2(collision.transform.position.x, collision.bounds.min.y);
            if (collision.gameObject.TryGetComponent<PlayerController>(out var player)) player.underwater = true;
            else if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.underwater = true;
                enemy.Heal();
            }
        }
        else
        {
            // Hit from below, now above water
            spawnPos = new Vector2(collision.transform.position.x, collision.bounds.max.y);            
        }

        ObjectPooler.Instance.GetFromPool("Splash Particle", spawnPos, Quaternion.identity);
        float vel = Mathf.Clamp(Mathf.Abs(rb.linearVelocity.y), 0f, maxForce);
        vel *= rb.linearVelocity.y >= 0 ? 1 : -1;
        Splash(collision, vel);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            if (collision.bounds.center.y >= edgeCollider.points[1].y + edgeCollider.offset.y + gameObject.transform.localPosition.y)
                player.underwater = false;
            else
                player.underwater = true;
        }

        else if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            if (collision.bounds.center.y >= edgeCollider.points[1].y + edgeCollider.offset.y + gameObject.transform.localPosition.y)
                enemy.underwater = false;
            else
                enemy.underwater = true;
        }
    }
}