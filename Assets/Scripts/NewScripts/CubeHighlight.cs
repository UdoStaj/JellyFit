using UnityEditor;
using UnityEngine;

public class CubeHighlight : MonoBehaviour
{
    public GameObject highLightPrefab;
    GameObject cubeHighlightContainer;
    GameObject highlight;
    GridSystem grid;

    private void Start()
    {
        cubeHighlightContainer = GameObject.Find("CubeHighlightContainer");
        grid = FindObjectOfType<GridSystem>();
        if (grid == null)
        {
            Debug.LogError("CubeHighlight: GridSystem bulunamadý.");
            enabled = false;
            return;
        }
        highlight = Instantiate(highLightPrefab, transform.position,Quaternion.identity,cubeHighlightContainer.transform);
    }

    private void Update()
    {
        CalculateGridField();
    }
    void CalculateGridField()
    {
        Vector3 pos = transform.position;
        Vector2Int gridIndex = grid.GetGridIndexFromWorld(pos);

        if (grid.IsValidCell(gridIndex))
        {
            Vector3 cellCenter = grid.GetCellCenter(gridIndex);
            highlight.SetActive(true);
            highlight.transform.position = new Vector3(cellCenter.x, 0.193f, cellCenter.z);
            highlight.transform.localScale = new Vector3(grid.cellSize, 0.01f, grid.cellSize);
        }
        else
        {
            highlight.SetActive(false);
        }
    }
}
