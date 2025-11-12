using UnityEngine;

public class WaveSimulator : MonoBehaviour
{
    [Header("Wave Shape")]
    [SerializeField] int segments;
    [SerializeField] float width, height, amplitude;

    [Header("Tidal Wave")]
    [SerializeField] float waveDuration; // Wave rise and fall time
    [SerializeField] float dormantDuration; // How long between waves
    [SerializeField] float heightIncrease; // How much the wave rises by, reduce this each level to make it look like you are ascending the wave (20, 13, 7, 3?)
    [SerializeField] float flattenAmount; // 0-1, how much to flatten the top edge by upon reaching the peak
    [SerializeField] AnimationCurve verticalSpeed; // Decrease as the wave rises, increase as it falls
    
    Transform cameraTransform;
    Mesh mesh;
    Vector3[] baseVertices;
    float timer, dormantTimer, curvedT, startY;
    bool isDormant = true;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        BuildMesh();
        cameraTransform = transform.parent;
        startY = transform.position.y;
    }

    void BuildMesh()
    {
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
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        baseVertices = mesh.vertices;
    }

    void Update()
    {
        if (isDormant)
        {
            dormantTimer += Time.deltaTime;
            if (dormantTimer >= dormantDuration)
            {
                dormantTimer = 0f;
                isDormant = false;
            }
        }
        else
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / waveDuration;

            if (timer >= waveDuration)
            {
                normalizedTime = 1f;
                timer = 0f;
                isDormant = true;
                dormantTimer = 0f;
            }

            // Determine wave speed
            curvedT = verticalSpeed.Evaluate(normalizedTime);

            // Base Y position follows camera
            float baseY = cameraTransform.position.y;

            // Add the animated offset relative to that
            float offsetY = Mathf.Lerp(startY, heightIncrease, curvedT);

            transform.position = new Vector3(
                transform.position.x,
                baseY + offsetY,
                transform.position.z
            );

            float flatten;
            if (normalizedTime < 0.8f)
            {
                float riseT = normalizedTime / 0.8f;
                flatten = Mathf.Lerp(1f, 1f - flattenAmount, riseT);
            }
            else
            {
                flatten = 1f - flattenAmount; // Stay flattened during fall
            }

            // Wave motion along the top edge
            AnimateSurface(flatten);
        }
    }

    void AnimateSurface(float flatten)
    {
        Vector3[] verts = mesh.vertices;

        for (int i = 0; i <= segments; i++)
        {
            float x = baseVertices[i].x;

            float wave1 = Mathf.Sin(Time.time + x) * amplitude;
            float wave2 = Mathf.Sin(Time.time * 1.4f + x / 0.7f) * (amplitude * 0.6f);
            float wave3 = Mathf.Sin(Time.time * 0.6f + x / 1.6f) * (amplitude * 0.3f);

            float randomAmp = Mathf.PerlinNoise(x * 0.5f, Time.time * 0.2f) * 0.8f + 0.6f;
            float chaos = Mathf.PerlinNoise(x * 1.2f, Time.time * 0.6f) * (1f - flatten) * amplitude * 0.7f;

            verts[i].y = (wave1 + wave2 + wave3) * randomAmp * flatten + chaos;
        }

        for (int i = 1; i < segments; i++) // soften to make segments less triangular
        {
            verts[i].y = (verts[i - 1].y + verts[i].y + verts[i + 1].y) / 3f;
        }

        mesh.vertices = verts;
        mesh.RecalculateBounds();
    }
}