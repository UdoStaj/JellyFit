using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeDragAndDrop : MonoBehaviour
{
    [SerializeField] private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;

    public float yTransitionSpeed = 5f;
    public float XZTransitionSpeed = 5f;
    [SerializeField] private float targetY = 1.3f;
    private float currentTargetY;
    private float originalY;
    [SerializeField] private Vector3 lastValidPosition;
    Coroutine changeYCoroutine;
    Coroutine snapCoroutine;
    [SerializeField] private LayerMask draggableLayer = -1;

    public List<Node> currentNeighbors;
    public BlockedEdges blockedEdges = new BlockedEdges();

    private void Start()
    {
        mainCamera = Camera.main;
        originalY = transform.position.y;
        lastValidPosition = transform.position;
    }

    private void Update()
    {
        HandleMouseInput();

        if (isDragging)
        {
            DragObject();
        }

        if (isDragging || Vector3.Distance(transform.position, lastValidPosition) > 0.1f)
        {
            UpdateNeighbors();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, draggableLayer))
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform)
                {
                    StartDragging();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    private void StartDragging()
    {
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

        if (IsPositionInsideGrid(transform.position))
        {
            StartSnapToGrid();
        }
        else
        {
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

            newPos.x = Mathf.Clamp(newPos.x, GridSystem.instance.transform.position.x - GridSystem.instance.horizontalLength / 2 + GridSystem.instance.nodeEdgeLength / 2,
                                   GridSystem.instance.transform.position.x + GridSystem.instance.horizontalLength / 2 - GridSystem.instance.nodeEdgeLength / 2);

            newPos.z = Mathf.Clamp(newPos.z, GridSystem.instance.transform.position.z - GridSystem.instance.verticalLength / 2 + GridSystem.instance.nodeEdgeLength / 2,
                                   GridSystem.instance.transform.position.z + GridSystem.instance.verticalLength / 2 - GridSystem.instance.nodeEdgeLength / 2);

            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        }
    }

    private bool IsPositionInsideGrid(Vector3 position)
    {
        try
        {
            return position.x >= GridSystem.instance.transform.position.x - GridSystem.instance.horizontalLength / 2 &&
                   position.x <= GridSystem.instance.transform.position.x + GridSystem.instance.horizontalLength / 2 &&
                   position.z >= GridSystem.instance.transform.position.z - GridSystem.instance.verticalLength / 2 &&
                   position.z <= GridSystem.instance.transform.position.z + GridSystem.instance.verticalLength / 2;
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
        Vector3 snapPos = new Vector3(
            Mathf.Round((transform.position.x - GridSystem.instance.transform.position.x + GridSystem.instance.horizontalLength / 2) / GridSystem.instance.nodeEdgeLength) * GridSystem.instance.nodeEdgeLength + GridSystem.instance.transform.position.x - GridSystem.instance.horizontalLength / 2,
            transform.position.y,
            Mathf.Round((transform.position.z - GridSystem.instance.transform.position.z + GridSystem.instance.verticalLength / 2) / GridSystem.instance.nodeEdgeLength) * GridSystem.instance.nodeEdgeLength + GridSystem.instance.transform.position.z - GridSystem.instance.verticalLength / 2);

        while (Vector3.Distance(transform.position, snapPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, snapPos, Time.deltaTime * XZTransitionSpeed);
            yield return null;
        }

        transform.position = snapPos;
        lastValidPosition = transform.position;
        snapCoroutine = null;
    }

    IEnumerator BackToLastPosition(Vector3 lastPos)
    {
        while (Vector3.Distance(transform.position, lastPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, lastPos, Time.deltaTime * XZTransitionSpeed);
            yield return null;
        }
        transform.position = lastPos;
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

    public void UpdateNeighbors()
    {
        currentNeighbors = GetNeighborsOfKnife(true, 2, 1);
    }

    public List<Node> GetNeighborsOfKnife(bool isHorizontal, int knifeWidthInNodes, int knifeHeightInNodes)
    {
        List<Node> neighbors = new List<Node>();

        Vector2Int startCoords = GridSystem.instance.GetCoordinatesFromPosition(transform.position);

        // Yatay (Horizontal) Yön
        if (isHorizontal)
        {
            // Yatayda bıçak genişliği kadar döngü kuruyoruz
            for (int i = 0; i < knifeWidthInNodes; i++)
            {
                Vector2Int currentCoords = new Vector2Int(startCoords.x + i, startCoords.y);

                if (currentCoords.x >= 0 && currentCoords.x < GridSystem.instance.grid.GetLength(0) &&
                    currentCoords.y >= 0 && currentCoords.y < GridSystem.instance.grid.GetLength(1))
                {
                    // Bıçak yüksekliği 1 için, sadece üst ve alt komşuları alıyoruz
                    for (int j = -1; j <= 0; j++) // Yükseklik 1 ise, sadece üst ve alt komşuları alır
                    {
                        int offsetY = currentCoords.y + j;

                        if (offsetY >= 0 && offsetY < GridSystem.instance.grid.GetLength(1))
                        {
                            if (!BlockedEdgesContains(currentCoords, new Vector2Int(currentCoords.x, offsetY)))
                            {
                                neighbors.Add(GridSystem.instance.grid[currentCoords.x, offsetY]);
                            }
                        }
                    }

                    // Bıçak boyutu 2 ise, üstte ve altta birer komşu daha ekleriz
                    /*if (knifeHeightInNodes == 2)
                    {
                        for (int j = -2; j <= 2; j++) // 2 boyutlu olduğu için bir daha genişletiyoruz
                        {
                            int offsetY = currentCoords.y + j;

                            if (offsetY >= 0 && offsetY < GridSystem.instance.grid.GetLength(1))
                            {
                                if (!BlockedEdgesContains(currentCoords, new Vector2Int(currentCoords.x, offsetY)))
                                {
                                    neighbors.Add(GridSystem.instance.grid[currentCoords.x, offsetY]);
                                }
                            }
                        }
                    }*/
                }
            }
        }
        // Dikey (Vertical) Yön
        else
        {
            // Dikeyde bıçak yüksekliği kadar döngü kuruyoruz
            for (int j = 0; j < knifeHeightInNodes; j++)
            {
                Vector2Int currentCoords = new Vector2Int(startCoords.x, startCoords.y + j);

                if (currentCoords.x >= 0 && currentCoords.x < GridSystem.instance.grid.GetLength(0) &&
                    currentCoords.y >= 0 && currentCoords.y < GridSystem.instance.grid.GetLength(1))
                {
                    // Bıçak genişliği 1 ise, sadece sağ ve sol komşuları alıyoruz
                    for (int i = -1; i <= 0; i++) // Genişlik 1 olduğu için, sadece sağ ve sol komşuları alıyoruz
                    {
                        int offsetX = currentCoords.x + i;

                        if (offsetX >= 0 && offsetX < GridSystem.instance.grid.GetLength(0))
                        {
                            if (!BlockedEdgesContains(currentCoords, new Vector2Int(offsetX, currentCoords.y)))
                            {
                                neighbors.Add(GridSystem.instance.grid[offsetX, currentCoords.y]);
                            }
                        }
                    }

                    // Bıçak boyutu 2 ise, sağda ve solda 1'er komşu daha ekleriz
                    /*if (knifeWidthInNodes == 2)
                    {
                        for (int i = -2; i <= 2; i++) // 2 boyutlu olduğu için bir daha genişletiyoruz
                        {
                            int offsetX = currentCoords.x + i;

                            if (offsetX >= 0 && offsetX < GridSystem.instance.grid.GetLength(0))
                            {
                                if (!BlockedEdgesContains(currentCoords, new Vector2Int(offsetX, currentCoords.y)))
                                {
                                    neighbors.Add(GridSystem.instance.grid[offsetX, currentCoords.y]);
                                }
                            }
                        }
                    }*/
                }
            }
        }

        return neighbors;
    }



    // Kenarların engellenip engellenmediğini kontrol eden fonksiyon
    private bool BlockedEdgesContains(Vector2Int node1, Vector2Int node2)
    {
        return blockedEdges.ContainsEdge(node1, node2);
    }
}
