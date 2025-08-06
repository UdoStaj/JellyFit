using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightCubes : MonoBehaviour
{
    public float highlightY;
    public List<GameObject> cubes = new List<GameObject>();
    private Dictionary<GameObject, GameObject> cubeHighlights = new Dictionary<GameObject, GameObject>(); // Dictionary to track highlights for each cube
    public static HighlightCubes instance;
    private ObjectPooling objectPooling; // Object pooling script reference
    public List<GridSystem> allGrids = new List<GridSystem>();
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        allGrids.Add(GridSystem.instance); // GridSystem'i listeye ekle
        
        objectPooling = ObjectPooling.Instance; // Object pooling script instance
        if (objectPooling == null)
        {
            Debug.LogError("ObjectPooling instance not found!");
            return;
        }
    }

    public void UpdateHighlights()
    {
        HashSet<Transform> processedGroups = new HashSet<Transform>(); //Aynı grup tekrar işlenmesin

        foreach (GameObject cube in cubes)
        {
            Transform parentGroup = cube.transform.parent;
            if (parentGroup == null || processedGroups.Contains(parentGroup))
                continue;

            processedGroups.Add(parentGroup);

            DragAndDrop dragDropScript = parentGroup.GetComponent<DragAndDrop>();
            bool isDragging = dragDropScript != null && dragDropScript.isDragging;

            if (isDragging)
            {
                // Objenin gerçekten hangi grid üzerinde olduğunu tespit et
                GridSystem activeGrid = GetGridUnderPosition(parentGroup.position);

                if (activeGrid != null)
                {
                    // Önce grubun tüm küplerinin bu grid içinde olup olmadığını kontrol et
                    bool allCubesInsideGrid = true;
                    for (int i = 0; i < parentGroup.transform.childCount; i++)
                    {
                        Transform child = parentGroup.transform.GetChild(i);
                        if (!IsPositionInsideGridBounds(child.position, activeGrid))
                        {
                            allCubesInsideGrid = false;
                            break;
                        }
                    }

                    if (allCubesInsideGrid)
                    {
                        // Tüm küpler grid içindeyse highlight'ları göster/güncelle
                        for (int i = 0; i < parentGroup.transform.childCount; i++)
                        {
                            Transform child = parentGroup.transform.GetChild(i);
                            Vector2Int coordinates = activeGrid.GetCoordinatesFromPosition(child.position);
                            Vector3 pos = activeGrid.GetPositionFromCoordinates(coordinates);

                            if (!cubeHighlights.ContainsKey(child.gameObject))
                            {
                                GameObject highlight = objectPooling.GetHighlight();
                                highlight.transform.position = new Vector3(pos.x, highlightY, pos.z);
                                cubeHighlights[child.gameObject] = highlight;
                            }
                            else
                            {
                                cubeHighlights[child.gameObject].transform.position = new Vector3(pos.x, highlightY, pos.z);
                            }
                        }
                    }
                    else
                    {
                        // Herhangi bir küp grid dışındaysa tüm grubun highlight'larını kaldır
                        for (int i = 0; i < parentGroup.childCount; i++)
                        {
                            GameObject child = parentGroup.GetChild(i).gameObject;
                            if (cubeHighlights.ContainsKey(child))
                            {
                                objectPooling.DisableHighlight(cubeHighlights[child]);
                                cubeHighlights.Remove(child);
                            }
                        }
                    }
                }
                else
                {
                    // Hiçbir grid üzerinde değilse tüm highlight'ları kaldır
                    for (int i = 0; i < parentGroup.childCount; i++)
                    {
                        GameObject child = parentGroup.GetChild(i).gameObject;
                        if (cubeHighlights.ContainsKey(child))
                        {
                            objectPooling.DisableHighlight(cubeHighlights[child]);
                            cubeHighlights.Remove(child);
                        }
                    }
                }
            }
            else
            {
                // Grubun tüm highlight'larını kaldır
                for (int i = 0; i < parentGroup.childCount; i++)
                {
                    GameObject child = parentGroup.GetChild(i).gameObject;
                    if (cubeHighlights.ContainsKey(child))
                    {
                        objectPooling.DisableHighlight(cubeHighlights[child]);
                        cubeHighlights.Remove(child);
                    }
                }
            }
        }
    }

    // Pozisyonun grid sınırları içinde olup olmadığını kontrol eden fonksiyon
    private bool IsPositionInsideGridBounds(Vector3 position, GridSystem gridSystem)
    {
        // Grid'in dünya pozisyonundaki sınırlarını hesapla
        Vector3 gridCenter = gridSystem.transform.position;
        float halfHorizontal = gridSystem.horizontalLength / 2f;
        float halfVertical = gridSystem.verticalLength / 2f;

        // Pozisyon grid sınırları içinde mi kontrol et
        return position.x >= gridCenter.x - halfHorizontal &&
               position.x <= gridCenter.x + halfHorizontal &&
               position.z >= gridCenter.z - halfVertical &&
               position.z <= gridCenter.z + halfVertical;
    }

    // Verilen pozisyonun hangi grid üzerinde olduğunu tespit eden fonksiyon
    private GridSystem GetGridUnderPosition(Vector3 position)
    {

        foreach (GridSystem grid in allGrids)
        {
            if (IsPositionInsideGridBounds(position, grid))
            {
                return grid;
            }
        }

        return null; // Hiçbir grid üzerinde değil
    }

    

    //PLAY BUTONUNA ATANACAK
    public void UpdateCubeList()
    {
        cubes.Clear();
        StartCoroutine(UpdateCubeListCoroutine());
    }

    IEnumerator UpdateCubeListCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        cubes.AddRange(GameObject.FindGameObjectsWithTag("Cube"));
        if (allGrids.Count<2)
        {
            allGrids.Add(BoxGridSystem.instance); // GridSystem'i listeye ekle
        }
        else if (allGrids[1]==null)
        {
            allGrids[1] = BoxGridSystem.instance; // GridSystem'i listeye ekle
        }
    }
}