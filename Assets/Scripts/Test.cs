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
    private List<Vector2Int> nodeList;
    private BlockedEdges blockedEdges;
    private Dictionary<Vector2Int, GameObject> cubeMap;

    private List<Transform> points = new List<Transform>();

    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    private void Start()
    {
        nodeList = new List<Vector2Int>();

        for (int i = 0; i < pointParent.childCount; i++)
            points.Add(pointParent.GetChild(i));

        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int coordinates = GridSystem.instance.GetCoordinatesFromPosition(points[i].position);
            nodeList.Add(coordinates);
            points[i].gameObject.SetActive(false);
        }

        cubeMap = new Dictionary<Vector2Int, GameObject>();
        GameObject dragObj = Instantiate(dragObjPrefab);
        Vector3 tempPos = Vector3.zero;

        foreach (Vector2Int cell in nodeList)
        {
            tempPos += GridSystem.instance.GetPositionFromCoordinates(cell);
        }

        dragObj.transform.position = tempPos / nodeList.Count;

        foreach (Vector2Int pos in nodeList)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = GridSystem.instance.GetPositionFromCoordinates(pos);
            cube.transform.localScale = Vector3.one * cubeSize;
            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material.color = initialColor;
            cubeMap.Add(pos, cube);

            cube.transform.parent = dragObj.transform;
        }
    }

    private void CreateBlockedEdges()
    {
        blockedEdges = new BlockedEdges();

        for (int i = 0; i < adjacencyEdges.Count; i++)
        {
            if (adjacencyEdges[i].edges.Length == 0) continue;
            Vector2Int firstCoordinates = GridSystem.instance.GetCoordinatesFromPosition(adjacencyEdges[i].edges[0].position);
            Vector2Int secondCoordinates = GridSystem.instance.GetCoordinatesFromPosition(adjacencyEdges[i].edges[1].position);
            blockedEdges.AddEdge(firstCoordinates, secondCoordinates);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateBlockedEdges();

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            List<HashSet<Vector2Int>> regions = new List<HashSet<Vector2Int>>();

            foreach (Vector2Int cell in nodeList)
            {
                if (visited.Contains(cell))
                {
                    continue;
                }

                Stack<Vector2Int> stack = new Stack<Vector2Int>();
                HashSet<Vector2Int> region = new HashSet<Vector2Int>();
                stack.Push(cell);

                while (stack.Count > 0)
                {
                    Vector2Int current = stack.Pop();
                    if (!visited.Add(current))
                    {
                        continue;
                    }

                    region.Add(current);

                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int neighbor = current + dir;
                        if (nodeList.Contains(neighbor) && blockedEdges.ContainsEdge(current, neighbor) == false)
                        {
                            stack.Push(neighbor);
                        }
                    }
                }

                regions.Add(region);
            }

            for (int i = 1; i < regions.Count; i++)
            {
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

                GameObject dragObj = Instantiate(dragObjPrefab);
                Vector3 tempPos = Vector3.zero;

                foreach (Vector2Int cell in regions[i])
                {
                    tempPos += GridSystem.instance.GetPositionFromCoordinates(cell);
                }

                dragObj.transform.position = tempPos / regions[i].Count;

                foreach (Vector2Int cell in regions[i])
                {
                    GameObject cubeObject = cubeMap[cell];
                    Renderer cubeRenderer = cubeObject.GetComponent<Renderer>();
                    cubeRenderer.material.color = color;
                    cubeObject.transform.parent = dragObj.transform;
                }
            }
        }
    }
}

public class BlockedEdges
{
    private readonly HashSet<BlockedEdge> edges = new HashSet<BlockedEdge>();

    public void AddEdge(Vector2Int firstNode, Vector2Int secondNode)
    {
        edges.Add(new BlockedEdge(firstNode, secondNode));
        edges.Add(new BlockedEdge(secondNode, firstNode));
    }

    public bool ContainsEdge(Vector2Int firstNode, Vector2Int secondNode)
    {
        foreach (BlockedEdge e in edges)
        {
            if (e.firstNode == firstNode && e.secondNode == secondNode)
            {
                return true;
            }
        }

        return false;
    }
}

public class BlockedEdge
{
    public Vector2Int firstNode;
    public Vector2Int secondNode;

    public BlockedEdge(Vector2Int firstNode, Vector2Int secondNode)
    {
        this.firstNode = firstNode;
        this.secondNode = secondNode;
    }
}

[Serializable]
public struct AdjacencyEdge
{
    public Transform[] edges;
}