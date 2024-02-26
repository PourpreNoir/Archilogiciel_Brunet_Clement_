using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DistortGO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class ProceduralTerrain : MonoBehaviour
{
    private int numMatTerrain;
    public Material[] materialsTerrain;
    private Mesh p_mesh;
    public float dimension;
    public float resolution;
    public AnimationCurve deformationCurve;
    public List<AnimationCurve> deformationPatterns;
    private int currentPatternIndex = 0;
    public float radius = 1f;
    public float intensity = 1f;

    public GameObject PickObj;

    void Start()
    {
        p_mesh = new Mesh();
        p_mesh.Clear();

        int numVertices = Mathf.RoundToInt(resolution) + 1;
        float stepSize = dimension / resolution;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float offsetX = -dimension / 2;
        float offsetZ = -dimension / 2;

        for (int i = 0; i < numVertices; i++)
        {
            for (int j = 0; j < numVertices; j++)
            {
                float x = i * stepSize + offsetX;
                float z = j * stepSize + offsetZ;
                float y = 0;

                vertices.Add(new Vector3(x, y, z));

                if (i < numVertices - 1 && j < numVertices - 1)
                {
                    int topLeft = i * numVertices + j;
                    int topRight = topLeft + 1;
                    int bottomLeft = (i + 1) * numVertices + j;
                    int bottomRight = bottomLeft + 1;

                    triangles.Add(topLeft);
                    triangles.Add(topRight);
                    triangles.Add(bottomLeft);

                    triangles.Add(topRight);
                    triangles.Add(bottomRight);
                    triangles.Add(bottomLeft);
                }
            }
        }

        p_mesh.vertices = vertices.ToArray();
        p_mesh.triangles = triangles.ToArray();
        p_mesh.RecalculateNormals();
        p_mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = p_mesh;
        GetComponent<MeshCollider>().sharedMesh = p_mesh;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12)) 
                GetComponent<MeshRenderer>().material = materialsTerrain[(numMatTerrain++)% materialsTerrain.Length];
        // Modify intensity with Left-SHIFT and Right-SHIFT
        if (Input.GetKey(KeyCode.LeftShift))
        {
            intensity -= 0.1f; // Decrease intensity
        }
        else if (Input.GetKey(KeyCode.RightShift))
        {
            intensity += 0.1f; // Increase intensity
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Switch to the next pattern
            currentPatternIndex = (currentPatternIndex + 1) % deformationPatterns.Count;
            deformationCurve = deformationPatterns[currentPatternIndex];
        }

        // Modify radius with + and -
        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus))
        {
            radius += 0.1f; // Increase radius
        }
        else if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {
            radius -= 0.1f; // Decrease radius
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                ProceduralTerrain hitTerrain = hit.collider.GetComponent<ProceduralTerrain>();
                if (hitTerrain != null)
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        hitTerrain.DeformTerrainDown(hit.point);
                    }
                    else
                    {
                        hitTerrain.DeformTerrainUp(hit.point);
                    }
                }
            }
        }
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ExtendTerrain(Vector3.forward);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ExtendTerrain(Vector3.back);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ExtendTerrain(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ExtendTerrain(Vector3.right);
            }
        }

        // Highlight chunks with M
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(HighlightChunks());
        }
    }

    IEnumerator HighlightChunks()
    {
        // Store the original material
        Material originalMaterial = GetComponent<MeshRenderer>().material;

        // Create a new material for highlighting
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = new Color(Random.value, Random.value, Random.value);

        // Apply the new material
        GetComponent<MeshRenderer>().material = newMaterial;

        // Wait for 3 seconds
        yield return new WaitForSeconds(3);

        // Revert to the original material
        GetComponent<MeshRenderer>().material = originalMaterial;
    }
    

    private void DeformTerrainUp(Vector3 hitPoint)
    {
        Vector3[] vertices = p_mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(hitPoint, vertices[i]);
            if (distance < radius)
            {
                float deformation = deformationCurve.Evaluate(distance / radius) * intensity;
                vertices[i] += deformation * Vector3.up;
            }
        }
        p_mesh.vertices = vertices;
        p_mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = p_mesh;
    }

    private void DeformTerrainDown(Vector3 hitPoint)
    {
        Vector3[] vertices = p_mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(hitPoint, vertices[i]);
            if (distance < radius)
            {
                float deformation = deformationCurve.Evaluate(distance / radius) * intensity;
                vertices[i] -= deformation * Vector3.up;
            }
        }
        p_mesh.vertices = vertices;
        p_mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = p_mesh;
    }
    public void ExtendTerrain(Vector3 direction)
    {
        // Calculate the position for the new terrain
        Vector3 newPosition = transform.position + direction * dimension;
        Debug.Log("New position: " + newPosition);

        // Check if there's already a terrain at the new position
        Collider[] colliders = Physics.OverlapSphere(newPosition, dimension / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<ProceduralTerrain>() != null)
            {
                // There's already a terrain here, so return without creating a new one
                Debug.Log("Terrain already exists at new position");
                return;
            }
        }

        // Create a new GameObject for the extended terrain
        GameObject extendedTerrain = new GameObject("ExtendedTerrain");
        extendedTerrain.AddComponent<MeshFilter>();
        extendedTerrain.AddComponent<MeshRenderer>();
        extendedTerrain.AddComponent<MeshCollider>();

        // Generate the new terrain
        ProceduralTerrain terrainComponent = extendedTerrain.AddComponent<ProceduralTerrain>();
        terrainComponent.dimension = dimension;
        terrainComponent.resolution = resolution;
        terrainComponent.deformationCurve = deformationCurve;
        terrainComponent.deformationPatterns = deformationPatterns;
        terrainComponent.currentPatternIndex = currentPatternIndex;
        terrainComponent.radius = radius;
        terrainComponent.intensity = intensity;

        // Set the position of the new terrain
        extendedTerrain.transform.position = newPosition;

        // Start the terrain generation
        Debug.Log("Starting terrain generation");
        terrainComponent.Start();
    }
}