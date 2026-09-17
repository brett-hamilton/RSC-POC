using UnityEngine;

public enum TileType { Grass, Dirt, Stone, Water }

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RSCTerrainMesh : MonoBehaviour
{
    public int width = 20;
    public int depth = 20;
    public float tileSize = 1f;
    public float[,] heights;
    public TileType[,] tileTypes;

    // RSC-style flat color palette per tile type
    static readonly Color ColorGrass = new Color(0.29f, 0.51f, 0.20f);
    static readonly Color ColorDirt  = new Color(0.45f, 0.34f, 0.20f);
    static readonly Color ColorStone = new Color(0.55f, 0.55f, 0.55f);
    static readonly Color ColorWater = new Color(0.20f, 0.35f, 0.65f);

    Color ColorForType(TileType type)
    {
        switch (type)
        {
            case TileType.Dirt:  return ColorDirt;
            case TileType.Stone: return ColorStone;
            case TileType.Water: return ColorWater;
            default:             return ColorGrass;
        }
    }

    void Start()
    {
        if (heights == null) heights = GenerateSampleHeights();
        if (tileTypes == null) tileTypes = GenerateSampleTypes();
        GenerateMesh();
    }

    float[,] GenerateSampleHeights()
    {
        float[,] h = new float[width + 1, depth + 1];
        for (int x = 0; x <= width; x++)
            for (int z = 0; z <= depth; z++)
                h[x, z] = Mathf.PerlinNoise(x * 0.2f, z * 0.2f) * 2f;
        return h;
    }

    TileType[,] GenerateSampleTypes()
    {
        TileType[,] t = new TileType[width, depth];
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                t[x, z] = (x + z) % 7 == 0 ? TileType.Dirt : TileType.Grass; // placeholder pattern
        return t;
    }

    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "RSC Terrain";

        int tileCount = width * depth;
        Vector3[] vertices = new Vector3[tileCount * 4];
        int[] triangles = new int[tileCount * 6];
        Color[] colors = new Color[tileCount * 4];
        Vector3[] normals = new Vector3[tileCount * 4];

        int v = 0, t = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Vector3 p0 = new Vector3(x * tileSize, heights[x, z], z * tileSize);
                Vector3 p1 = new Vector3((x + 1) * tileSize, heights[x + 1, z], z * tileSize);
                Vector3 p2 = new Vector3(x * tileSize, heights[x, z + 1], (z + 1) * tileSize);
                Vector3 p3 = new Vector3((x + 1) * tileSize, heights[x + 1, z + 1], (z + 1) * tileSize);

                vertices[v + 0] = p0;
                vertices[v + 1] = p1;
                vertices[v + 2] = p2;
                vertices[v + 3] = p3;

                Vector3 flatNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                normals[v + 0] = normals[v + 1] = normals[v + 2] = normals[v + 3] = flatNormal;

                Color tileColor = ColorForType(tileTypes[x, z]);
                colors[v + 0] = colors[v + 1] = colors[v + 2] = colors[v + 3] = tileColor;

                triangles[t + 0] = v + 0;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;

                v += 4;
                t += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.normals = normals;

        GetComponent<MeshFilter>().mesh = mesh;

        // keep the collider in sync with the generated mesh so painting/raycasts work
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = null;      // force Unity to refresh it
        meshCollider.sharedMesh = mesh;
    }

    public void SetTileType(int x, int z, TileType type)
    {
        if (x < 0 || x >= width || z < 0 || z >= depth) return;
        tileTypes[x, z] = type;
        GenerateMesh(); // rebuild immediately so you see the paint update
    }

    public void EnsureInitialized()
    {
        if (heights == null) heights = GenerateSampleHeights();
        if (tileTypes == null) tileTypes = GenerateSampleTypes();
    }
}