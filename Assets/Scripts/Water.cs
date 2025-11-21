using UnityEngine;

public class Water : MonoBehaviour
{
    [Header("Wave Shape")]
    [SerializeField] int segments;
    [SerializeField] float width, height, amplitude;
    [SerializeField] int seed = 0;
    [HideInInspector] public float amplitudeMultiplier;

    [Header("Parallax Settings")]
    [SerializeField] Transform cam;
    [SerializeField] float parallaxEffect;
    float startPos;

    Mesh mesh;
    public Vector3[] baseVertices;
    [HideInInspector] public int[] topVerticesIndex;

    float phaseOffset;
    float frequency1, frequency2, frequency3;
    float noiseOffsetX, noiseOffsetT;
    float chaosOffsetX, chaosOffsetT;
    float speedMultiplier;

    private void Awake()
    {
        amplitudeMultiplier = 1f;
        startPos = transform.position.x;

        GenerateMesh();

        // Seed-based random wave pattern
        Random.InitState(seed);
        phaseOffset = Random.Range(0f, 1000f);
        frequency1 = Random.Range(0.8f, 1.4f);
        frequency2 = Random.Range(1.1f, 2.0f);
        frequency3 = Random.Range(0.4f, 0.9f);
        noiseOffsetX = Random.Range(0f, 200f);
        noiseOffsetT = Random.Range(0f, 200f);
        chaosOffsetX = Random.Range(0f, 200f);
        chaosOffsetT = Random.Range(0f, 200f);
        speedMultiplier = Random.Range(0.7f, 1.3f);
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Vector3[] verts = new Vector3[(segments + 1) * 2];
        int[] tris = new int[segments * 6];
        Vector2[] uvs = new Vector2[verts.Length];

        for (int i = 0; i <= segments; i++)
        {
            float x = (i / (float)segments) * width - width / 2f;
            verts[i] = new Vector3(x, 0f, 0f);                 // top row
            verts[i + segments + 1] = new Vector3(x, -height, 0f); // bottom row

            float u = i / (float)segments;
            uvs[i] = new Vector2(u, 1f);
            uvs[i + segments + 1] = new Vector2(u, 0f);
        }

        topVerticesIndex = new int[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            topVerticesIndex[i] = i; // top row vertices only
        }

        for (int i = 0; i < segments; i++)
        {
            int topLeft = i;
            int topRight = i + 1;
            int bottomLeft = i + segments + 1;
            int bottomRight = i + segments + 2;

            int triIndex = i * 6;
            tris[triIndex] = topLeft;
            tris[triIndex + 1] = topRight;
            tris[triIndex + 2] = bottomRight;
            tris[triIndex + 3] = topLeft;
            tris[triIndex + 4] = bottomRight;
            tris[triIndex + 5] = bottomLeft;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        baseVertices = mesh.vertices;
    }

    private void FixedUpdate()
    {
        AnimateSurface();
        if (parallaxEffect != 0) ParallaxMovement();
    }

    private void AnimateSurface()
    {
        Vector3[] verts = mesh.vertices;
        float t = Time.time * speedMultiplier + phaseOffset;

        for (int i = 0; i <= segments; i++)
        {
            float x = baseVertices[i].x;    
            float wave1 = Mathf.Sin(t * frequency1 + x) * amplitude;
            float wave2 = Mathf.Sin(t * frequency2 + x / 0.7f) * (amplitude * 0.6f);
            float wave3 = Mathf.Sin(t * frequency3 + x / 1.6f) * (amplitude * 0.3f);

            float randomAmp = Mathf.PerlinNoise((x + noiseOffsetX) * 0.5f, (t + noiseOffsetT) * 0.2f) * 0.8f + 0.6f;
            float chaos = Mathf.PerlinNoise((x + chaosOffsetX) * 1.2f, (t + chaosOffsetT) * 0.6f) * amplitude * 0.7f;

            verts[i].y = (wave1 + wave2 + wave3) * randomAmp * amplitudeMultiplier + chaos;
        }

        // Smooth endpoints
        float endY = (verts[0].y + verts[segments].y) * 0.5f;
        verts[0].y = endY;
        verts[segments].y = endY;

        for (int i = 1; i < segments; i++) // Soften segments to be less triangular
            verts[i].y = (verts[i - 1].y + verts[i].y + verts[i + 1].y) / 3f;

        mesh.vertices = baseVertices = verts;
    }

    private void ParallaxMovement()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);
        transform.position = new(startPos + distance, transform.position.y, transform.position.z);

        // If background has reached the end of its width then move it for infinite scrolling
        if (movement > startPos + width) { startPos += width; }
        else if (movement < startPos - width) { startPos -= width; }
    }
}