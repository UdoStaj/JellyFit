//using System.Collections.Generic;
//using UnityEngine;

//public class CuttingSystem : MonoBehaviour
//{
//    [Header("Cut Settings")]
//    [Range(1, 10)]
//    public int cutLength = 2; // Kaç grid hücresi kesileceği
//    public LayerMask soapLayerMask = -1;

//    [Header("Highlight Settings")]
//    public GameObject highlightPrefab;
//    private List<GameObject> highlightInstances = new List<GameObject>();

//    private float cellSize;
//    private Vector3 gridOrigin;

//    private void Start()
//    {
//        // Grid sistem referansını al
//        var gridSystem = Object.FindFirstObjectByType<GridSystem>();
//        if (gridSystem == null)
//        {
//            Debug.LogError("CuttingSystem: GridSystem bulunamadı!");
//            enabled = false;
//            return;
//        }

//        gridOrigin = gridSystem.transform.position;
//        cellSize = gridSystem.cellSize;
//    }

//    /// <summary>
//    /// Kesilecek çizgileri highlight ile göster
//    /// </summary>
//    public void ShowCutPreview(Vector3 knifePosition, Vector3 knifeDirection)
//    {
//        // Önce mevcut highlight'ları temizle
//        ClearHighlights();

//        if (GridSystem.Instance == null || highlightPrefab == null) return;

//        // Kesim çizgilerini hesapla ve göster
//        List<Vector3> cutLinePositions = GetCutLinePositions(knifePosition, knifeDirection);

//        // Her kesim çizgisi pozisyonu için highlight oluştur
//        foreach (Vector3 linePos in cutLinePositions)
//        {
//            GameObject highlight = Instantiate(highlightPrefab, linePos + Vector3.up * 0.1f, Quaternion.identity);
//            highlightInstances.Add(highlight);
//        }
//    }

//    /// <summary>
//    /// Kesim çizgilerinin dünya pozisyonlarını hesaplar
//    /// </summary>
//    private List<Vector3> GetCutLinePositions(Vector3 knifePosition, Vector3 knifeDirection)
//    {
//        List<Vector3> linePositions = new List<Vector3>();

//        Vector2Int knifeGridPos = GridSystem.Instance.GetGridIndexFromWorld(knifePosition);
//        Vector3 cutDirection = GetCutDirection(knifeDirection);
//        if (cutDirection == Vector3.zero) return linePositions;

//        List<Vector2Int> cutPositions = GetCutPositionsFromKnife(knifeGridPos, cutDirection);

//        foreach (Vector2Int gridPos in cutPositions)
//        {
//            if (GridSystem.Instance.IsValidCell(gridPos))
//            {
//                Vector3 cellCenter = GridSystem.Instance.GetCellCenter(gridPos);
//                Vector3 cutLinePos = GetCutLinePosition(cellCenter, cutDirection, gridPos, knifeGridPos);
//                linePositions.Add(cutLinePos);
//            }
//        }

//        return linePositions;
//    }

//    /// <summary>
//    /// Belirli bir cell için kesim çizgisinin pozisyonunu hesaplar
//    /// </summary>
//    private Vector3 GetCutLinePosition(Vector3 cellCenter, Vector3 cutDirection, Vector2Int cellGridPos, Vector2Int knifeGridPos)
//    {
//        float halfCell = cellSize * 0.5f;

//        if (cutDirection == Vector3.right || cutDirection == Vector3.left)
//        {
//            if (knifeGridPos.x > cellGridPos.x)
//                return new Vector3(cellCenter.x + halfCell, cellCenter.y, cellCenter.z);
//            else
//                return new Vector3(cellCenter.x - halfCell, cellCenter.y, cellCenter.z);
//        }
//        else if (cutDirection == Vector3.forward || cutDirection == Vector3.back)
//        {
//            if (knifeGridPos.y > cellGridPos.y)
//                return new Vector3(cellCenter.x, cellCenter.y, cellCenter.z + halfCell);
//            else
//                return new Vector3(cellCenter.x, cellCenter.y, cellCenter.z - halfCell);
//        }

//        return cellCenter;
//    }

//    /// <summary>
//    /// Highlight'ları temizle
//    /// </summary>
//    public void ClearHighlights()
//    {
//        foreach (GameObject highlight in highlightInstances)
//        {
//            if (highlight != null)
//                DestroyImmediate(highlight);
//        }
//        highlightInstances.Clear();
//    }

//    /// <summary>
//    /// Verilen pozisyon ve yönde kesim işlemi gerçekleştirir
//    /// </summary>
//    /// <param name="knifePosition">Bıçağın dünya pozisyonu</param>
//    /// <param name="knifeDirection">Bıçağın yönü (transform.right)</param>
//    public void PerformCut(Vector3 knifePosition, Vector3 knifeDirection)
//    {
//        Debug.Log($"=== CuttingSystem.PerformCut ÇAĞRILDI ===");
//        Debug.Log($"Kesim başlatılıyor - Pozisyon: {knifePosition}, Bıçak Yönü: {knifeDirection}");
//        Debug.Log($"Kesim uzunluğu: {cutLength} grid hücresi");

//        // Highlight'ları temizle
//        ClearHighlights();

//        if (GridSystem.Instance == null)
//        {
//            Debug.LogError("GridSystem.Instance NULL!");
//            return;
//        }

//        // Bıçağın grid pozisyonunu bul
//        Vector2Int knifeGridPos = GridSystem.Instance.GetGridIndexFromWorld(knifePosition);
//        Debug.Log($"Bıçak grid pozisyonu: {knifeGridPos}");

//        // Bıçağın yönüne göre kesim yönünü belirle
//        Vector3 cutDirection = GetCutDirection(knifeDirection);
//        Debug.Log($"Belirlenmiş kesim yönü: {cutDirection}");

//        if (cutDirection == Vector3.zero)
//        {
//            Debug.LogWarning("Geçerli kesim yönü bulunamadı!");
//            return;
//        }

//        // Kesim pozisyonlarını hesapla - bıçak pozisyonundan başlayarak
//        List<Vector2Int> cutPositions = GetCutPositionsFromKnife(knifeGridPos, cutDirection);
//        Debug.Log($"Kesim pozisyonları: {cutPositions.Count} adet");

//        // Bu pozisyonlardaki küpleri ve bağlantılarını kes
//        CutAtPositions(cutPositions, cutDirection);

//        Debug.Log("Kesim işlemi tamamlandı");
//    }

//    /// <summary>
//    /// Bıçak pozisyonundan başlayarak kesim pozisyonlarını hesaplar
//    /// </summary>
//    private List<Vector2Int> GetCutPositionsFromKnife(Vector2Int knifeGridPos, Vector3 cutDirection)
//    {
//        List<Vector2Int> positions = new List<Vector2Int>();

//        // Kesim yönünü grid koordinatlarına çevir
//        Vector2Int gridDirection = Vector2Int.zero;
//        if (cutDirection == Vector3.right) gridDirection = Vector2Int.right;
//        else if (cutDirection == Vector3.left) gridDirection = Vector2Int.left;
//        else if (cutDirection == Vector3.forward) gridDirection = Vector2Int.up;
//        else if (cutDirection == Vector3.back) gridDirection = Vector2Int.down;

//        Debug.Log($"Grid direction: {gridDirection}");

//        // Bıçak pozisyonundan başlayarak kesim uzunluğu kadar pozisyon oluştur
//        for (int i = 0; i < cutLength; i++)
//        {
//            Vector2Int cutPos = knifeGridPos + (gridDirection * i);

//            if (GridSystem.Instance.IsValidCell(cutPos))
//            {
//                positions.Add(cutPos);
//                Debug.Log($"Kesim pozisyonu eklendi: {cutPos}");
//            }
//            else
//            {
//                Debug.Log($"Geçersiz pozisyon: {cutPos}");
//                break; // Grid dışına çıktıysak durur
//            }
//        }

//        return positions;
//    }

//    /// <summary>
//    /// Bıçağın yönüne göre kesim yönünü belirler (DÜZELTME: Kesim yönü bıçağa dik olmalı)
//    /// </summary>
//    private Vector3 GetCutDirection(Vector3 knifeDirection)
//    {
//        // Bıçağın yönünü normalize et ve Y'yi sıfırla
//        knifeDirection.y = 0;
//        knifeDirection = knifeDirection.normalized;

//        Debug.Log($"Normalize edilmiş bıçak yönü: {knifeDirection}");

//        // DÜZELTME: Kesim yönü bıçağa dik olmalı, bıçakla aynı yönde değil!
//        // Bıçak sağa bakıyorsa, kesim ileri-geri yönünde olmalı
//        float absX = Mathf.Abs(knifeDirection.x);
//        float absZ = Mathf.Abs(knifeDirection.z);

//        if (absX > absZ)
//        {
//            // Bıçak X yönünde bakıyor -> Z yönünde kes (bıçağa dik)
//            return Vector3.forward; // Her zaman forward yönünde kes
//        }
//        else
//        {
//            // Bıçak Z yönünde bakıyor -> X yönünde kes (bıçağa dik)
//            return Vector3.right; // Her zaman right yönünde kes
//        }
//    }

//    /// <summary>
//    /// Kesim pozisyonlarını hesaplar
//    /// </summary>
//    private List<Vector2Int> GetCutPositions(Vector2Int startPos, Vector3 cutDirection)
//    {
//        List<Vector2Int> positions = new List<Vector2Int>();

//        // Kesim yönünü grid koordinatlarına çevir
//        Vector2Int gridDirection = Vector2Int.zero;
//        if (cutDirection == Vector3.right) gridDirection = Vector2Int.right;
//        else if (cutDirection == Vector3.left) gridDirection = Vector2Int.left;
//        else if (cutDirection == Vector3.forward) gridDirection = Vector2Int.up;
//        else if (cutDirection == Vector3.back) gridDirection = Vector2Int.down;

//        Debug.Log($"Grid direction: {gridDirection}");

//        // Kesim uzunluğu kadar pozisyon oluştur
//        for (int i = 0; i < cutLength; i++)
//        {
//            Vector2Int cutPos = startPos + (gridDirection * i);

//            if (GridSystem.Instance.IsValidCell(cutPos))
//            {
//                positions.Add(cutPos);
//                Debug.Log($"Kesim pozisyonu eklendi: {cutPos}");
//            }
//            else
//            {
//                Debug.Log($"Geçersiz pozisyon: {cutPos}");
//            }
//        }

//        return positions;
//    }

//    /// <summary>
//    /// Belirtilen pozisyonlarda kesim yapar - sadece kesim çizgisindeki bağlantıları kes
//    /// </summary>
//    private void CutAtPositions(List<Vector2Int> cutPositions, Vector3 cutDirection)
//    {
//        int totalCuts = 0;

//        // Kesim çizgisine dik olan yönleri belirle
//        Vector3[] cutDirections = GetPerpendicularDirections(cutDirection);

//        Debug.Log($"Kesim yapılacak yönler: {string.Join(", ", cutDirections)}");

//        foreach (Vector2Int gridPos in cutPositions)
//        {
//            if (GridSystem.Instance.IsCellOccupied(gridPos))
//            {
//                GameObject cube = GridSystem.Instance.GetCellObject(gridPos);
//                if (cube != null && cube.CompareTag("SoapCube"))
//                {
//                    Debug.Log($"Küp kontrol ediliyor: {cube.name} ({gridPos})");

//                    CubeGroupControl cubeControl = cube.GetComponent<CubeGroupControl>();
//                    if (cubeControl != null)
//                    {
//                        // Sadece kesim yönüne dik olan bağlantıları kes
//                        foreach (Vector3 dir in cutDirections)
//                        {
//                            // Bu yönde komşu var mı ve bağlı mı kontrol et
//                            if (cubeControl.neighbors.ContainsKey(dir) &&
//                                cubeControl.neighborIsAttached.ContainsKey(dir) &&
//                                cubeControl.neighborIsAttached[dir])
//                            {
//                                GameObject neighbor = cubeControl.neighbors[dir];
//                                if (neighbor != null)
//                                {
//                                    // Komşu küpün pozisyonunu kontrol et - kesim çizgisinin diğer tarafında mı?
//                                    Vector2Int neighborGridPos = GridSystem.Instance.GetGridIndexFromWorld(neighbor.transform.position);

//                                    if (ShouldCutConnection(gridPos, neighborGridPos, cutDirection, dir))
//                                    {
//                                        CutConnection(cubeControl, dir, neighbor);
//                                        totalCuts++;
//                                        Debug.Log($"✂️ Bağlantı kesildi: {cube.name} <-> {neighbor.name} ({dir})");
//                                    }
//                                    else
//                                    {
//                                        Debug.Log($"❌ Bağlantı kesilmedi (kesim çizgisinde değil): {cube.name} <-> {neighbor.name} ({dir})");
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        Debug.Log($"Toplam {totalCuts} bağlantı kesildi");
//    }

//    /// <summary>
//    /// Bu bağlantının kesilip kesilmeyeceğini belirler
//    /// </summary>
//    private bool ShouldCutConnection(Vector2Int cubePos, Vector2Int neighborPos, Vector3 cutDirection, Vector3 connectionDirection)
//    {
//        // Kesim yönüne göre hangi bağlantıların kesileceğini belirle
//        if (cutDirection == Vector3.right || cutDirection == Vector3.left)
//        {
//            // X yönünde kesim yapıyoruz, Z yönündeki (forward/back) bağlantıları kes
//            if (connectionDirection == Vector3.forward)
//            {
//                // Forward bağlantısı - komşu yukarıda mı?
//                return neighborPos.y > cubePos.y;
//            }
//            else if (connectionDirection == Vector3.back)
//            {
//                // Back bağlantısı - komşu aşağıda mı?
//                return neighborPos.y < cubePos.y;
//            }
//        }
//        else if (cutDirection == Vector3.forward || cutDirection == Vector3.back)
//        {
//            // Z yönünde kesim yapıyoruz, X yönündeki (right/left) bağlantıları kes
//            if (connectionDirection == Vector3.right)
//            {
//                // Right bağlantısı - komşu sağda mı?
//                return neighborPos.x > cubePos.x;
//            }
//            else if (connectionDirection == Vector3.left)
//            {
//                // Left bağlantısı - komşu solda mı?
//                return neighborPos.x < cubePos.x;
//            }
//        }

//        return false;
//    }

//    /// <summary>
//    /// Kesim yönüne dik olan yönleri döndürür
//    /// </summary>
//    private Vector3[] GetPerpendicularDirections(Vector3 cutDirection)
//    {
//        if (cutDirection == Vector3.forward || cutDirection == Vector3.back)
//        {
//            // Z yönünde kesim yapıyoruz, X yönündeki bağlantıları kes
//            return new Vector3[] { Vector3.left, Vector3.right };
//        }
//        else if (cutDirection == Vector3.right || cutDirection == Vector3.left)
//        {
//            // X yönünde kesim yapıyoruz, Z yönündeki bağlantıları kes
//            return new Vector3[] { Vector3.forward, Vector3.back };
//        }

//        return new Vector3[0];
//    }

//    /// <summary>
//    /// İki küp arasındaki bağlantıyı keser
//    /// </summary>
//    private void CutConnection(CubeGroupControl cubeControl, Vector3 direction, GameObject neighbor)
//    {
//        // Bu küpün bağlantısını kes
//        if (cubeControl.neighborIsAttached.ContainsKey(direction))
//        {
//            cubeControl.neighborIsAttached[direction] = false;
//        }

//        // Komşu küpün ters yönlü bağlantısını da kes
//        CubeGroupControl neighborControl = neighbor.GetComponent<CubeGroupControl>();
//        if (neighborControl != null)
//        {
//            Vector3 oppositeDirection = -direction;
//            if (neighborControl.neighborIsAttached.ContainsKey(oppositeDirection))
//            {
//                neighborControl.neighborIsAttached[oppositeDirection] = false;
//            }
//        }
//    }

//    /// <summary>
//    /// Manuel test için kesim işlemi
//    /// </summary>
//    [ContextMenu("Test Cut")]
//    public void TestCut()
//    {
//        // Test amaçlı - orta noktadan sağa doğru kesim
//        Vector3 testPosition = GridSystem.Instance.GetCellCenter(new Vector2Int(GridSystem.Instance.gridWidth / 2, GridSystem.Instance.gridHeight / 2));
//        Vector3 testDirection = Vector3.right;

//        PerformCut(testPosition, testDirection);
//    }
//}