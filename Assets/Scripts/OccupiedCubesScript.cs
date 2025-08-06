using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupiedCubesScript : DragAndDrop
{
    public BoxGridSystem boxGrid;
    public override void Start()
    {
        StartBoxSnapToGrid();
        UpdateOccupiedNodes();
    }
    public override void Update()
    {}

    public override void UpdateOccupiedNodes()
    {
        if (boxGrid == null) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (IsPositionInsideGrid())
            {
                Node childNode = boxGrid.GetNodeFromPosition(child.position);
                childNode.SetEmpty(false);
            }
        }
    }
    public override bool IsPositionInsideGrid()
    {
        if (boxGrid == null) return false;

        try
        {
            int maxX = Mathf.FloorToInt(boxGrid.verticalLength / boxGrid.nodeEdgeLength);
            int maxY = Mathf.FloorToInt(boxGrid.horizontalLength / boxGrid.nodeEdgeLength);

            // Tüm child'larýn grid içinde olup olmadýðýný kontrol et
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Vector2Int coordinates = boxGrid.GetCoordinatesFromPosition(child.position);

                if (coordinates.x < 0 || coordinates.x >= maxX || coordinates.y < 0 || coordinates.y >= maxY)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
    public void StartBoxSnapToGrid()
    {
        StartCoroutine(BarrierSnapToGrid());
    }
    public IEnumerator BarrierSnapToGrid()
    {
        if (boxGrid == null) yield break;

        Transform firstChild = transform.GetChild(0);
        Vector3 firstChildSnapPos = boxGrid.GetPositionFromCoordinates(
                       boxGrid.GetCoordinatesFromPosition(firstChild.position));

        // Parent'ýn ne kadar hareket etmesi gerektiðini hesapla
        Vector3 offset = firstChildSnapPos - firstChild.position;
        Vector3 targetParentPos = transform.position + offset;
        targetParentPos.y = transform.position.y;

        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                            new Vector3(targetParentPos.x, 0, targetParentPos.z)) > 0.05f)
        {
            Vector3 newPos = Vector3.Lerp(transform.position, targetParentPos, Time.deltaTime * XZTransitionSpeed);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
            yield return null;
        }
        transform.position = new Vector3(targetParentPos.x, transform.position.y, targetParentPos.z);

    }
}
