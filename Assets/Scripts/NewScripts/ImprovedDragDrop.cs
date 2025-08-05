using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ImprovedDragDrop : MonoBehaviour
{
    private Camera mainCamera;
    public bool isDragging = false;
    public GridSystem gridSystem;
    private AdvancedGroupManager groupManager;

    private bool groupsReady = false;

    private Vector3 dragOffset;
    public float snapSpeed = 5f;

    // Grup hareket bilgileri
    private List<GameObject> currentGroup;
    private List<Vector3> groupStartPositions;
    private List<Vector3> groupTargetPositions;
    private List<Vector2Int> groupStartGridPositions;

    // Sürükleme referans noktası - DÜZELTİLDİ
    private Vector3 lastMouseWorldPos; // Son mouse dünya pozisyonu
    private int myIndexInGroup;

    // Y pozisyonlarını korumak için
    private List<float> groupStartYPositions;

    // Havaya kalkma için yeni değişkenler
    private List<float> groupOriginalYPositions; // Oyun başındaki orijinal Y pozisyonları
    private List<float> groupCurrentTargetY; // Şu anki hedef Y pozisyonları
    public float liftHeight = 0.5f; // Ne kadar havaya kalkacak
    public float liftSpeed = 3f; // Havaya kalkma hızı

    private void OnEnable()
    {
        if (groupManager == null)
            groupManager = FindObjectOfType<AdvancedGroupManager>();

        if (groupManager != null)
        {
            Debug.Log("DragDrop: Event'e abone olundu");
            groupManager.OnGroupsUpdated += OnGroupsReady;
        }
    }

    private void OnDisable()
    {
        if (groupManager != null)
        {
            groupManager.OnGroupsUpdated -= OnGroupsReady;
        }
    }

    private void OnGroupsReady(List<AdvancedGroupManager.CubeGroup> updatedGroups)
    {
        groupsReady = true;
    }

    void Start()
    {
        mainCamera = Camera.main;
        gridSystem = FindObjectOfType<GridSystem>();
        groupManager = FindObjectOfType<AdvancedGroupManager>();

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

        // Oyun başında orijinal Y pozisyonunu kaydet
        SaveOriginalYPosition();
    }

    /// <summary>
    /// Oyun başında bu küpün orijinal Y pozisyonunu kaydet
    /// </summary>
    private void SaveOriginalYPosition()
    {
        if (groupOriginalYPositions == null)
            groupOriginalYPositions = new List<float>();

        // Bu küpün grubunu bul ve tüm grubun orijinal Y pozisyonlarını kaydet
        // Şimdilik sadece bu küpün Y pozisyonunu kaydedelim, grup bulunduğunda tamamlanacak
    }

    void OnMouseDown()
    {
        if (!groupsReady)
        {
            Debug.LogWarning("Gruplar henüz hazır değil!");
            return;
        }
        // Bu küpün hangi grupta olduğunu bul
        currentGroup = FindGroupContaining(gameObject);

        if (currentGroup == null || currentGroup.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} herhangi bir grupta bulunamadı!");
            return;
        }

        // Bu küpün grup içindeki index'ini bul
        myIndexInGroup = currentGroup.IndexOf(gameObject);

        isDragging = true;

        // Mouse pozisyonunu grid seviyesinde dünya koordinatına çevir - DÜZELTİLDİ
        Plane plane = new Plane(Vector3.up, new Vector3(0, gridSystem.gridOrigin.y, 0));
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            dragOffset = transform.position - worldPos;
            lastMouseWorldPos = worldPos; // Son mouse pozisyonunu kaydet
        }

        // Grubun başlangıç pozisyonlarını kaydet
        groupStartPositions = new List<Vector3>();
        groupStartGridPositions = new List<Vector2Int>();
        groupStartYPositions = new List<float>();
        groupOriginalYPositions = new List<float>();
        groupCurrentTargetY = new List<float>();

        foreach (GameObject cube in currentGroup)
        {
            groupStartPositions.Add(cube.transform.position);
            groupStartYPositions.Add(cube.transform.position.y);
            groupOriginalYPositions.Add(cube.transform.position.y); // Orijinal Y pozisyonunu kaydet
            groupCurrentTargetY.Add(cube.transform.position.y + liftHeight); // Hedef Y pozisyonunu ayarla (havaya kalk)

            Vector2Int gridPos = gridSystem.GetGridIndexFromWorld(cube.transform.position);
            groupStartGridPositions.Add(gridPos);

            // Grid'de bu pozisyonu boş olarak işaretle (geçici)
            gridSystem.MarkCellEmpty(gridPos);
        }
    }

    void OnMouseUp()
    {
        if (!isDragging || currentGroup == null) return;

        isDragging = false;

        // Grup hedefi artık orijinal Y pozisyonları olacak (yere in)
        if (groupCurrentTargetY != null && groupOriginalYPositions != null)
        {
            for (int i = 0; i < groupCurrentTargetY.Count && i < groupOriginalYPositions.Count; i++)
            {
                groupCurrentTargetY[i] = groupOriginalYPositions[i];
            }
        }

        // Sürüklenen küpün (referans küpün) hedef grid pozisyonunu bul
        Vector2Int referenceTargetGrid = gridSystem.GetGridIndexFromWorld(currentGroup[myIndexInGroup].transform.position);

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
                Vector3 newTargetPos = new Vector3(cellCenter.x, groupOriginalYPositions[i], cellCenter.z); // Orijinal Y kullan
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
                Vector3 oldTargetPos = new Vector3(oldCellCenter.x, groupOriginalYPositions[i], oldCellCenter.z); // Orijinal Y kullan
                groupTargetPositions.Add(oldTargetPos);
            }

            // Eski grid pozisyonlarını tekrar işgal et
            for (int i = 0; i < groupStartGridPositions.Count; i++)
            {
                gridSystem.MarkCellOccupied(groupStartGridPositions[i]);
            }
        }

        // Highlight'ı kapat
        if (gridSystem.highlightPrefab != null)
            gridSystem.highlightPrefab.SetActive(false);
    }

    void Update()
    {
        if (isDragging && currentGroup != null)
        {
            // Mouse pozisyonunu takip et - DÜZELTİLDİ
            Plane plane = new Plane(Vector3.up, new Vector3(0, gridSystem.gridOrigin.y, 0));
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 currentMouseWorld = ray.GetPoint(distance);

                // Mouse'un hareket ettiği miktarı hesapla
                Vector3 mouseDelta = currentMouseWorld - lastMouseWorldPos;
                mouseDelta.y = 0; // Y eksenindeki hareketi sıfırla

                // Son mouse pozisyonunu güncelle
                lastMouseWorldPos = currentMouseWorld;

                // Grubun TÜM küplerini mouse delta'sı kadar hareket ettir
                for (int i = 0; i < currentGroup.Count; i++)
                {
                    Vector3 currentPos = currentGroup[i].transform.position;
                    Vector3 newPos = new Vector3(
                        currentPos.x + mouseDelta.x,
                        currentPos.y, // Y pozisyonunu koruy (havaya kalkma animasyonu için)
                        currentPos.z + mouseDelta.z
                    );
                    currentGroup[i].transform.position = newPos;
                }

                // Highlight göster (sürüklenen küp için en yakın grid pozisyonunu bul)
                Vector2Int nearestGrid = gridSystem.GetGridIndexFromWorld(currentGroup[myIndexInGroup].transform.position);

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
                if (distance > 0.1f)
                {
                    allReached = false;
                }
                else
                {
                    // Hedefe çok yaklaştıysa kesin oturt
                    currentGroup[i].transform.position = targetPos;
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
            }
        }

        // Havaya kalkma/inme animasyonu
        if (currentGroup != null && groupCurrentTargetY != null)
        {
            for (int i = 0; i < currentGroup.Count && i < groupCurrentTargetY.Count; i++)
            {
                Vector3 currentPos = currentGroup[i].transform.position;
                float targetY = groupCurrentTargetY[i];

                // Y pozisyonunu yumuşak bir şekilde hedef Y'ye doğru hareket ettir
                float newY = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * liftSpeed);
                currentGroup[i].transform.position = new Vector3(currentPos.x, newY, currentPos.z);
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
        Debug.Log($"Grid pozisyonu kontrol ediliyor: {gridPos}");
        if (!gridSystem.IsCellOccupied(gridPos))
            return false;
        Debug.Log($"Grid pozisyonu {gridPos} işgal edilmiş");

        // Eğer pozisyon işgal edilmişse, bu bizim grubumuzun bir küpü mü kontrol et
        Vector3 worldPos = gridSystem.GetCellCenter(gridPos);

        // Tüm grupları kontrol et
        foreach (var group in groupManager.groups)
        {
            // Kendi grubumuzsa pas geç
            if (group.cubes == currentGroup)
                continue;

            foreach (var cube in group.cubes)
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

        int groupId = groupManager.GetGroupIdForCube(cube);
        if (groupId == -1)
        {
            Debug.LogWarning($"{cube.name} hiçbir grupta bulunamadı (map'te yok)");
            return null;
        }

        var groupCubes = groupManager.GetGroupCubes(groupId);
        if (groupCubes == null || groupCubes.Count == 0)
        {
            Debug.LogWarning($"Grup {groupId} bulunamadı ya da boş");
            return null;
        }

        return groupCubes;
    }
}