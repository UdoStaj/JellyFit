using System;
using System.Collections.Generic;
using UnityEngine;

public class CubeGroupControl : MonoBehaviour
{
    public Dictionary<Vector3Int, GameObject> neighbors = new Dictionary<Vector3Int, GameObject>();
    public Dictionary<Vector3Int, bool> neighborIsAttached = new Dictionary<Vector3Int, bool>();

    private static readonly Vector3Int[] directions = {
    Vector3Int.forward,   // (0, 0, 1)
    Vector3Int.back,      // (0, 0, -1)
    Vector3Int.left,      // (-1, 0, 0)
    Vector3Int.right      // (1, 0, 0)
};

    private void Start()
    {
        
        Invoke(nameof(FindNeighbors), 0.5f);
    }

    private void FindNeighbors()
    {
        if (GridSystem.Instance == null)
        {
            Debug.LogError($"{gameObject.name}: GridSystem bulunamadı!");
            return;
        }

        GameObject[] allSoapCubes = GameObject.FindGameObjectsWithTag("SoapCube");
        GameObject[] allBoxCubes = GameObject.FindGameObjectsWithTag("BoxCube");
        
        if (allBoxCubes.Length == 0)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            List<GameObject> boxCubes = new List<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("boxCube") || obj.name.Contains("BoxCube"))
                {
                    boxCubes.Add(obj);
                }
            }
            allBoxCubes = boxCubes.ToArray();
        }

        Vector3 myPosition = transform.position;

        foreach (GameObject otherCube in allSoapCubes)
        {
            if (otherCube == gameObject) continue;

            Vector3 otherPosition = otherCube.transform.position;
            Vector3 direction = otherPosition - myPosition;
            float distance = direction.magnitude;

            float tolerance = GridSystem.Instance.cellSize * 0.3f;
            if (Mathf.Abs(distance - GridSystem.Instance.cellSize) < tolerance)
            {
                Vector3Int closestDirection = GetClosestDirection(direction.normalized);

                neighbors[closestDirection] = otherCube;
                neighborIsAttached[closestDirection] = true;
            }
        }

        foreach (GameObject otherCube in allBoxCubes)
        {
            if (otherCube == gameObject) continue;

            Vector3 otherPosition = otherCube.transform.position;
            Vector3 direction = otherPosition - myPosition;
            float distance = direction.magnitude;

            float tolerance = GridSystem.Instance.cellSize * 0.3f;
            if (Mathf.Abs(distance - GridSystem.Instance.cellSize) < tolerance)
            {
                Vector3Int closestDirection = GetClosestDirection(direction.normalized);

                neighbors[closestDirection] = otherCube;
                neighborIsAttached[closestDirection] = true;
            }
        }
    }

    private Vector3Int GetClosestDirection(Vector3 direction)
    {
        Vector3Int closestDir = Vector3Int.forward;
        float closestDot = -2f;

        foreach (Vector3Int dir in directions)
        {
            float dot = Vector3.Dot(direction.normalized, dir);
            if (dot > closestDot)
            {
                closestDot = dot;
                closestDir = dir;
            }
        }

        return closestDir;
    }

    public void CutBetween(Vector3Int direction)
    {
        if (neighborIsAttached.ContainsKey(direction))
        {
            neighborIsAttached[direction] = false;
        }

        if (neighbors.TryGetValue(direction, out GameObject neighborObj))
        {
            var neighborControl = neighborObj.GetComponent<CubeGroupControl>();
            if (neighborControl != null)
            {
                Vector3Int oppositeDirection = -direction;
                if (neighborControl.neighborIsAttached.ContainsKey(oppositeDirection))
                {
                    neighborControl.neighborIsAttached[oppositeDirection] = false;
                }
            }
        }
    }

    public void LogNeighbors()
    {
        Debug.Log($"=== {gameObject.name} Komşuluk Raporu ===");
        foreach (var direction in directions)
        {
            if (neighbors.ContainsKey(direction))
            {
                string neighborName = neighbors[direction] != null ? neighbors[direction].name : "NULL";
                bool isAttached = neighborIsAttached.ContainsKey(direction) ? neighborIsAttached[direction] : false;
                Debug.Log($"  {direction}: {neighborName} (Bağlı: {isAttached})");
            }
            else
            {
                Debug.Log($"  {direction}: Komşu yok");
            }
        }
        Debug.Log("=====================================");
    }

    [System.Serializable]
    public class NeighborDebugInfo
    {
        public string direction;
        public GameObject neighbor;
        public bool isAttached;
    }

    [SerializeField, Space(10)]
    [Header("Debug Info (Sadece görüntüleme için)")]
    private List<NeighborDebugInfo> debugNeighbors = new List<NeighborDebugInfo>();

    private void UpdateDebugInfo()
    {
        debugNeighbors.Clear();
        foreach (var direction in directions)
        {
            NeighborDebugInfo info = new NeighborDebugInfo();
            info.direction = direction.ToString();
            info.neighbor = neighbors.ContainsKey(direction) ? neighbors[direction] : null;
            info.isAttached = neighborIsAttached.ContainsKey(direction) ? neighborIsAttached[direction] : false;
            debugNeighbors.Add(info);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            UpdateDebugInfo();
        }
#endif
    }

    [ContextMenu("Find Neighbors Now")]
    public void FindNeighborsManually()
    {
        FindNeighbors();
    }
}