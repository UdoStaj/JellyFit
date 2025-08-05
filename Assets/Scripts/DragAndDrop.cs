using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField]private bool isDragging = false;
    private bool isFirstFrame=true;
    private Vector3 offset;
    private Camera mainCamera;

    public float yTransitionSpeed = 5f; // Speed of Y position transition
    public float XZTransitionSpeed = 5f;
    [SerializeField]private float targetY=1.3f;
    private float currentTargetY;
    private float originalY;
    [SerializeField]private Vector3 lastValidPosition;
    Coroutine changeYCoroutine;
    Coroutine snapCoroutine;
    [SerializeField] private LayerMask draggableLayer = -1;

    private List<Node> previouslyOccupiedNodes = new List<Node>();
    private GridSystem currentGrid; // Hangi grid'de olduðumuzu takip eder
    private GridSystem originalGrid; // Baþlangýçta hangi grid'de olduðumuzu hatýrlar
    
    private void Start()
    {
        mainCamera = Camera.main;
        originalY = transform.position.y;
        
        // Baþlangýçta hangi grid'de olduðumuzu bul
        FindCurrentGrid();
        originalGrid = currentGrid;
        UpdateOccupiedNodes();
    }

    private void Update()
    {
        HandleMouseInput();

        if (isDragging)
        {
            DragObject();
        }
    }

    // Objenin þu anda hangi grid'de olduðunu bulur
    private void FindCurrentGrid()
    {
        GridSystem[] allGrids = FindObjectsOfType<GridSystem>();
        float closestDistance = float.MaxValue;
        GridSystem closestGrid = null;

        foreach (GridSystem grid in allGrids)
        {
            // Grid'in merkez pozisyonunu hesapla
            Vector3 gridCenter = grid.transform.position;
            float distance = Vector3.Distance(transform.position, gridCenter);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestGrid = grid;
            }
        }

        currentGrid = closestGrid;
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isFirstFrame)
            {
                lastValidPosition = transform.position;
                isFirstFrame = false;
            }
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, draggableLayer))
            {
                // Eðer bu obje veya child objelerinden biri týklandýysa
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform)
                {
                    StartDragging();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
            isFirstFrame = true;
        }
    }

    private void StartDragging()
    {
        ClearPreviouslyOccupiedNodes();
        currentTargetY = targetY;
        if (changeYCoroutine != null)
        {
            StopCoroutine(changeYCoroutine);
        }
        changeYCoroutine = StartCoroutine(ChangeYPosAtDragging(currentTargetY));

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, originalY, 0));
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 currentPosOnPlane = new Vector3(transform.position.x, originalY, transform.position.z);
            offset = currentPosOnPlane - hitPoint;
        }
        else
        {
            offset = Vector3.zero;
        }
        isDragging = true;
    }

    private void StopDragging()
    {
        currentTargetY = originalY;
        if (changeYCoroutine != null)
            StopCoroutine(changeYCoroutine);
        changeYCoroutine = StartCoroutine(ChangeYPosAtDragging(currentTargetY));
        isDragging = false;

        // Drop sýrasýnda hangi grid'e en yakýn olduðumuzu kontrol et
        FindCurrentGrid();

        if (IsPositionInsideGrid())
        {
            // Her child küp için node'u iþaretle
            bool canPlace = true;
            List<Node> nodesToOccupy = new List<Node>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Node childNode = currentGrid.GetNodeFromPosition(child.position);

                if (!childNode.IsEmpty())
                {
                    canPlace = false;
                    break;
                }
                nodesToOccupy.Add(childNode);
            }

            if (canPlace)
            {
                // Tüm node'larý iþgal et
                foreach (Node node in nodesToOccupy)
                {
                    node.SetEmpty(false);
                }
                previouslyOccupiedNodes = nodesToOccupy;
                StartSnapToGrid();
            }
            else
            {
                RestorePreviouslyOccupiedNodes();
                StartBackToLastPosition();
            }
        }
        else
        {
            RestorePreviouslyOccupiedNodes();
            StartBackToLastPosition();
        }
    }

    private void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, originalY, 0));

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 newPos = hitPoint + offset;
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        }
    }
    
    private bool IsPositionInsideGrid()
    {
        if (currentGrid == null) return false;
        
        try
        {
            int maxX = Mathf.FloorToInt(currentGrid.verticalLength / currentGrid.nodeEdgeLength);
            int maxY = Mathf.FloorToInt(currentGrid.horizontalLength / currentGrid.nodeEdgeLength);

            // Tüm child'larýn grid içinde olup olmadýðýný kontrol et
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Vector2Int coordinates = currentGrid.GetCoordinatesFromPosition(child.position);

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
    
    public void StartSnapToGrid()
    {
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SnapToGrid());

    }
    
    public void StartBackToLastPosition()
    {
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(BackToLastPosition(lastValidPosition));
    }
    
    public IEnumerator SnapToGrid()
    {
        if (currentGrid == null) yield break;
        
        Transform firstChild = transform.GetChild(0);
        Vector3 firstChildSnapPos = currentGrid.GetPositionFromCoordinates(
                       currentGrid.GetCoordinatesFromPosition(firstChild.position));

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
        if(currentGrid.AreAllNodesFull())
        {
            Debug.Log("All nodes are full.");
            //burada tüm node'lar doluysa yapýlacak iþlemler(level complete)
        }

        snapCoroutine = null;
    }

    IEnumerator BackToLastPosition(Vector3 lastPos)
    {
        // Geri dönerken orijinal grid'i kullan
        currentGrid = originalGrid;
        
        while (Vector3.Distance(transform.position, lastPos) > 0.05f)
        {
            Vector3 newPos = Vector3.Lerp(transform.position, lastPos, Time.deltaTime * XZTransitionSpeed);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
            yield return null;
        }
        transform.position = new Vector3(lastPos.x, transform.position.y, lastPos.z); // Tam hizalama
    }
    
    IEnumerator ChangeYPosAtDragging(float targetYPos)
    {
        while (true)
        {
            if (Mathf.Abs(transform.position.y - targetYPos) > 0.05f)
            {
                float newY = Mathf.Lerp(transform.position.y, targetYPos, Time.deltaTime * yTransitionSpeed);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                
            }
            else
            {
                transform.position = new Vector3(transform.position.x, targetYPos, transform.position.z);
                break;
            }
            yield return null;

        }
        changeYCoroutine = null;
    }

    private void UpdateOccupiedNodes()
    {
        if (currentGrid == null) return;
        
        previouslyOccupiedNodes.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (IsPositionInsideGrid())
            {
                Node childNode = currentGrid.GetNodeFromPosition(child.position);
                previouslyOccupiedNodes.Add(childNode);
                childNode.SetEmpty(false);
            }
        }
    }

    private void ClearPreviouslyOccupiedNodes()
    {
        foreach (Node node in previouslyOccupiedNodes)
        {
            node.SetEmpty(true);
        }
    }

    private void RestorePreviouslyOccupiedNodes()
    {
        foreach (Node node in previouslyOccupiedNodes)
        {
            node.SetEmpty(false);
        }
    }
}