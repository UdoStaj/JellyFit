using System.Collections.Generic;
using UnityEngine;

public class SoapCube : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionDistance = 1.1f;
    public LayerMask soapLayerMask = -1;

    [Header("Debug")]
    public bool showConnections = true;
    public bool showGroupColors = true;

    private List<SoapCube> connectedCubes = new List<SoapCube>();
    private SoapGroup currentGroup;
    private GridSystem gridSystem;
    private Vector2Int gridPosition;
    private Renderer cubeRenderer;
    private Color originalColor;

    public List<SoapCube> ConnectedCubes => connectedCubes;
    public SoapGroup Group => currentGroup;
    public Vector2Int GridPosition => gridPosition;

    private void Awake()
    {
        gridSystem = FindObjectOfType<GridSystem>();
        if (gridSystem == null)
        {
            Debug.LogError("GridSystem not found in scene!");
        }

        // Renderer ve orijinal rengi kaydet
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            originalColor = cubeRenderer.material.color;
        }
    }

    private void Start()
    {
        // Grid pozisyonunu hesapla
        if (gridSystem != null)
        {
            gridPosition = gridSystem.GetGridIndexFromWorld(transform.position);
        }

        // Baðlantýlarý bul ve grubu oluþtur
        FindConnections();
        SoapGroupManager.Instance?.RegisterCube(this);
    }

    public void FindConnections()
    {
        // Mevcut baðlantýlarý temizle
        List<SoapCube> oldConnections = new List<SoapCube>(connectedCubes);
        connectedCubes.Clear();

        // Eski baðlantýlardaki karþýlýklý referanslarý temizle
        foreach (var oldConnection in oldConnections)
        {
            if (oldConnection != null)
            {
                oldConnection.connectedCubes.Remove(this);
            }
        }

        if (gridSystem == null) return;

        // 4 yön kontrol et (sað, sol, yukarý, aþaðý - grid bazýnda)
        Vector2Int[] directions = {
            Vector2Int.right,   // sað
            Vector2Int.left,    // sol
            Vector2Int.up,      // yukarý (grid'de)
            Vector2Int.down     // aþaðý (grid'de)
        };

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = gridPosition + direction;

            if (gridSystem.IsValidCell(neighborPos))
            {
                Vector3 neighborWorldPos = gridSystem.GetCellCenter(neighborPos);

                // Bu pozisyonda SoapCube var mý kontrol et
                Collider[] colliders = Physics.OverlapSphere(neighborWorldPos, 0.3f, soapLayerMask);

                foreach (var collider in colliders)
                {
                    SoapCube neighbor = collider.GetComponent<SoapCube>();
                    if (neighbor != null && neighbor != this && !connectedCubes.Contains(neighbor))
                    {
                        // Karþýlýklý baðlantý kur
                        AddConnection(neighbor);
                        neighbor.AddConnectionInternal(this);
                    }
                }
            }
        }

        Debug.Log($"{name} - {connectedCubes.Count} baðlantý bulundu");
    }

    public void AddConnection(SoapCube other)
    {
        if (other != null && !connectedCubes.Contains(other) && other != this)
        {
            connectedCubes.Add(other);
            Debug.Log($"Baðlantý eklendi: {name} <-> {other.name}");
        }
    }

    // Ýç kullaným için - sonsuz döngüyü önler
    private void AddConnectionInternal(SoapCube other)
    {
        if (other != null && !connectedCubes.Contains(other) && other != this)
        {
            connectedCubes.Add(other);
        }
    }

    public void RemoveConnection(SoapCube other)
    {
        if (connectedCubes.Contains(other))
        {
            connectedCubes.Remove(other);
            Debug.Log($"Baðlantý kaldýrýldý: {name} <-> {other.name}");
        }
    }

    public void SetGroup(SoapGroup group)
    {
        currentGroup = group;

        // Grup rengini uygula (debug için)
        if (showGroupColors && cubeRenderer != null)
        {
            if (group != null)
            {
                cubeRenderer.material.color = group.groupColor;
            }
            else
            {
                cubeRenderer.material.color = originalColor;
            }
        }
    }

    public void UpdateGridPosition()
    {
        if (gridSystem != null)
        {
            Vector2Int newGridPos = gridSystem.GetGridIndexFromWorld(transform.position);
            if (newGridPos != gridPosition)
            {
                gridPosition = newGridPos;

                // Pozisyon deðiþtiðinde baðlantýlarý yeniden kontrol et
                Invoke(nameof(FindConnections), 0.1f);
            }
        }
    }

    // Bu küpün hangi gruba ait olduðunu kontrol et
    public bool IsConnectedTo(SoapCube other)
    {
        if (other == null || other == this) return false;

        // Direkt baðlantý var mý?
        if (connectedCubes.Contains(other)) return true;

        // Ayný grupta mý?
        return currentGroup != null && currentGroup == other.currentGroup;
    }

    // Bu küpten baþlayarak baðlý tüm küpleri bul (flood fill)
    public HashSet<SoapCube> GetAllConnectedCubes()
    {
        HashSet<SoapCube> visited = new HashSet<SoapCube>();
        Queue<SoapCube> queue = new Queue<SoapCube>();

        queue.Enqueue(this);
        visited.Add(this);

        while (queue.Count > 0)
        {
            SoapCube current = queue.Dequeue();

            foreach (var neighbor in current.connectedCubes)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }

    // Baðlantý sayýsýný döndür
    public int GetConnectionCount()
    {
        return connectedCubes.Count;
    }

    // Belirli bir yöndeki baðlantýyý al
    public SoapCube GetConnectionInDirection(Vector2Int direction)
    {
        Vector2Int targetPos = gridPosition + direction;

        foreach (var connected in connectedCubes)
        {
            if (connected != null && connected.gridPosition == targetPos)
            {
                return connected;
            }
        }

        return null;
    }

    // Baðlantýlarý görselleþtir
    private void OnDrawGizmos()
    {
        if (!showConnections) return;

        // Baðlantý çizgileri
        Gizmos.color = currentGroup != null ? currentGroup.groupColor : Color.yellow;
        foreach (var connected in connectedCubes)
        {
            if (connected != null)
            {
                Gizmos.DrawLine(transform.position, connected.transform.position);

                // Baðlantý noktalarýný göster
                Vector3 midPoint = (transform.position + connected.transform.position) * 0.5f;
                Gizmos.DrawWireSphere(midPoint, 0.1f);
            }
        }

        // Bu küpün pozisyonunu vurgula
        if (currentGroup != null)
        {
            Gizmos.color = currentGroup.groupColor;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.05f);
        }
    }

    private void OnDestroy()
    {
        // Bu küp yok edilirken baðlantýlarý temizle
        foreach (var connected in connectedCubes)
        {
            if (connected != null)
            {
                connected.RemoveConnection(this);
            }
        }

        SoapGroupManager.Instance?.UnregisterCube(this);
    }

    // Debug bilgileri
    [System.Serializable]
    public class DebugInfo
    {
        public int connectionCount;
        public string groupName;
        public Vector2Int gridPos;
        public List<string> connectedCubeNames;
    }

    public DebugInfo GetDebugInfo()
    {
        var info = new DebugInfo();
        info.connectionCount = connectedCubes.Count;
        info.groupName = currentGroup != null ? currentGroup.name : "None";
        info.gridPos = gridPosition;
        info.connectedCubeNames = new List<string>();

        foreach (var cube in connectedCubes)
        {
            if (cube != null)
            {
                info.connectedCubeNames.Add(cube.name);
            }
        }

        return info;
    }
}