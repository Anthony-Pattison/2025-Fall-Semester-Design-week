using UnityEngine;

public class WorldBoundaries : MonoBehaviour
{
    [Header("Terrain Reference")]
    [SerializeField] private Terrain terrain;

    [Header("Boundary Settings")]
    [SerializeField] private float boundaryHeight = 200f;
    [SerializeField] private float boundaryThickness = 10f;
    [SerializeField] private bool showBoundariesInEditor = true;

    private Vector3 terrainCenter;
    private Vector3 terrainSize;

    void Start()
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }

        if (terrain != null)
        {
            CreateBoundaries();
        }
        else
        {
            Debug.LogError("No terrain found! Please assign a terrain.");
        }
    }

    void CreateBoundaries()
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // Calculate terrain center and size
        terrainSize = terrainData.size;
        terrainCenter = terrainPos + new Vector3(terrainSize.x / 2f, 0f, terrainSize.z / 2f);

        // Create 4 walls around the terrain
        CreateWall("North Wall",
            new Vector3(terrainCenter.x, boundaryHeight / 2f, terrainPos.z + terrainSize.z),
            new Vector3(terrainSize.x + boundaryThickness * 2, boundaryHeight, boundaryThickness));

        CreateWall("South Wall",
            new Vector3(terrainCenter.x, boundaryHeight / 2f, terrainPos.z),
            new Vector3(terrainSize.x + boundaryThickness * 2, boundaryHeight, boundaryThickness));

        CreateWall("East Wall",
            new Vector3(terrainPos.x + terrainSize.x, boundaryHeight / 2f, terrainCenter.z),
            new Vector3(boundaryThickness, boundaryHeight, terrainSize.z));

        CreateWall("West Wall",
            new Vector3(terrainPos.x, boundaryHeight / 2f, terrainCenter.z),
            new Vector3(boundaryThickness, boundaryHeight, terrainSize.z));

        Debug.Log($"World boundaries created! Terrain size: {terrainSize}, Center: {terrainCenter}");
    }

    void CreateWall(string name, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        wall.transform.position = position;
        wall.layer = LayerMask.NameToLayer("Default");
 
        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
    }

    Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = {
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)
        };

        int[] triangles = {
            0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4, 1, 2, 6, 1, 6, 5,
            2, 3, 7, 2, 7, 6, 3, 0, 4, 3, 4, 7
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // Public getter for terrain center 
    public Vector3 GetTerrainCenter()
    {
        if (terrain != null)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            return terrainPos + new Vector3(terrainData.size.x / 2f, 0f, terrainData.size.z / 2f);
        }
        return Vector3.zero;
    }

    // Public getter for terrain size 
    public Vector3 GetTerrainSize()
    {
        if (terrain != null)
        {
            return terrain.terrainData.size;
        }
        return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (terrain != null && showBoundariesInEditor)
        {
            Gizmos.color = Color.red;
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            Vector3 size = terrainData.size;
            Vector3 center = terrainPos + new Vector3(size.x / 2f, boundaryHeight / 2f, size.z / 2f);

            Gizmos.DrawWireCube(center, new Vector3(size.x, boundaryHeight, size.z));
        }
    }
}