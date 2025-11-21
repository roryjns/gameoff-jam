using UnityEngine;
using System.Collections.Generic;

public class WaterSplash : MonoBehaviour
{
    [Header("Springs")]
    [SerializeField] int wavePropagationIterations;
    [SerializeField] float springConstant, damping, spread;

    [Header("Force")]
    [SerializeField] float forceMultiplier;
    [SerializeField] float maxForce;

    [Header("Collision")]
    [SerializeField] float playerCollisionRadiusMultiplier;
    
    Mesh mesh;
    Water water;
    int[] topVerticesIndex;
    Vector3[] vertices;
    float[] splashOffset;  // length = topVerticesIndex.Length

    [SerializeField] LayerMask waterMask;
    EdgeCollider2D edgeCollider;

    private class WaterPoint
    {
        public float velocity, pos, targetHeight;
    }

    private readonly List<WaterPoint> waterPoints = new();

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        edgeCollider = GetComponent<EdgeCollider2D>();
        water = GetComponent<Water>();
        vertices = mesh.vertices;
        topVerticesIndex = water.topVerticesIndex;
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
            vertices[v].y = water.baseVertices[v].y + splashOffset[i];
        }

        mesh.vertices = vertices;
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        Vector2[] colliderPoints = new Vector2[topVerticesIndex.Length];

        for (int i = 0; i < topVerticesIndex.Length; i++)
        {
            colliderPoints[i] = (Vector2)vertices[topVerticesIndex[i]];
        }

        edgeCollider.points = colliderPoints;
    }

    private void Splash(Collider2D collision, float force)
    {
        Debug.Log("splash " + force);
        float radius = collision.bounds.extents.x * playerCollisionRadiusMultiplier;
        Vector2 center = collision.transform.position;

        for (int i = 0; i < waterPoints.Count; i++)
        {
            Vector2 vertexWorldPos = transform.TransformPoint(vertices[topVerticesIndex[i]]);
            float distanceSquared = (vertexWorldPos - center).sqrMagnitude;
            if (distanceSquared <= radius * radius)
            {
                waterPoints[i].velocity = force;
                Debug.Log("adjusted point");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((waterMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (collision.TryGetComponent<Rigidbody2D>(out var rb))
            {
                /*
                // Spawn particles
                Vector2 localPos = gameObject.transform.localPosition;
                Vector2 hitObjectPos = collision.transform.position;
                Bounds hitObjectBounds = collision.bounds;
                Vector3 spawnPos = Vector3.zero;

                if (collision.transform.position.y >= edgeCollider.points[1].y + edgeCollider.offset.y + localPos.y)
                    spawnPos = hitObjectPos - new Vector2(0f, hitObjectBounds.extents.y); // Hit from above
                else
                    spawnPos = hitObjectPos + new Vector2(0f, hitObjectBounds.extents.y); // Hit from below
                
                Instantiate(splashParticles, spawnPos, Quaternion.identity); 
                */

                float velocity = rb.linearVelocity.y * forceMultiplier;
                velocity = Mathf.Clamp(Mathf.Abs(velocity), 0f, maxForce);
                float multiplier = rb.linearVelocity.y >= 0 ? 1 : -1;
                velocity *= multiplier;
                Splash(collision, velocity);
            }
        }
    }
}