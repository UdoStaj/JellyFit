using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DragDrop : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    public GridSystem gridSystem;
    private GroupManager groupManager;

    private Vector3 dragOffset;
    public float snapSpeed = 5f;

    // Grup hareket bilgileri
    private List<GameObject> currentGroup;
    private List<Vector3> groupStartPositions;
    private List<Vector3> groupTargetPositions;
    private List<Vector2Int> groupStartGridPositions;

    // Sürükleme referans noktası
    private Vector3 dragReferencePoint;
    private int myIndexInGroup;

    // Y pozisyonlarını korumak için
    private List<float> groupStartYPositions;

    void Start()
    {
        mainCamera = Camera.main;
        gridSystem = FindObjectOfType<GridSystem>();
        groupManager = FindObjectOfType<GroupManager>();

        if (gridSystem == null)
        {
            Debug.LogError($"{gameObject.name}: GridSystem bulunamadı!");
            enabled = false;
            return;
        }

        if (groupManager == null)
        {
            Debug.LogError($"{gameObject.name}: GroupManager bulunamadı!");
            enabled = false;
            return;
        }
    }

    void OnMouseDown()
    {
        // Bu küpün hangi grupta olduğunu bul
        currentGroup = FindGroupContaining(gameObject);

        if (currentGroup == null || currentGroup.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} herhangi bir grupta bulunamadı!");
            return;
        }

        // Bu küpün grup içindeki index'ini bul
        myIndexInGroup = currentGroup.IndexOf(gameObject);

        Debug.Log($"Sürükleme başlatılıyor. Grup büyüklüğü: {currentGroup.Count}, Benim index'im: {myIndexInGroup}");

        isDragging = true;

        // Mouse pozisyonunu dünya koordinatına çevir
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            dragOffset = transform.position - worldPos;
            dragReferencePoint = worldPos;
        }

        // Grubun başlangıç pozisyonlarını kaydet
        groupStartPositions = new List<Vector3>();
        groupStartGridPositions = new List<Vector2Int>();
        groupStartYPositions = new List<float>();

        foreach (GameObject cube in currentGroup)
        {
            groupStartPositions.Add(cube.transform.position);
            groupStartYPositions.Add(cube.transform.position.y);
            Vector2Int gridPos = gridSystem.GetGridIndexFromWorld(cube.transform.position);
            groupStartGridPositions.Add(gridPos);

            // Grid'de bu pozisyonu boş olarak işaretle (geçici)
            gridSystem.MarkCellEmpty(gridPos);
        }

        Debug.Log($"Grup başlangıç pozisyonları kaydedildi: {groupStartPositions.Count} küp");
    }

    void OnMouseUp()
    {
        if (!isDragging || currentGroup == null) return;

        isDragging = false;

        // Sürüklenen küpün (referans küpün) hedef grid pozisyonunu bul
        Vector2Int referenceTargetGrid = gridSystem.GetGridIndexFromWorld(currentGroup[myIndexInGroup].transform.position);

        Debug.Log($"Referans küp hedef grid pozisyonu: {referenceTargetGrid}");

        // Grubun tüm küplerinin yeni grid pozisyonlarını hesapla
        List<Vector2Int> newGroupGridPositions = new List<Vector2Int>();
        bool canPlaceGroup = true;

        // Grup içindeki her küp için yeni grid pozisyonunu hesapla
        for (int i = 0; i < currentGroup.Count; i++)
        {
            // Başlangıçtaki grid offset'ini hesapla
            Vector2Int startGridOffset = groupStartGridPositions[i] - groupStartGridPositions[myIndexInGroup];

            // Yeni grid pozisyonunu hesapla
            Vector2Int newGridPos = referenceTargetGrid + startGridOffset;

            Debug.Log($"Küp {i}: Başlangıç offset: {startGridOffset}, Yeni grid pos: {newGridPos}");

            // Bu pozisyon geçerli mi kontrol et
            if (!gridSystem.IsValidCell(newGridPos))
            {
                canPlaceGroup = false;
                Debug.Log($"Küp {i} için grid pozisyonu {newGridPos} geçersiz (grid sınırları dışında)");
                break;
            }

            // Bu pozisyon dolu mu kontrol et (sadece başka grupların küpleri için)
            if (IsCellOccupiedByOtherGroup(newGridPos, currentGroup))
            {
                canPlaceGroup = false;
                Debug.Log($"Küp {i} için grid pozisyonu {newGridPos} başka bir grup tarafından işgal edilmiş");
                break;
            }

            newGroupGridPositions.Add(newGridPos);
        }

        if (canPlaceGroup)
        {
            // Grup yerleştirilebilir - yeni pozisyonları uygula
            groupTargetPositions = new List<Vector3>();

            for (int i = 0; i < currentGroup.Count; i++)
            {
                Vector3 cellCenter = gridSystem.GetCellCenter(newGroupGridPositions[i]);
                Vector3 newTargetPos = new Vector3(cellCenter.x, groupStartYPositions[i], cellCenter.z);
                groupTargetPositions.Add(newTargetPos);

                // Grid'de yeni pozisyonu işgal et
                gridSystem.MarkCellOccupied(newGroupGridPositions[i]);
            }

            Debug.Log("Grup başarıyla yeni pozisyona yerleştirildi");
        }
        else
        {
            // Grup yerleştirilemez - eski pozisyonlara yumuşak geri dön
            groupTargetPositions = new List<Vector3>();

            for (int i = 0; i < currentGroup.Count; i++)
            {
                // Eski grid pozisyonunun tam merkez koordinatını al
                Vector3 oldCellCenter = gridSystem.GetCellCenter(groupStartGridPositions[i]);
                Vector3 oldTargetPos = new Vector3(oldCellCenter.x, groupStartYPositions[i], oldCellCenter.z);
                groupTargetPositions.Add(oldTargetPos);
            }

            // Eski grid pozisyonlarını tekrar işgal et
            for (int i = 0; i < groupStartGridPositions.Count; i++)
            {
                gridSystem.MarkCellOccupied(groupStartGridPositions[i]);
            }

            Debug.Log("Grup eski pozisyonuna yumuşak şekilde geri döndürülüyor");
        }

        // Highlight'ı kapat
        if (gridSystem.highlightPrefab != null)
            gridSystem.highlightPrefab.SetActive(false);
    }

    void Update()
    {
        if (isDragging && currentGroup != null)
        {
            // Mouse pozisyonunu takip et
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 currentMouseWorld = ray.GetPoint(distance);
                Vector3 movementDelta = currentMouseWorld - dragReferencePoint;
                movementDelta.y = 0;

                // Grubun TÜM küplerini aynı miktarda hareket ettir
                for (int i = 0; i < currentGroup.Count; i++)
                {
                    Vector3 newPos = new Vector3(
                        groupStartPositions[i].x + movementDelta.x,
                        groupStartYPositions[i],
                        groupStartPositions[i].z + movementDelta.z
                    );
                    currentGroup[i].transform.position = newPos;
                }

                // Highlight göster (sürüklenen küp için en yakın grid pozisyonunu bul)
                Vector3 referenceNewPos = new Vector3(
                    groupStartPositions[myIndexInGroup].x + movementDelta.x,
                    groupStartYPositions[myIndexInGroup],
                    groupStartPositions[myIndexInGroup].z + movementDelta.z
                );

                Vector2Int nearestGrid = gridSystem.GetGridIndexFromWorld(referenceNewPos);

                if (gridSystem.IsValidCell(nearestGrid))
                {
                    Vector3 cellCenter = gridSystem.GetCellCenter(nearestGrid);
                    if (gridSystem.highlightPrefab != null)
                    {
                        gridSystem.highlightPrefab.SetActive(true);
                        gridSystem.highlightPrefab.transform.position = new Vector3(
                            cellCenter.x,
                            cellCenter.y + 0.01f,
                            cellCenter.z
                        );
                    }
                }
                else
                {
                    if (gridSystem.highlightPrefab != null)
                        gridSystem.highlightPrefab.SetActive(false);
                }
            }
        }
        else if (!isDragging && groupTargetPositions != null && currentGroup != null)
        {
            // Grubun tüm küplerini hedef pozisyonlara doğru yumuşak hareket ettir
            bool allReached = true;

            for (int i = 0; i < currentGroup.Count && i < groupTargetPositions.Count; i++)
            {
                Vector3 currentPos = currentGroup[i].transform.position;
                Vector3 targetPos = groupTargetPositions[i];

                // Yumuşak geçiş için Lerp kullan - sabit hız
                Vector3 newPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * snapSpeed);
                currentGroup[i].transform.position = newPos;

                // Hedefe yaklaşım kontrolü
                float distance = Vector3.Distance(currentPos, targetPos);
                if (distance > 0.02f) // Daha yumuşak snap için threshold artırıldı
                {
                    allReached = false;
                }
            }

            // Tüm küpler hedeflerine ulaştı mı?
            if (allReached)
            {
                // Son pozisyonları kesin olarak ayarla
                for (int i = 0; i < currentGroup.Count && i < groupTargetPositions.Count; i++)
                {
                    currentGroup[i].transform.position = groupTargetPositions[i];
                }

                groupTargetPositions = null;
                Debug.Log("Grup hedef pozisyonuna başarıyla snap edildi");
            }
        }

        // Highlight yönetimi
        if (!isDragging && gridSystem.highlightPrefab != null && gridSystem.highlightPrefab.activeInHierarchy)
        {
            gridSystem.highlightPrefab.SetActive(false);
        }
    }

    /// <summary>
    /// Belirtilen grid pozisyonunun başka bir grup tarafından işgal edilip edilmediğini kontrol eder
    /// </summary>
    private bool IsCellOccupiedByOtherGroup(Vector2Int gridPos, List<GameObject> currentGroup)
    {
        // GridSystem'in temel işgal kontrolü
        if (!gridSystem.IsCellOccupied(gridPos))
            return false;

        // Eğer pozisyon işgal edilmişse, bu bizim grubumuzun bir küpü mü kontrol et
        Vector3 worldPos = gridSystem.GetCellCenter(gridPos);

        // Tüm grupları kontrol et
        foreach (var group in groupManager.groups)
        {
            // Kendi grubumuzsa pas geç
            if (group.SoapCubes == currentGroup)
                continue;

            foreach (var cube in group.SoapCubes)
            {
                Vector2Int cubeGridPos = gridSystem.GetGridIndexFromWorld(cube.transform.position);
                if (cubeGridPos == gridPos)
                {
                    Debug.Log($"Grid pozisyonu {gridPos} başka bir grup tarafından işgal edilmiş: {cube.name}");
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Verilen küpün hangi grupta olduğunu bulur
    /// </summary>
    private List<GameObject> FindGroupContaining(GameObject cube)
    {
        if (groupManager == null) return null;

        foreach (var group in groupManager.groups)
        {
            if (group.SoapCubes.Contains(cube))
            {
                Debug.Log($"{cube.name} küpü {group.SoapCubes.Count} küplük grupta bulundu");
                return group.SoapCubes;
            }
        }

        Debug.LogWarning($"{cube.name} hiçbir grupta bulunamadı!");
        return null;
    }

    /// <summary>
    /// Debug için - hangi grupta olduğunu göster
    /// </summary>
    [ContextMenu("Show My Group")]
    public void ShowMyGroup()
    {
        List<GameObject> myGroup = FindGroupContaining(gameObject);
        if (myGroup != null)
        {
            Debug.Log($"{gameObject.name} grubu ({myGroup.Count} küp):");
            foreach (var cube in myGroup)
            {
                Debug.Log($"  - {cube.name}");
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} herhangi bir grupta değil");
        }
    }
}