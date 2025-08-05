using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoapGroupManager : MonoBehaviour
{
    public static SoapGroupManager Instance { get; private set; }

    [Header("Group Management")]
    public float recalculateDelay = 0.1f;
    public bool debugMode = true;

    [Header("Visual Settings")]
    public bool useRandomGroupColors = true;
    public Color[] predefinedColors = {
        Color.red, Color.green, Color.blue, Color.yellow,
        Color.magenta, Color.cyan, Color.white
    };

    private List<SoapCube> allCubes = new List<SoapCube>();
    private List<SoapGroup> activeGroups = new List<SoapGroup>();
    private bool needsRecalculation = false;
    private int colorIndex = 0;

    public List<SoapGroup> ActiveGroups => activeGroups;
    public int TotalCubesCount => allCubes.Count;
    public int ActiveGroupsCount => activeGroups.Count;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Sahne baþladýðýnda tüm gruplarý yeniden hesapla
        Invoke(nameof(RecalculateAllGroups), 0.5f);
    }

    public void RegisterCube(SoapCube cube)
    {
        if (cube != null && !allCubes.Contains(cube))
        {
            allCubes.Add(cube);
            needsRecalculation = true;

            if (debugMode)
            {
                Debug.Log($"SoapCube kaydedildi: {cube.name} (Toplam: {allCubes.Count})");
            }

            if (!IsInvoking(nameof(DelayedRecalculate)))
            {
                Invoke(nameof(DelayedRecalculate), recalculateDelay);
            }
        }
    }

    public void UnregisterCube(SoapCube cube)
    {
        if (allCubes.Contains(cube))
        {
            allCubes.Remove(cube);
            needsRecalculation = true;

            if (debugMode)
            {
                Debug.Log($"SoapCube kaydý silindi: {cube.name} (Kalan: {allCubes.Count})");
            }

            if (!IsInvoking(nameof(DelayedRecalculate)))
            {
                Invoke(nameof(DelayedRecalculate), recalculateDelay);
            }
        }
    }

    private void DelayedRecalculate()
    {
        if (needsRecalculation)
        {
            RecalculateAllGroups();
            needsRecalculation = false;
        }
    }

    public void RecalculateAllGroups()
    {
        if (debugMode)
        {
            Debug.Log("=== SoapGroupManager: Gruplar yeniden hesaplanýyor ===");
        }

        // Mevcut gruplarý temizle
        ClearAllGroups();

        // Tüm küpleri temizle
        foreach (var cube in allCubes)
        {
            if (cube != null)
            {
                cube.SetGroup(null);
            }
        }

        // Yeni gruplarý oluþtur
        HashSet<SoapCube> processedCubes = new HashSet<SoapCube>();
        colorIndex = 0; // Renk indeksini sýfýrla

        foreach (var cube in allCubes)
        {
            if (cube != null && !processedCubes.Contains(cube))
            {
                SoapGroup newGroup = CreateNewGroup();
                FloodFillGroup(cube, newGroup, processedCubes);

                if (newGroup.MemberCount > 0)
                {
                    activeGroups.Add(newGroup);

                    if (debugMode)
                    {
                        Debug.Log($"Yeni grup oluþturuldu: {newGroup.name} - {newGroup.MemberCount} üye - Renk: {newGroup.groupColor}");
                    }
                }
                else
                {
                    Destroy(newGroup.gameObject);
                }
            }
        }

        if (debugMode)
        {
            Debug.Log($"=== Grup hesaplama tamamlandý: {activeGroups.Count} grup, {allCubes.Count} toplam küp ===");
            PrintGroupSummary();
        }
    }

    private void FloodFillGroup(SoapCube startCube, SoapGroup group, HashSet<SoapCube> processedCubes)
    {
        Queue<SoapCube> queue = new Queue<SoapCube>();
        queue.Enqueue(startCube);
        processedCubes.Add(startCube);
        group.AddMember(startCube);

        while (queue.Count > 0)
        {
            SoapCube current = queue.Dequeue();

            foreach (var neighbor in current.ConnectedCubes)
            {
                if (neighbor != null && !processedCubes.Contains(neighbor))
                {
                    processedCubes.Add(neighbor);
                    group.AddMember(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private SoapGroup CreateNewGroup()
    {
        GameObject groupObj = new GameObject($"SoapGroup_{activeGroups.Count + 1}");
        groupObj.transform.SetParent(transform);

        SoapGroup group = groupObj.AddComponent<SoapGroup>();

        // Renk ata
        if (useRandomGroupColors)
        {
            group.groupColor = GetNextGroupColor();
        }

        return group;
    }

    private Color GetNextGroupColor()
    {
        if (predefinedColors.Length > 0)
        {
            Color color = predefinedColors[colorIndex % predefinedColors.Length];
            colorIndex++;
            return color;
        }
        else
        {
            // Random renk üret
            return new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                1f
            );
        }
    }

    private void ClearAllGroups()
    {
        foreach (var group in activeGroups)
        {
            if (group != null)
            {
                Destroy(group.gameObject);
            }
        }
        activeGroups.Clear();
    }

    public void CutConnection(SoapCube cubeA, SoapCube cubeB)
    {
        if (cubeA == null || cubeB == null) return;

        if (debugMode)
        {
            Debug.Log($"=== Baðlantý kesiliyor: {cubeA.name} <-> {cubeB.name} ===");
        }

        // Baðlantýyý kes
        cubeA.RemoveConnection(cubeB);
        cubeB.RemoveConnection(cubeA);

        // Gruplarý yeniden hesapla
        RecalculateAllGroups();
    }

    public void CutConnectionAtPosition(Vector3 cutPosition, Vector3 cutDirection)
    {
        if (debugMode)
        {
            Debug.Log($"Pozisyonda kesim yapýlýyor: {cutPosition}, Yön: {cutDirection}");
        }

        // Kesim pozisyonuna yakýn küpleri bul
        List<SoapCube> nearbyCubes = new List<SoapCube>();

        foreach (var cube in allCubes)
        {
            if (cube != null)
            {
                float distance = Vector3.Distance(cube.transform.position, cutPosition);
                if (distance < 2f) // 2 birim yakýnlýktaki küpler
                {
                    nearbyCubes.Add(cube);
                }
            }
        }

        if (debugMode)
        {
            Debug.Log($"Kesim alanýnda {nearbyCubes.Count} küp bulundu");
        }

        // Kesim çizgisinin geçtiði baðlantýlarý bul ve kes
        int connectionsCut = 0;

        for (int i = 0; i < nearbyCubes.Count; i++)
        {
            for (int j = i + 1; j < nearbyCubes.Count; j++)
            {
                var cubeA = nearbyCubes[i];
                var cubeB = nearbyCubes[j];

                if (cubeA.ConnectedCubes.Contains(cubeB))
                {
                    // Baðlantý çizgisinin kesim çizgisi ile kesiþip kesiþmediðini kontrol et
                    if (DoesLineIntersectCut(cubeA.transform.position, cubeB.transform.position, cutPosition, cutDirection))
                    {
                        CutConnection(cubeA, cubeB);
                        connectionsCut++;
                    }
                }
            }
        }

        if (debugMode)
        {
            if (connectionsCut == 0)
            {
                Debug.Log("Kesim pozisyonunda hiçbir baðlantý bulunamadý.");
            }
            else
            {
                Debug.Log($"Toplam {connectionsCut} baðlantý kesildi.");
            }
        }
    }

    private bool DoesLineIntersectCut(Vector3 lineStart, Vector3 lineEnd, Vector3 cutPosition, Vector3 cutDirection)
    {
        // Basit kesiþim kontrolü - kesim çizgisinin baðlantý çizgisini kesip kesmediðini kontrol et
        Vector3 lineDir = (lineEnd - lineStart).normalized;
        Vector3 toCut = (cutPosition - lineStart);

        float dot = Vector3.Dot(lineDir, cutDirection.normalized);
        float distance = Vector3.Cross(toCut, lineDir).magnitude;

        // Eðer çizgiler paralele yakýnsa ve mesafe küçükse kesiþim var
        return Mathf.Abs(dot) < 0.8f && distance < 0.5f;
    }

    // Grup özetini yazdýr (debug için)
    private void PrintGroupSummary()
    {
        Debug.Log("=== GRUP ÖZETÝ ===");
        for (int i = 0; i < activeGroups.Count; i++)
        {
            var group = activeGroups[i];
            if (group != null)
            {
                Debug.Log($"Grup {i + 1}: {group.MemberCount} üye - Merkez: {group.GetGroupCenter()}");

                // Grup üyelerini listele
                string memberNames = string.Join(", ", group.Members.Where(m => m != null).Select(m => m.name));
                Debug.Log($"  Üyeler: {memberNames}");
            }
        }
        Debug.Log("================");
    }

    // Belirli bir küpün hangi grupta olduðunu bul
    public SoapGroup GetGroupContaining(SoapCube cube)
    {
        if (cube == null) return null;

        foreach (var group in activeGroups)
        {
            if (group != null && group.ContainsCube(cube))
            {
                return group;
            }
        }

        return null;
    }

    // Ýki küpün ayný grupta olup olmadýðýný kontrol et
    public bool AreInSameGroup(SoapCube cubeA, SoapCube cubeB)
    {
        if (cubeA == null || cubeB == null) return false;

        var groupA = GetGroupContaining(cubeA);
        var groupB = GetGroupContaining(cubeB);

        return groupA != null && groupB != null && groupA == groupB;
    }

    // En büyük grubu bul
    public SoapGroup GetLargestGroup()
    {
        SoapGroup largest = null;
        int maxSize = 0;

        foreach (var group in activeGroups)
        {
            if (group != null && group.MemberCount > maxSize)
            {
                maxSize = group.MemberCount;
                largest = group;
            }
        }

        return largest;
    }

    // Tek baþýna kalan küpleri bul
    public List<SoapCube> GetSingletonCubes()
    {
        List<SoapCube> singletons = new List<SoapCube>();

        foreach (var group in activeGroups)
        {
            if (group != null && group.MemberCount == 1)
            {
                singletons.AddRange(group.Members);
            }
        }

        return singletons;
    }

    // Manuel grup yeniden hesaplama (Debug için)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void ForceRecalculate()
    {
        Debug.Log("Manuel grup yeniden hesaplama baþlatýldý");
        RecalculateAllGroups();
    }

    // Debug için
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ForceRecalculate();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            PrintGroupSummary();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Debug için görsel gösterim
    private void OnDrawGizmos()
    {
        if (activeGroups == null) return;

        // Her grup için merkez noktasýný ve baðlantýlarý göster
        foreach (var group in activeGroups)
        {
            if (group != null && group.MemberCount > 1)
            {
                Gizmos.color = group.groupColor;
                Vector3 center = group.GetGroupCenter();

                // Grup merkezini göster
                Gizmos.DrawWireSphere(center, 0.8f);

                // Grup üyelerini merkeze baðla
                foreach (var member in group.Members)
                {
                    if (member != null)
                    {
                        Gizmos.DrawLine(center, member.transform.position);
                    }
                }
            }
        }
    }
}