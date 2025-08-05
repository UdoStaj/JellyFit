    using UnityEngine;

public class KnifeCutSystem : MonoBehaviour
{
    [Header("Cut Settings")]
    public float cutDetectionRadius = 1f;
    public LayerMask soapLayerMask = -1;
    public LineRenderer cutLineRenderer;

    [Header("Visual Settings")]
    public Color cutLineColor = Color.red;
    public float cutLineWidth = 0.1f;
    public float cutLineDuration = 1f;

    [Header("Cut Feedback")]
    public bool showCutEffect = true;
    public GameObject cutEffectPrefab;
    public AudioClip cutSound;

    private Camera mainCamera;
    private bool isCutting = false;
    private Vector3 cutStartPosition;
    private Vector3 cutEndPosition;
    private SoapGroupManager groupManager;
    private AudioSource audioSource;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        groupManager = SoapGroupManager.Instance;
        if (groupManager == null)
        {
            Debug.LogError("SoapGroupManager not found!");
        }

        // AudioSource ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        if (cutLineRenderer == null)
        {
            GameObject lineObj = new GameObject("CutLine");
            lineObj.transform.SetParent(transform);
            cutLineRenderer = lineObj.AddComponent<LineRenderer>();
        }

        // Material oluþtur ve ata
        Material lineMaterial = CreateLineMaterial();
        cutLineRenderer.material = lineMaterial;

        // LineRenderer ayarlarý
        cutLineRenderer.startWidth = cutLineWidth;
        cutLineRenderer.endWidth = cutLineWidth;
        cutLineRenderer.positionCount = 2;
        cutLineRenderer.enabled = false;
        cutLineRenderer.useWorldSpace = true;

        // Shadow casting kapatýlabilir (performance için)
        cutLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cutLineRenderer.receiveShadows = false;
    }

    private Material CreateLineMaterial()
    {
        // Uyumlu shader kullan
        Shader lineShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        if (lineShader == null)
        {
            lineShader = Shader.Find("Sprites/Default");
        }
        if (lineShader == null)
        {
            lineShader = Shader.Find("Unlit/Color");
        }

        Material lineMat = new Material(lineShader);
        lineMat.color = cutLineColor;
        return lineMat;
    }

    private void Update()
    {
        HandleCutInput();
    }

    private void HandleCutInput()
    {
        // Mouse ile kesim - Sol mouse butonu ile baþla
        if (Input.GetMouseButtonDown(0))
        {
            StartCut();
        }

        // Mouse sürükleme sýrasýnda çizgiyi güncelle
        if (Input.GetMouseButton(0) && isCutting)
        {
            UpdateCutLine();
        }

        // Mouse býrakýldýðýnda kesimi tamamla
        if (Input.GetMouseButtonUp(0) && isCutting)
        {
            CompleteCut();
        }

        // Alternatif: Klavye ile hýzlý kesim (test için)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformQuickCut();
        }

        // Debug: C tuþu ile kesim bilgilerini göster
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShowCutDebugInfo();
        }
    }

    private void StartCut()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos != Vector3.zero)
        {
            isCutting = true;
            cutStartPosition = mouseWorldPos;
            cutEndPosition = mouseWorldPos;

            // Çizgiyi göster
            cutLineRenderer.enabled = true;
            UpdateCutLineRenderer();

            Debug.Log($"Kesim baþlatýldý: {cutStartPosition}");
        }
    }

    private void UpdateCutLine()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos != Vector3.zero)
        {
            cutEndPosition = mouseWorldPos;
            UpdateCutLineRenderer();
        }
    }

    private void UpdateCutLineRenderer()
    {
        cutLineRenderer.SetPosition(0, cutStartPosition);
        cutLineRenderer.SetPosition(1, cutEndPosition);
    }

    private void CompleteCut()
    {
        if (!isCutting) return;

        isCutting = false;

        float cutLength = Vector3.Distance(cutStartPosition, cutEndPosition);
        Debug.Log($"Kesim tamamlandý: {cutStartPosition} -> {cutEndPosition} (Uzunluk: {cutLength:F2})");

        // Kesim iþlemini gerçekleþtir
        int cutConnections = PerformCut(cutStartPosition, cutEndPosition);

        // Ses efekti çal
        if (cutConnections > 0 && cutSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cutSound);
        }

        // Görsel efekt göster
        if (showCutEffect && cutConnections > 0)
        {
            ShowCutEffect();
        }

        // Çizgiyi gizle
        Invoke(nameof(HideCutLine), cutLineDuration);
    }

    private void HideCutLine()
    {
        cutLineRenderer.enabled = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    private int PerformCut(Vector3 startPos, Vector3 endPos)
    {
        Vector3 cutDirection = (endPos - startPos).normalized;
        Vector3 cutCenter = (startPos + endPos) * 0.5f;
        float cutLength = Vector3.Distance(startPos, endPos);

        if (cutLength < 0.1f)
        {
            Debug.Log("Kesim çok kýsa, iþlem iptal edildi.");
            return 0;
        }

        Debug.Log($"Kesim iþlemi: Merkez={cutCenter}, Yön={cutDirection}, Uzunluk={cutLength}");

        // Kesim çizgisine yakýn SoapCube'larý bul
        SoapCube[] allCubes = FindObjectsOfType<SoapCube>();
        var nearbyCubes = new System.Collections.Generic.List<SoapCube>();

        foreach (var cube in allCubes)
        {
            if (cube != null)
            {
                float distanceToLine = DistanceFromPointToLine(cube.transform.position, startPos, endPos);
                if (distanceToLine < cutDetectionRadius)
                {
                    nearbyCubes.Add(cube);
                }
            }
        }

        Debug.Log($"Kesim alanýnda {nearbyCubes.Count} küp bulundu.");

        // Baðlantýlarý kontrol et ve kes
        int connectionsCut = 0;
        var cutConnections = new System.Collections.Generic.List<(SoapCube, SoapCube)>();

        for (int i = 0; i < nearbyCubes.Count; i++)
        {
            for (int j = i + 1; j < nearbyCubes.Count; j++)
            {
                var cubeA = nearbyCubes[i];
                var cubeB = nearbyCubes[j];

                if (cubeA.ConnectedCubes.Contains(cubeB))
                {
                    // Baðlantý çizgisinin kesim çizgisi ile kesiþip kesiþmediðini kontrol et
                    if (DoLinesIntersect(cubeA.transform.position, cubeB.transform.position, startPos, endPos))
                    {
                        cutConnections.Add((cubeA, cubeB));
                        connectionsCut++;
                    }
                }
            }
        }

        // Bulunan baðlantýlarý kes
        foreach (var (cubeA, cubeB) in cutConnections)
        {
            groupManager.CutConnection(cubeA, cubeB);
            Debug.Log($"Baðlantý kesildi: {cubeA.name} <-> {cubeB.name}");
        }

        if (connectionsCut == 0)
        {
            Debug.Log("Hiçbir baðlantý kesilemedi.");
        }
        else
        {
            Debug.Log($"Toplam {connectionsCut} baðlantý kesildi. Gruplar yeniden hesaplandý.");
        }

        return connectionsCut;
    }

    private void PerformQuickCut()
    {
        // Test için hýzlý kesim - ekranýn ortasýndan yatay bir kesim
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Vector3 leftPoint = new Vector3(Screen.width * 0.2f, Screen.height * 0.5f, 0);
        Vector3 rightPoint = new Vector3(Screen.width * 0.8f, Screen.height * 0.5f, 0);

        Ray leftRay = mainCamera.ScreenPointToRay(leftPoint);
        Ray rightRay = mainCamera.ScreenPointToRay(rightPoint);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(leftRay, out float leftDistance) &&
            groundPlane.Raycast(rightRay, out float rightDistance))
        {
            Vector3 worldLeft = leftRay.GetPoint(leftDistance);
            Vector3 worldRight = rightRay.GetPoint(rightDistance);

            cutStartPosition = worldLeft;
            cutEndPosition = worldRight;

            // Görsel efekt göster
            cutLineRenderer.enabled = true;
            UpdateCutLineRenderer();

            int cutConnections = PerformCut(worldLeft, worldRight);

            // Ses efekti
            if (cutConnections > 0 && cutSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cutSound);
            }

            // Çizgiyi gizle
            Invoke(nameof(HideCutLine), cutLineDuration);

            Debug.Log($"Hýzlý kesim yapýldý: {cutConnections} baðlantý kesildi");
        }
    }

    private void ShowCutEffect()
    {
        if (cutEffectPrefab != null)
        {
            Vector3 effectPosition = (cutStartPosition + cutEndPosition) * 0.5f;
            GameObject effect = Instantiate(cutEffectPrefab, effectPosition, Quaternion.identity);

            // Efekti otomatik olarak yok et
            Destroy(effect, 2f);
        }
    }

    private void ShowCutDebugInfo()
    {
        Debug.Log("=== KESÝM SÝSTEMÝ DEBUG BÝLGÝLERÝ ===");
        Debug.Log($"Aktif kesim: {isCutting}");
        Debug.Log($"Kesim algýlama yarýçapý: {cutDetectionRadius}");
        Debug.Log($"Son kesim pozisyonu: {cutStartPosition} -> {cutEndPosition}");

        if (groupManager != null)
        {
            Debug.Log($"Aktif grup sayýsý: {groupManager.ActiveGroupsCount}");
            Debug.Log($"Toplam küp sayýsý: {groupManager.TotalCubesCount}");
        }

        // Tüm soap cube'larýn baðlantý durumunu göster
        SoapCube[] allCubes = FindObjectsOfType<SoapCube>();
        Debug.Log($"Sahnedeki toplam SoapCube: {allCubes.Length}");

        foreach (var cube in allCubes)
        {
            if (cube != null)
            {
                Debug.Log($"{cube.name}: {cube.GetConnectionCount()} baðlantý, Grup: {(cube.Group != null ? cube.Group.name : "Yok")}");
            }
        }
        Debug.Log("=====================================");
    }

    private float DistanceFromPointToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDirection = (lineEnd - lineStart).normalized;
        Vector3 pointToStart = point - lineStart;

        // Noktanýn çizgi üzerindeki projektörlü pozisyonu
        float projection = Vector3.Dot(pointToStart, lineDirection);

        Vector3 closestPointOnLine;
        if (projection < 0)
        {
            closestPointOnLine = lineStart;
        }
        else if (projection > Vector3.Distance(lineStart, lineEnd))
        {
            closestPointOnLine = lineEnd;
        }
        else
        {
            closestPointOnLine = lineStart + lineDirection * projection;
        }

        return Vector3.Distance(point, closestPointOnLine);
    }

    private bool DoLinesIntersect(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
    {
        // 3D çizgilerin kesiþip kesiþmediðini kontrol et (XZ düzleminde)
        Vector2 p1 = new Vector2(line1Start.x, line1Start.z);
        Vector2 p2 = new Vector2(line1End.x, line1End.z);
        Vector2 p3 = new Vector2(line2Start.x, line2Start.z);
        Vector2 p4 = new Vector2(line2End.x, line2End.z);

        return DoLinesIntersect2D(p1, p2, p3, p4);
    }

    private bool DoLinesIntersect2D(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        if (Mathf.Abs(denominator) < 0.0001f)
        {
            return false; // Çizgiler paralel
        }

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }

    // Debug için görselleþtirme
    private void OnDrawGizmos()
    {
        if (isCutting)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(cutStartPosition, cutEndPosition);
            Gizmos.DrawWireSphere(cutStartPosition, 0.2f);
            Gizmos.DrawWireSphere(cutEndPosition, 0.2f);
        }

        // Kesim yarýçapýný göster
        Gizmos.color = Color.yellow;
        if (cutStartPosition != Vector3.zero && cutEndPosition != Vector3.zero)
        {
            Vector3 cutCenter = (cutStartPosition + cutEndPosition) * 0.5f;
            Gizmos.DrawWireSphere(cutCenter, cutDetectionRadius);
        }

        // Kesim alanýndaki küpleri vurgula
        if (isCutting)
        {
            SoapCube[] allCubes = FindObjectsOfType<SoapCube>();
            Gizmos.color = Color.cyan;

            foreach (var cube in allCubes)
            {
                if (cube != null)
                {
                    float distance = DistanceFromPointToLine(cube.transform.position, cutStartPosition, cutEndPosition);
                    if (distance < cutDetectionRadius)
                    {
                        Gizmos.DrawWireCube(cube.transform.position, Vector3.one * 1.2f);
                    }
                }
            }
        }
    }
}