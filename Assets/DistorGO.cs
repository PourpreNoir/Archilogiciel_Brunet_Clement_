using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistortGO : MonoBehaviour
{
    private Vector3[] p_vertices;
    private Mesh p_mesh;
    private Dictionary<int, List<int>> les_po = new Dictionary<int, List<int>>();

    public GameObject PickObj; // Define PickObj as a public variable

    void Start()
    {
        p_mesh = GetComponent<MeshFilter>().mesh;
        p_vertices = p_mesh.vertices;

        const float SEUIL_DISTANCE_VERTICES_SIMILAIRES = 0.01f;
        bool[] bool_vert = new bool[p_vertices.Length];

        for (int i = 0; i < bool_vert.Length; i++)
        {
            bool_vert[i] = false;
        }

        int index_vert = 0;
        int nb_po = 0;

        while (index_vert < p_vertices.Length)
        {
            if (!bool_vert[index_vert])
            {
                bool_vert[index_vert] = true;

                GameObject po = Instantiate(PickObj);
                po.name = "po" + nb_po;
                nb_po++;

                po.transform.position = transform.TransformPoint(p_vertices[index_vert]);
                les_po.Add(po.GetInstanceID(), new List<int> { index_vert });

                // Vérifier quels sont les vertices similaires
                for (int i = index_vert + 1; i < p_vertices.Length; i++)
                {
                    if (!bool_vert[i] && Vector3.Distance(p_vertices[index_vert], p_vertices[i]) < SEUIL_DISTANCE_VERTICES_SIMILAIRES)
                    {
                        bool_vert[i] = true;
                        les_po[po.GetInstanceID()].Add(i);
                    }
                }

                // Ajouter un composant pour détecter les mouvements de la souris
                po.AddComponent<DragObject>().SetDistortGO(this);
            }

            index_vert++;
        }
    }

    // Méthode appelée lorsqu'un PickingObject est déplacé
    public void OnPickingObjectMoved(int poInstanceID, Transform x)
    {

        
            // Appliquer le déplacement aux vertices associés
            foreach (int vertexIndex in les_po[poInstanceID])
            {
                Debug.Log(p_vertices[vertexIndex]);
                p_vertices[vertexIndex] = transform.InverseTransformPoint(x.position);
            }

            // Mettre à jour le maillage avec les nouvelles positions des vertices
            p_mesh.vertices = p_vertices;
            p_mesh.RecalculateBounds();
            p_mesh.RecalculateNormals();
        
    }
}
