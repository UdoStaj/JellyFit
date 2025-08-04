using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem instance;
    [SerializeField] private float verticalLength; // X
    [SerializeField] private float horizontalLength; // Y

    [SerializeField] private float nodeEdgeLength;

    [SerializeField] private GameObject testPrefab;

    private Vector3 upperLeftNodePosition;

    private Node[,] grid;

    private void Awake()
    {
        instance = this;
        CreateGrid();
    }

    public void CreateGrid()
    {
        int x = Mathf.FloorToInt(verticalLength / nodeEdgeLength); // x eksenindeki boyut
        int y = Mathf.FloorToInt(horizontalLength / nodeEdgeLength); // y eksenindeki boyut

        grid = new Node[x, y];

        Vector3 nodePosition = transform.position +
                               new Vector3(-horizontalLength / 2 + nodeEdgeLength / 2, 0, verticalLength / 2 - nodeEdgeLength / 2);

        upperLeftNodePosition = nodePosition;

        for (int i = 0; i < x; i++) //x in verisini tutuyor
        {
            for (int j = 0; j < y; j++) //y in verisini tutuyor
            {
                Node tempNode = new Node(nodePosition, i, j);

                grid[i, j] = tempNode;

                nodePosition.x += nodeEdgeLength;
            }

            nodePosition.z -= nodeEdgeLength;
            nodePosition.x = upperLeftNodePosition.x;
        }
    }

    public Node GetNodeFromPosition(Vector3 position)
    {
        float differenceX = Mathf.Abs(position.z - upperLeftNodePosition.z);
        float differenceY = Mathf.Abs(position.x - upperLeftNodePosition.x);

        int x = Mathf.RoundToInt(differenceX / nodeEdgeLength);
        int y = Mathf.RoundToInt(differenceY / nodeEdgeLength);

        return grid[x, y];
    }

    public Vector3 GetPositionFromCoordinates(Vector2Int coordinates)
    {
        return grid[coordinates.x, coordinates.y].GetPosition();
    }

    public Vector2Int GetCoordinatesFromPosition(Vector3 position) => GetNodeFromPosition(position).coordinates;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(horizontalLength, 1, verticalLength));
    }
}

public class Node
{
    private Vector3 position;
    public Vector2Int coordinates;

    public Node(Vector3 position, int x, int y)
    {
        SetPosition(position, x, y);
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector3 position, int x, int y)
    {
        this.position = position;
        coordinates = new Vector2Int(x, y);
    }
}