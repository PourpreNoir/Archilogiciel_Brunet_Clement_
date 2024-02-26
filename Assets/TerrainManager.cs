using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainManager : MonoBehaviour
{
  

// appel à chaque frame : lappui sur la touche P passera d'un mat à l'autre 

    public TMP_Text FPS_UItext;
    private int currentFPS;
    private float cumulatedTime;


    public GameObject F1;
    public bool F1bool = false;
    public GameObject F2;
    public bool F2bool = false;
    public GameObject F10;
    public bool F10bool = false;
    public Text dimensionText;
    public Text resolutionText;
    public Text radiusText;
    public Text intensityText;
    public static TerrainManager Instance { get; private set; }

    public float dimension;
    public float resolution;
    public AnimationCurve deformationCurve;
    public float radius = 1f;
    public float intensity = 1f;

    public GameObject terrainpref;

    public List<Terrain> terrains = new List<Terrain>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Instantiate(terrainpref);
        terrains.AddRange(FindObjectsOfType<Terrain>());
        currentFPS = 0;
        cumulatedTime = 0;
    }

    void Update()
    {
        currentFPS++;
        cumulatedTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.F12)){
           foreach (Terrain terrain in terrains)
            {
                terrain.Changematerial();
            }
        }
        
        if(cumulatedTime >1)
        {
            FPS_UItext.text = "FPS="+currentFPS.ToString()+"   FPS moyen="+Mathf.RoundToInt(Time.frameCount / Time.time);
            cumulatedTime -= 1;
            currentFPS = 0;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if(F1bool == true){
                F1.SetActive(false);
                F1bool = false;
            }
            else{
                F1.SetActive(true);
                F1bool = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if(F2bool == true){
                F2.SetActive(false);
                F2bool = false;
            }
            else{
                F2.SetActive(true);
                F2bool = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if(F10bool == true){
                F10.SetActive(false);
                F10bool = false;
            }
            else{
                F10.SetActive(true);
                F10bool = true;
            }
            // Update the text components with the current values of the parameters
            dimensionText.text = "Dimension: " + dimension;
            resolutionText.text = "Resolution: " + resolution;
            radiusText.text = "Radius: " + radius;
            intensityText.text = "Intensity: " + intensity;
        }
        if (Input.GetMouseButton(0))
        {
            //Detecter quel chunk est survolé dans la liste chunks
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                foreach (Terrain terrain in terrains)
                {
                    if (hit.collider == terrain.GetComponent<MeshCollider>())
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            terrain.DeformTerrainDown(hit.point);
                        }
                        else
                        {
                            terrain.DeformTerrainUp(hit.point);
                        }
                    }
                }
            }
        }
    
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            intensity += 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            intensity -= 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            radius += 5f;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            radius -= 5f;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            List<Terrain> tempterrains = new List<Terrain>(terrains);
            foreach (Terrain terrain in tempterrains)
            {
                terrain.AddTerrain(Vector3.forward, terrainpref);
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            List<Terrain> tempterrains = new List<Terrain>(terrains);
            foreach (Terrain terrain in tempterrains)
            {
                terrain.AddTerrain(Vector3.back, terrainpref);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            List<Terrain> tempterrains = new List<Terrain>(terrains);
            foreach (Terrain terrain in tempterrains)
            {
                terrain.AddTerrain(Vector3.left, terrainpref);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            List<Terrain> tempterrains = new List<Terrain>(terrains);
            foreach (Terrain terrain in tempterrains)
            {
                terrain.AddTerrain(Vector3.right, terrainpref);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            List<Terrain> tempterrains = new List<Terrain>(terrains);
            foreach (Terrain terrain in tempterrains)
            {
                terrain.HighlightChunks();
            }
            
        }
    }
    public Terrain GetNeighborChunk(Terrain terrain, Vector3 direction)
    {
        // Parcours de tous les chunks
        foreach (Terrain c in terrains)
        {
            // Si le chunk courant n'est pas le chunk passé en paramètre
            if (c != terrain)
            {
                // Calcul de la différence de position entre le chunk courant et le chunk passé en paramètre
                Vector3 diff = c.transform.position - terrain.transform.position;

                // Si la magnitude de cette différence est approximativement égale à la dimension du chunk
                // et si le produit scalaire entre la direction normalisée de cette différence et la direction normalisée passée en paramètre est approximativement égal à 1
                // (ce qui signifie que les deux directions sont les mêmes)
                if (Mathf.Approximately(diff.magnitude, dimension) && Mathf.Approximately(Vector3.Dot(diff.normalized, direction.normalized), 1))
                {
                    // Alors le chunk courant est le chunk voisin recherché et est retourné
                    return c;
                }
            }
        }

        // Si aucun chunk voisin n'est trouvé, la méthode retourne null
        return null;
    }
}
