using System.Collections;
using UnityEngine;

public class SliceManager : MonoBehaviour
{
    private Camera mainCamera;
    private Animator animator;
    private NewCutSystem newCutSystem;

    public GameObject knife;
    public GameObject cursorPrefab;

    private GameObject cursorInstance;

    private bool isDragging = false;
    private float cellSize;
    private Vector3 gridOrigin;

    public int lineCount = 1;
        
    private Vector3 dragOffset;
    private Vector3 previousKnifePosition;

    

    void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        newCutSystem = FindAnyObjectByType<NewCutSystem>();
        if (newCutSystem == null)
        {
            Debug.LogError("SliceManager: NewCutSystem bulunamadı! Lütfen sahneye NewCutSystem scripti ekleyin.");
            enabled = false;
            return;
        }

        var gridSystem = Object.FindFirstObjectByType<GridSystem>();
        if (gridSystem == null)
        {
            Debug.LogError("SliceManager: GridSystem bulunamadı!");
            enabled = false;
            return;
        }

        gridOrigin = gridSystem.gridOrigin;
        cellSize = gridSystem.cellSize;

        if (knife == null)
        {
            knife = GameObject.FindGameObjectWithTag("Knife");
            
            if (knife == null)
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains("Bicak") || obj.name.Contains("Knife"))
                    {
                        knife = obj;
                        break;
                    }
                }
            }
            
            if (knife == null)
            {
                Debug.LogError("SliceManager: Knife bulunamadı! Lütfen bıçak GameObject'ine 'Knife' tag'i ekleyin veya Inspector'da atayın.");
                enabled = false;
                return;
            }
        }

        if (cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab);
            cursorInstance.SetActive(false);
        }
        previousKnifePosition = knife.transform.position;
    }

    void Update()
    {
        if (isDragging)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            DragWithMouse();
#else
            DragWithTouch();
#endif
        }
    }

    public float SnapToGridLine(float worldCoord, float origin, float cellSize)
    {
        return origin + Mathf.Round((worldCoord - origin) / cellSize) * cellSize;
    }

    void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            dragOffset = knife.transform.position - worldPos;
            isDragging = true;

            if (cursorInstance != null)
                cursorInstance.SetActive(true);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (cursorInstance != null)
            cursorInstance.SetActive(false);
    }

    void DragWithMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            float snappedX;
            float snappedZ;
            bool isVertical = Mathf.Abs(Vector3.Dot(knife.transform.forward, Vector3.forward)) > 0.8f;

            if (isVertical)
            {
                snappedX = SnapToGridLine(worldPos.x , gridOrigin.x , cellSize);
                snappedZ = SnapToGridLine(worldPos.z, gridOrigin.z + cellSize / 2f, cellSize);
            }
            else
            {
                snappedX = SnapToGridLine(worldPos.x, gridOrigin.x + cellSize/2f , cellSize);
                snappedZ = SnapToGridLine(worldPos.z, gridOrigin.z , cellSize);
            }

            Vector3 newPos = new Vector3(snappedX, knife.transform.position.y, snappedZ);
            knife.transform.position = newPos;

            if (cursorInstance != null)
                cursorInstance.transform.position = newPos + Vector3.up * 0.05f;
        }
    }

    void DragWithTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = mainCamera.ScreenPointToRay(touch.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);

                float snappedX = SnapToGridLine(worldPos.x, gridOrigin.x, cellSize);
                float snappedZ = SnapToGridLine(worldPos.z, gridOrigin.z, cellSize);

                Vector3 newPos = new Vector3(snappedX, knife.transform.position.y, snappedZ);
                knife.transform.position = newPos;

                if (cursorInstance != null)
                    cursorInstance.transform.position = newPos;
            }
        }
    }

}