using System.Collections;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField]private bool isDragging = false;
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
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        }
    }
    private bool IsPositionInsideGrid(Vector3 position)
    {
        try
        {
            // Ýlk child cube'ý referans alarak kontrol et
            Transform firstChild = transform.GetChild(0);
            Vector3 firstChildSnapPos = GridSystem.instance.GetPositionFromCoordinates(
                           GridSystem.instance.GetCoordinatesFromPosition(firstChild.position));
            Vector3 offset = firstChildSnapPos - firstChild.position;

            int maxX = Mathf.FloorToInt(GridSystem.instance.verticalLength / GridSystem.instance.nodeEdgeLength);
            int maxY = Mathf.FloorToInt(GridSystem.instance.horizontalLength / GridSystem.instance.nodeEdgeLength);

            // Tüm child'larýn grid içinde olup olmadýðýný kontrol et
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Vector3 projectedPos = child.position + offset;
                Vector2Int coordinates = GridSystem.instance.GetCoordinatesFromPosition(projectedPos);

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
        Transform firstChild = transform.GetChild(0);
        Vector3 firstChildSnapPos = GridSystem.instance.GetPositionFromCoordinates(
                       GridSystem.instance.GetCoordinatesFromPosition(firstChild.position));

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

        lastValidPosition = transform.position;
        snapCoroutine = null;
    }

    IEnumerator BackToLastPosition(Vector3 lastPos)
    {
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
}
