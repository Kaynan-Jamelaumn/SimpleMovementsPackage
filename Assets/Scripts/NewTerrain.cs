using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralTerrain : MonoBehaviour
{
    [SerializeField] private int gridSize = 10; // Number of grid cells
    [SerializeField] private float scaleFactor = 1.0f; // Scale factor for the terrain size
    [SerializeField] private Material terrainMaterial; // Assign a material with a texture in the editor
    [SerializeField] private float lodDistance = 50f; // Distance for LOD switching
    [SerializeField] private float persistence = 0.6f; // Adjust persistence to control the smoothness of terrain
    [SerializeField] private int octaves = 4; // Octaves for fractal noise
    [SerializeField] private float baseFrequency = 0.5f; // Adjust base frequency to control the scale of terrain features
    [SerializeField] private float amplitude = 3f; // Adjust amplitude to control the height of terrain features

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private void Start()
    {
        mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Allows for larger meshes
        };
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        GenerateTerrainAsync();
    }

    private async void GenerateTerrainAsync()
    {
        await Task.Run(() => CreateShape());
        UpdateMesh();
        AssignMaterial();
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    private void CreateShape()
    {
        int width = gridSize;
        int height = gridSize;

        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        float[,] heightMap = new float[width + 1, height + 1];

        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                heightMap[x, y] = CalculateHeight(x, y);
            }
        }

        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                vertexList.Add(new Vector3(x * scaleFactor, heightMap[x, y], y * scaleFactor));
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int vert = y * (width + 1) + x;
                triangleList.Add(vert);
                triangleList.Add(vert + width + 1);
                triangleList.Add(vert + 1);

                triangleList.Add(vert + 1);
                triangleList.Add(vert + width + 1);
                triangleList.Add(vert + width + 2);
            }
        }

        vertices = vertexList.ToArray();
        triangles = triangleList.ToArray();
    }

    float CalculateHeight(int x, int y)
    {
        float amplitude = this.amplitude;
        float frequency = baseFrequency;
        float height = 0;

        for (int o = 0; o < octaves; o++)
        {
            float sampleX = x / (float)gridSize * frequency;
            float sampleY = y / (float)gridSize * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX + 0.5f, sampleY + 0.5f) * 2 - 1;
            height += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= 2;

            // Early exit optimization
            if (amplitude < 0.001f)
                break;
        }

        return height;
    }


    void AssignMaterial()
    {
        if (terrainMaterial != null)
        {
            GetComponent<MeshRenderer>().material = terrainMaterial;
        }
        else
        {
            Debug.LogWarning("Please assign a terrain material with a texture.");
        }
    }
    void Update()
    {
        // Check distance for LOD
        float distanceToPlayer = Vector3.Distance(transform.position, Camera.main.transform.position);
        if (distanceToPlayer > lodDistance)
        {
            // Apply LOD adjustments here
        }
        else
        {
            // Apply regular mesh rendering here
        }
    }
}