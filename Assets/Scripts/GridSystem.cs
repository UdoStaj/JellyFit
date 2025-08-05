using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class LevelData
{
    public List<Vector2Int> boxCells;
    public List<Vector2Int> soapCells;
    public List<Vector2Int> obstacleCells;
    public int gridWidth;
    public int gridHeight;
    public float cellSize;
}

public class GridSystem : MonoBehaviour
{
    public GameObject targetObject;

    [Min(1)] public int gridWidth = 5;
    [Min(1)] public int gridHeight = 5;
    [Min(0.1f)] public float cellSize = 1f;

    public List<Vector2Int> boxCells = new List<Vector2Int>();
    public List<Vector2Int> soapCells = new List<Vector2Int>();
    public List<Vector2Int> obstacleCells = new List<Vector2Int>();

    public GameObject boxPrefab;
    public GameObject soapPrefab;
    public GameObject obstaclePrefab;
    public GameObject highlightPrefab;
    public GameObject soaps;

    [HideInInspector] public List<Vector2Int> markedCells = new List<Vector2Int>();

    public Vector3 gridOrigin;
    private bool[,] gridOccupied;

    private float autoSaveTimer = 0f;
    public float autoSaveInterval = 10f; // Her 10 saniyede otomatik kaydet

    public static GridSystem Instance;
    public SoapBox MyBox;
    private void Awake()
    {
        Instance = this;
    }
    public void InitializeGrid()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned!");
            return;
        }

        Renderer rend = targetObject.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Target object has no Renderer!");
            return;
        }

        Bounds bounds = rend.bounds;

        float usableWidth = gridWidth * cellSize;
        float usableHeight = gridHeight * cellSize;

        if (usableWidth > bounds.size.x || usableHeight > bounds.size.z)
        {
            Debug.LogWarning("Grid is larger than the target object!");
        }

        gridOrigin = new Vector3(
            bounds.center.x - usableWidth / 2f,
            bounds.max.y,
            bounds.center.z - usableHeight / 2f
        );

        gridOccupied = new bool[gridWidth, gridHeight];
    }

    public Vector2Int GetGridIndexFromWorld(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GetCellCenter(Vector2Int index)
    {
        return new Vector3(
            gridOrigin.x + index.x * cellSize + cellSize / 2f,
            gridOrigin.y,
            gridOrigin.z + index.y * cellSize + cellSize / 2f
        );
    }

    public GameObject GetCellObject(Vector2Int index)
    {

        if (!IsValidCell(index)) return null;

        Debug.Log("is valid cell true");
        Vector3 cellCenter = GetCellCenter(index);
        Collider[] colliders = Physics.OverlapBox(cellCenter, new Vector3(cellSize / 2f, cellSize / 2f, cellSize / 2f));

        foreach (var collider in colliders)
        {
            if (collider.gameObject != targetObject)
            {
                return collider.gameObject;
            }
        }

        return null;
    }


    public bool IsValidCell(Vector2Int index)
    {
        return index.x >= 0 && index.x < gridWidth && index.y >= 0 && index.y < gridHeight;
    }
    public bool IsValidCellCut(Vector2Int index)
    {
        return index.x >= 0 && index.x <= gridWidth && index.y >= 0 && index.y <= gridHeight;
    }

    public bool IsCellOccupied(Vector2Int index)
    {
        if (!IsValidCell(index)) return false;
        return gridOccupied[index.x, index.y];
    }

    public bool IsCellOccupiedCut(Vector2Int index)
    {
        if (!IsValidCellCut(index)) return false;
        return gridOccupied[index.x, index.y];
    }

    public void MarkCellOccupied(Vector2Int index)
    {
        if (IsValidCell(index))
        {
            gridOccupied[index.x, index.y] = true;
        }
    }

    public void MarkCellEmpty(Vector2Int index)
    {
        if (IsValidCell(index))
        {
            gridOccupied[index.x, index.y] = false;
        }
    }

    private void Start()
    {
        InitializeGrid();
        PlaceMarkedCubes();
    }


    public void PlaceMarkedCubes()
    {
        // Box cubes
        foreach (var index in boxCells)
        {
            if (IsValidCell(index) && !IsCellOccupied(index))
            {
                Vector3 pos = GetCellCenter(index);
                MyBox.Add(GetGridIndexFromWorld(pos));  
            }
        }
        // Soap cubes
        foreach (var index in soapCells)
        {
            if (IsValidCell(index) && !IsCellOccupied(index))
            {
                Vector3 pos = GetCellCenter(index);
                if (soapPrefab != null)
                {
                    Instantiate(soapPrefab, new Vector3(pos.x, pos.y + soapPrefab.transform.localScale.y / 2f, pos.z), Quaternion.identity,soaps.transform);
                    MarkCellOccupied(index);
                }
            }
        }

        // Obstacle cubes
        foreach (var index in obstacleCells)
        {
            if (IsValidCell(index) && !IsCellOccupied(index))
            {
                Vector3 pos = GetCellCenter(index);
                if (obstaclePrefab != null)
                {
                    Instantiate(obstaclePrefab, new Vector3(pos.x, pos.y + obstaclePrefab.transform.localScale.y / 2f, pos.z), Quaternion.identity);
                    MarkCellOccupied(index);
                }
            }
        }
    }
    /*public void SaveLevel(string levelName)
    {
        LevelData levelData = new LevelData()
        {
            boxCells = new List<Vector2Int>(boxCells),
            soapCells = new List<Vector2Int>(soapCells),
            obstacleCells = new List<Vector2Int>(obstacleCells),
            gridWidth = gridWidth,
            gridHeight = gridHeight,
            cellSize = cellSize
        };

        string json = JsonUtility.ToJson(levelData, true);
        string dir = Path.Combine(Application.dataPath, "Ceyhun/Levels");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string filePath = Path.Combine(dir, levelName + ".json");
        File.WriteAllText(filePath, json);

#if UNITY_EDITOR
        Debug.Log("Level saved at: " + filePath);
        AssetDatabase.Refresh();
#endif
    }*/


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            InitializeGrid();
        }

        Gizmos.color = Color.gray;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int index = new Vector2Int(x, y);
                Vector3 center = GetCellCenter(index);
                Vector3 size = new Vector3(cellSize, 0.01f, cellSize);
                Gizmos.DrawWireCube(center, size);

                if (boxCells.Contains(index))
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(center + Vector3.up * 0.01f, size * 0.8f);
                    Gizmos.color = Color.gray;
                }
                else if (soapCells.Contains(index))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(center + Vector3.up * 0.01f, size * 0.8f);
                    Gizmos.color = Color.gray;
                }
                else if (obstacleCells.Contains(index))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(center + Vector3.up * 0.01f, size * 0.8f);
                    Gizmos.color = Color.gray;
                }
            }
        }
    }
#endif
}