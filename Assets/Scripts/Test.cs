using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    [SerializeField] private Transform pointParent;
    [SerializeField] private GameObject dragObjPrefab;
    [SerializeField] private List<AdjacencyEdge> adjacencyEdges;
    public float cubeSize = 0.99f;
    public Color initialColor = Color.red;

    private BlockedEdges blockedEdges;
    private Dictionary<Vector2Int, GameObject> cubeMap;
    private List<Vector2Int> nodeList;

    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    private void Start()
    {
        cubeMap = new Dictionary<Vector2Int, GameObject>();
        nodeList = new List<Vector2Int>();

        // Grid üzerindeki başlangıç noktalarını oku
        List<Transform> points = new List<Transform>();
        for (int i = 0; i < pointParent.childCount; i++)
            points.Add(pointParent.GetChild(i));

        foreach (Transform point in points)
        {
            Vector2Int coord = GridSystem.instance.GetCoordinatesFromPosition(point.position);
            if (!cubeMap.ContainsKey(coord))
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = GridSystem.instance.GetPositionFromCoordinates(coord);
                cube.transform.localScale = Vector3.one * cubeSize;
                cube.GetComponent<Renderer>().material.color = initialColor;
                cube.layer = LayerMask.NameToLayer("draggableLayer");
                cube.AddComponent<CubeData>().gridCoord = coord;

                cubeMap.Add(coord, cube);
                nodeList.Add(coord);
            }

            point.gameObject.SetActive(false);
        }

        // İlk grubu tek bir dragObj altına al
        CreateInitialDragObject();
    }

    private void CreateInitialDragObject()
    {
        GameObject dragObj = Instantiate(dragObjPrefab, transform);
        Vector3 avgPos = Vector3.zero;

        foreach (var coord in nodeList)
            avgPos += GridSystem.instance.GetPositionFromCoordinates(coord);

        dragObj.transform.position = avgPos / nodeList.Count;

        foreach (var coord in nodeList)
        {
            GameObject cube = cubeMap[coord];
            cube.transform.parent = dragObj.transform;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 1. adjacencyEdges listesini temizle
            adjacencyEdges.Clear();

            // 2. Sahnedeki tüm bıçakları bul
            KnifeDragAndDrop[] knives = FindObjectsOfType<KnifeDragAndDrop>();

            foreach (var knife in knives)
            {
                var neighbors = knife.currentNeighbors;

                // Yalnızca yatay olanlar için komşu ilişkisi oluştur
                if (knife.isHorizontal) // Bu bıçak yataysa
                {
                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        for (int j = i + 1; j < neighbors.Count; j++)
                        {
                            Vector2Int a = neighbors[i].coordinates;
                            Vector2Int b = neighbors[j].coordinates;

                            // Eğer bıçak yataysa, sadece Y koordinatları sabit olanları komşu yap
                            if (a.y == b.y)
                            {
                                AdjacencyEdge edge = new AdjacencyEdge
                                {
                                    edges = new Vector2Int[] { a, b }
                                };
                                adjacencyEdges.Add(edge);
                            }
                        }
                    }
                }
            }

            RecalculateRegions(); // Varsa
        }
    }



    private void RecalculateRegions()
    {
        RefreshCubeStates();
        CreateBlockedEdges();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<HashSet<Vector2Int>> regions = new List<HashSet<Vector2Int>>();

        foreach (var coord in nodeList)
        {
            if (visited.Contains(coord)) continue;

            HashSet<Vector2Int> region = new HashSet<Vector2Int>();
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(coord);

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Pop();
                if (!visited.Add(current)) continue;

                region.Add(current);

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = current + dir;
                    if (nodeList.Contains(neighbor) && !blockedEdges.ContainsEdge(current, neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }

            regions.Add(region);
        }

        // Yeni dragObj'ler oluştur
        foreach (var region in regions)
        {
            GameObject dragObj = Instantiate(dragObjPrefab, transform);
            Color color = new Color(Random.value, Random.value, Random.value);
            Vector3 centerPos = Vector3.zero;

            foreach (var coord in region)
                centerPos += GridSystem.instance.GetPositionFromCoordinates(coord);

            dragObj.transform.position = centerPos / region.Count;

            foreach (var coord in region)
            {
                if (cubeMap.TryGetValue(coord, out GameObject cube))
                {
                    cube.transform.parent = dragObj.transform;
                    cube.GetComponent<Renderer>().material.color = color;
                    
                }
            }
        }
    }

    private void RefreshCubeStates()
    {
        // Küpleri serbest bırak ve verileri güncelle
        nodeList.Clear();
        cubeMap.Clear();

        CubeData[] allCubes = FindObjectsOfType<CubeData>();
        foreach (CubeData cubeData in allCubes)
        {
            GameObject cube = cubeData.gameObject;
            Vector2Int coord = GridSystem.instance.GetCoordinatesFromPosition(cube.transform.position);

            cubeData.gridCoord = coord; // güncelle
            cube.transform.parent = null;
            cube.GetComponent<Renderer>().material.color = initialColor;

            if (!cubeMap.ContainsKey(coord))
            {
                cubeMap.Add(coord, cube);
                nodeList.Add(coord);
            }
        }

        // Eski dragObj'leri temizle
        foreach (Transform child in transform)
        {
            if (child.name.Contains(dragObjPrefab.name))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreateBlockedEdges()
    {
        blockedEdges = new BlockedEdges();

        foreach (var adj in adjacencyEdges)
        {
            if (adj.edges.Length < 2) continue;

            Vector2Int a = adj.edges[0];
            Vector2Int b = adj.edges[1];

            blockedEdges.AddEdge(a, b);
        }
    }
}

[Serializable]
public class BlockedEdges
{
    private readonly HashSet<BlockedEdge> edges = new HashSet<BlockedEdge>();

    public void AddEdge(Vector2Int a, Vector2Int b)
    {
        edges.Add(new BlockedEdge(a, b));
        edges.Add(new BlockedEdge(b, a));
    }

    public bool ContainsEdge(Vector2Int a, Vector2Int b)
    {
        return edges.Contains(new BlockedEdge(a, b));
    }
}

[Serializable]
public class BlockedEdge : IEquatable<BlockedEdge>
{
    public Vector2Int firstNode;
    public Vector2Int secondNode;

    public BlockedEdge(Vector2Int firstNode, Vector2Int secondNode)
    {
        this.firstNode = firstNode;
        this.secondNode = secondNode;
    }

    public bool Equals(BlockedEdge other)
    {
        return firstNode == other.firstNode && secondNode == other.secondNode;
    }

    public override int GetHashCode()
    {
        return firstNode.GetHashCode() ^ secondNode.GetHashCode();
    }
}

[Serializable]
public struct AdjacencyEdge
{
    public Vector2Int[] edges;
}

public class CubeData : MonoBehaviour
{
    public Vector2Int gridCoord;
}
