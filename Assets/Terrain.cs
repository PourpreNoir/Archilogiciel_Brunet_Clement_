using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Terrain : MonoBehaviour
{
    public int numMatTerrain;
    public Material[] materialsTerrain;
    private Mesh p_mesh;
    TerrainManager terrainmanager = TerrainManager.Instance;

    void Start()
    {
        p_mesh = new Mesh();
        p_mesh.Clear();

        int numVertices = Mathf.RoundToInt(terrainmanager.resolution) + 1;
        float stepSize = terrainmanager.dimension / terrainmanager.resolution;

        // Vérifiez si stepSize est NaN
        if (float.IsNaN(stepSize))
        {
            Debug.LogError("stepSize is NaN. Check the values of terrainmanager.dimension and terrainmanager.resolution.");
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float offsetX = -terrainmanager.dimension / 2;
        float offsetZ = -terrainmanager.dimension / 2;

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

    public void DeformTerrainUp(Vector3 hitPoint, bool propagateToNeighbors = true)
    {
        Vector3[] vertices = p_mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(hitPoint, transform.TransformPoint(vertices[i]));
            if (distance < terrainmanager.radius)
            {
                float deformation = terrainmanager.deformationCurve.Evaluate(distance / terrainmanager.radius) * terrainmanager.intensity;
                vertices[i] += deformation * Vector3.up;
            }
        }

        if (propagateToNeighbors)
        {
            Terrain[] neighbors = { terrainmanager.GetNeighborChunk(this, Vector3.left), terrainmanager.GetNeighborChunk(this, Vector3.right), terrainmanager.GetNeighborChunk(this, Vector3.forward), terrainmanager.GetNeighborChunk(this, Vector3.back) };
            foreach (Terrain neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    neighbor.DeformTerrainUp(hitPoint, false);
                }
            }
        }
        p_mesh.vertices = vertices;
        p_mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = p_mesh;
    }

    public void DeformTerrainDown(Vector3 hitPoint)
    {
        Vector3[] vertices = p_mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(hitPoint, transform.TransformPoint(vertices[i]));
            if (distance < terrainmanager.radius)
            {
                float deformation = terrainmanager.deformationCurve.Evaluate(distance / terrainmanager.radius) * terrainmanager.intensity;
                vertices[i] -= deformation * Vector3.up;
            }
        }
        p_mesh.vertices = vertices;
        p_mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = p_mesh;
    }

    public void AddTerrain(Vector3 direction, GameObject terrainpref)
    {
        // Calcule la position du nouveau terrain
        Vector3 newPosition = transform.position + direction * terrainmanager.dimension;

        // Détermine s'il y a déjà un terrain à cet emplacement
        Collider[] colliders = Physics.OverlapSphere(newPosition, terrainmanager.dimension/4);
        bool terrainAlreadyExists = colliders.Length > 0;

        if (!terrainAlreadyExists)
        {
            // Instancie un nouveau terrain à la position calculée
            GameObject newTerrain = Instantiate(terrainpref, newPosition, Quaternion.identity);
            newTerrain.name = "Terrain " + direction.ToString();

            // Ajoute le nouveau terrain à la liste des terrains gérés par le TerrainManager
            terrainmanager.terrains.Add(newTerrain.GetComponent<Terrain>());
            newTerrain.GetComponent<Terrain>().numMatTerrain = numMatTerrain;
        }
        else
        {
            Debug.LogWarning("Terrain already exists at " + newPosition);
        }

    }

    public void HighlightChunks()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material originalMaterial = renderer.material;

        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(Random.value, Random.value, Random.value);

        StartCoroutine(ResetMaterial(renderer, originalMaterial, 3f));
    }

    IEnumerator ResetMaterial(MeshRenderer renderer, Material originalMaterial, float delay)
    {
        yield return new WaitForSeconds(delay);
        renderer.material = originalMaterial;
    }
    public void Changematerial (){
        GetComponent<MeshRenderer>().material = materialsTerrain[(numMatTerrain++)% materialsTerrain.Length];
    }
}